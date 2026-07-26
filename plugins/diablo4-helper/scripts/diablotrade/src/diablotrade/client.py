"""HTTP layer for diablo.trade.

Stdlib only, so the package runs under a bare `uv run` with no install step.

What is and is not available, established by watching the site's own traffic:

* Reading is clean REST and works anonymously:
    GET /api/search/<shortId>      -> the saved search plus its listing ids
    GET /api/listing/get?ids=a,b,c -> full listing records, batched
* Writing (creating a search, creating a listing) is NOT REST. Next.js Server
  Actions handle it: a POST to the page URL carrying a `Next-Action` header
  whose value is a build-specific 40-hex id. See `diablotrade.actions`.
"""

from __future__ import annotations

import json
import time
import urllib.error
import urllib.parse
import urllib.request
from collections.abc import Iterable, Iterator
from dataclasses import dataclass
from typing import cast

from .models import Json, Listing, SavedSearch

BASE_URL = "https://diablo.trade"

# The site itself requests listing ids in batches of this size. Going wider
# risks a URL longer than the CDN will accept.
BATCH_SIZE = 50

# A browser-like UA is required; the CDN refuses urllib's default.
USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) diablotrade/0.1"


class DiabloTradeError(RuntimeError):
    """Any failure talking to diablo.trade."""


class AuthRequiredError(DiabloTradeError):
    """The endpoint refused an anonymous request."""


@dataclass(slots=True)
class Client:
    """Talks to diablo.trade.

    `cookie` is only needed for authenticated operations (posting, own
    listings). Search and listing reads work without it.
    """

    cookie: str | None = None
    base_url: str = BASE_URL
    timeout: float = 30.0
    # Be a polite client: the read endpoints are not rate-limit documented, and
    # a full 500-listing pull is 10 requests.
    delay_between_requests: float = 0.2

    def _request(
        self,
        path: str,
        *,
        method: str = "GET",
        body: bytes | None = None,
        headers: dict[str, str] | None = None,
    ) -> bytes:
        url = path if path.startswith("http") else self.base_url + path
        request = urllib.request.Request(url, data=body, method=method)
        request.add_header("User-Agent", USER_AGENT)
        request.add_header("Accept", "*/*")
        for key, value in (headers or {}).items():
            request.add_header(key, value)
        if self.cookie:
            request.add_header("Cookie", self.cookie)
        try:
            with urllib.request.urlopen(request, timeout=self.timeout) as response:
                payload: bytes = response.read()
                return payload
        except urllib.error.HTTPError as exc:
            if exc.code in (401, 403):
                raise AuthRequiredError(
                    f"{method} {path} returned {exc.code}. Pass a session cookie."
                ) from exc
            raise DiabloTradeError(f"{method} {path} returned HTTP {exc.code}") from exc
        except urllib.error.URLError as exc:
            raise DiabloTradeError(f"{method} {path} failed: {exc.reason}") from exc

    def fetch_text(self, path: str) -> str:
        """GET a page or asset as text. Used for Server Action id discovery."""
        return self._request(path).decode("utf-8", errors="replace")

    def post_raw(
        self, path: str, body: bytes, headers: dict[str, str]
    ) -> str:
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
        return raw if isinstance(raw, dict) else {}


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
