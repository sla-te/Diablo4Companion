"""HTTP layer for diablo.trade.

What is and is not available, established by watching the site's own traffic:

* Reading is clean REST and works anonymously:
    GET /api/search/<shortId>      -> the saved search plus its listing ids
    GET /api/listing/get?ids=a,b,c -> full listing records, batched
* Writing (creating a search, creating a listing) is NOT REST. Next.js Server
  Actions handle it: a POST to the page URL carrying a `Next-Action` header
  whose value is a build-specific 40-hex id. See `diablotrade.actions`.

Transport is `curl_cffi` rather than `urllib`, for four reasons that all came
out of debugging the Server Action path:

1. It replays a real browser's TLS/JA3 and HTTP2 fingerprint (`impersonate`).
   The site is behind a CDN that is entitled to start refusing clients that
   announce Chrome in the User-Agent while handshaking like Python.
2. Non-2xx responses carry their body instead of being raised away. Next.js
   puts the cause of a Server Action failure in that body.
3. Redirects can be inspected rather than followed. The action signals an
   unauthenticated caller with a 303 plus an `X-Action-Redirect` header, which
   is invisible if the client chases the redirect.
4. One session keeps a cookie jar, so cookies the site sets survive across the
   page GET and the action POST.
"""

from __future__ import annotations

import json
import time
import urllib.parse
from collections.abc import Iterable, Iterator
from dataclasses import dataclass, field
from typing import Protocol, Self, cast

from curl_cffi import CurlError
from curl_cffi.requests import BrowserTypeLiteral, Response, Session
from curl_cffi.requests.session import HttpMethod

from .models import Json, Listing, SavedSearch

BASE_URL = "https://diablo.trade"

# The site itself requests listing ids in batches of this size. Going wider
# risks a URL longer than the CDN will accept.
BATCH_SIZE = 50

# curl_cffi resolves the bare name to its newest Chrome profile, which is what
# we want: pinning a version means the fingerprint ages into being distinctive.
IMPERSONATE: BrowserTypeLiteral = "chrome"

# Error bodies are for a human reading a traceback, not for parsing. Long
# enough to hold a Next.js stack frame, short enough not to flood a terminal.
_ERROR_BODY_LIMIT = 2000


class _SessionRequester(Protocol):
    """The slice of curl_cffi's Session this client actually depends on.

    curl_cffi ships no type stubs, so `Session.request` is only partially known
    and every call site inherits that. Declaring the four arguments we pass
    keeps the gap in one cast, and documents the dependency: if curl_cffi ever
    changes this shape, one place fails rather than nothing failing.
    """

    def request(
        self,
        method: HttpMethod,
        url: str,
        *,
        data: bytes | None,
        headers: dict[str, str] | None,
        timeout: float,
        allow_redirects: bool,
    ) -> Response | None: ...


class DiabloTradeError(RuntimeError):
    """Any failure talking to diablo.trade."""


class AuthRequiredError(DiabloTradeError):
    """The endpoint refused an anonymous request."""


@dataclass(slots=True)
class Client:
    """Talks to diablo.trade.

    `cookie` is a raw Cookie header string, only needed for authenticated
    operations (creating a search, posting, own listings). Search and listing
    reads work without it.

    Holds a connection pool, so reuse one client rather than making them per
    call, and close it when done - or use it as a context manager.
    """

    cookie: str | None = None
    base_url: str = BASE_URL
    timeout: float = 30.0
    # Be a polite client: the read endpoints are not rate-limit documented, and
    # a full 500-listing pull is 10 requests.
    delay_between_requests: float = 0.2
    impersonate: BrowserTypeLiteral = IMPERSONATE
    _session: Session[Response] | None = field(
        default=None, init=False, repr=False, compare=False
    )

    def _ensure_session(self) -> Session[Response]:
        """Build the session on first use, so constructing a Client is cheap."""
        if self._session is None:
            session: Session[Response] = Session(impersonate=self.impersonate)
            # Seeding the jar rather than pinning a Cookie header lets the site
            # update its own cookies without us sending a stale duplicate.
            for part in (self.cookie or "").split(";"):
                name, _, value = part.strip().partition("=")
                if name:
                    session.cookies.set(name, value)
            self._session = session
        return self._session

    def close(self) -> None:
        if self._session is not None:
            self._session.close()
            self._session = None

    def __enter__(self) -> Self:
        return self

    def __exit__(self, *_exc: object) -> None:
        self.close()

    def _request(
        self,
        path: str,
        *,
        method: HttpMethod = "GET",
        body: bytes | None = None,
        headers: dict[str, str] | None = None,
    ) -> bytes:
        url = path if path.startswith("http") else self.base_url + path
        # `fetch_text` is handed URLs scraped out of page HTML, so the scheme is
        # attacker-influenced: pin it rather than trusting whatever comes back.
        if urllib.parse.urlsplit(url).scheme not in ("http", "https"):
            raise DiabloTradeError(f"refusing non-HTTP URL {url!r}")
        try:
            requester = cast(_SessionRequester, self._ensure_session())
            response = requester.request(
                method,
                url,
                data=body,
                headers=headers,
                timeout=self.timeout,
                # Following a redirect would swallow the response that says why
                # the request was redirected. See the module docstring.
                allow_redirects=False,
            )
        except CurlError as exc:
            raise DiabloTradeError(f"{method} {path} failed: {exc}") from exc
        if response is None:
            # Only reachable via the streaming/callback modes, which we never use.
            raise DiabloTradeError(f"{method} {path} returned no response")
        if response.status_code >= 300:
            raise _failure(method, path, response)
        content: bytes = response.content
        return content

    def fetch_text(self, path: str) -> str:
        """GET a page or asset as text. Used for Server Action id discovery."""
        return self._request(path).decode("utf-8", errors="replace")

    def post_raw(self, path: str, body: bytes, headers: dict[str, str]) -> str:
        """POST arbitrary bytes and return the response as text.

        Exists for the Server Action transport in `diablotrade.actions`, which
        needs a non-JSON request and a non-JSON response.
        """
        return self._request(path, method="POST", body=body, headers=headers).decode(
            "utf-8", errors="replace"
        )

    def get_json(self, path: str) -> object:
        raw = self._request(path)
        try:
            parsed: object = json.loads(raw.decode("utf-8"))
            return parsed
        except (UnicodeDecodeError, json.JSONDecodeError) as exc:
            raise DiabloTradeError(f"{path} did not return JSON") from exc

    # -- reads -------------------------------------------------------------

    def get_search(self, short_id: str) -> SavedSearch:
        """Load a search created in the site UI.

        `short_id` is the trailing segment of diablo.trade/listings/items/<id>.
        """
        search = SavedSearch.parse(self.get_json(f"/api/search/{short_id}"))
        if not search.listing_ids and not search.filters:
            raise DiabloTradeError(
                f"Search {short_id!r} returned nothing. Has it expired?"
            )
        return search

    def get_listings(self, listing_ids: Iterable[str]) -> Iterator[Listing]:
        """Hydrate listing ids into full records, batching as the site does."""
        ids = [i for i in listing_ids if i]
        for start in range(0, len(ids), BATCH_SIZE):
            batch = ids[start : start + BATCH_SIZE]
            query = urllib.parse.urlencode({"ids": ",".join(batch)})
            payload = self.get_json(f"/api/listing/get?{query}")
            for raw in _unwrap_listings(payload):
                listing = Listing.parse(raw)
                if listing is not None:
                    yield listing
            if self.delay_between_requests and start + BATCH_SIZE < len(ids):
                time.sleep(self.delay_between_requests)

    def search_listings(self, short_id: str) -> list[Listing]:
        """Convenience: resolve a saved search straight to full listings."""
        return list(self.get_listings(self.get_search(short_id).listing_ids))

    def session(self) -> Json:
        """Current session. Useful to verify a cookie is actually valid."""
        raw = self.get_json("/api/session")
        # isinstance narrows only to dict[Unknown, Unknown]; the cast records
        # what the endpoint returns rather than silencing the diagnostic.
        return cast(Json, raw) if isinstance(raw, dict) else {}


def _failure(method: str, path: str, response: Response) -> DiabloTradeError:
    """Turn a non-2xx response into the most specific error we can name."""
    status = response.status_code
    # Server Actions signal "log in" as a 303 to /session-expired rather than
    # as a 401, so the redirect target is the only thing that says what is wrong.
    redirect = response.headers.get("x-action-redirect", "")
    if status in (401, 403) or "session-expired" in redirect:
        detail = _detail(response, redirect)
        return AuthRequiredError(
            f"{method} {path} returned {status}{detail}. Pass a session cookie."
        )
    return DiabloTradeError(
        f"{method} {path} returned HTTP {status}{_detail(response, redirect)}"
    )


def _detail(response: Response, redirect: str) -> str:
    """Render a failure response compactly for an exception message."""
    # A redirect target names the cause precisely, and the body that comes with
    # it is a whole rendered page. Prefer the one-liner.
    if redirect:
        return f": redirect to {redirect}"
    text = response.text.strip()
    if not text:
        return ""
    if len(text) > _ERROR_BODY_LIMIT:
        text = text[:_ERROR_BODY_LIMIT] + " ...[truncated]"
    return f": {text}"


def _unwrap_listings(payload: object) -> list[object]:
    """The listing endpoint has been seen returning both shapes."""
    if isinstance(payload, list):
        return cast("list[object]", payload)
    if isinstance(payload, dict):
        container = cast(Json, payload)
        for key in ("listings", "data", "results"):
            value = container.get(key)
            if isinstance(value, list):
                return cast("list[object]", value)
    return []
