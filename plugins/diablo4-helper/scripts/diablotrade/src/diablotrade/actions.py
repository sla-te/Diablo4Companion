"""Writing to diablo.trade: creating searches and posting listings.

READ THIS BEFORE USING - the write path is not a documented API.

diablo.trade exposes no REST route for creating a search or posting a listing.
Both go through **Next.js Server Actions**. Grepping every JS chunk loaded by
/listings/create turns up only chat and session routes, plus a set of
`createServerReference("<40 hex>")` ids. A Server Action call looks like:

    POST https://diablo.trade/listings/create
    Next-Action: <40-hex action id>
    Cookie: <session>
    Content-Type: multipart/form-data; boundary=...
    <form-encoded arguments>

Two consequences you cannot design around:

1. **Action ids are build-specific.** They change whenever the site deploys
   (the deploy hash rides along as `?dpl=` on every asset). Any id hardcoded
   here would rot. `discover_action_ids` re-scrapes them at call time.

2. **Action ids are not labelled.** The bundle says a reference exists, not what
   it does. Nothing in the static assets says "this one creates a listing".

So this module ships a working, generic Server Action *transport* plus id
discovery. It deliberately does NOT ship a `create_listing()` that guesses an
id: posting to the wrong action under your account is not something to guess at.

To bind the transport to the real action, capture one genuine listing creation
in devtools (Network -> the POST to /listings/create -> Headers -> Next-Action,
and Payload for the argument shape), then call `invoke` with those. See
`docs/posting.md` in this package directory for the capture walkthrough.
"""

from __future__ import annotations

import re
import uuid
from dataclasses import dataclass

from .client import BASE_URL, Client, DiabloTradeError

# `createServerReference("40b03a27cb...")` in the compiled client bundles.
_ACTION_ID_PATTERN = re.compile(r'createServerReference\)?\(["\']([a-f0-9]{40,42})["\']')
_SCRIPT_SRC_PATTERN = re.compile(r'<script[^>]+src="([^"]+)"')


@dataclass(slots=True)
class ServerAction:
    """A Next.js Server Action endpoint: the page that hosts it plus its id."""

    page_path: str
    action_id: str


def discover_action_ids(client: Client, page_path: str) -> list[str]:
    """Scrape the Server Action ids referenced by a page's client bundles.

    Returns them in encounter order, deduplicated. Which id does what is not
    discoverable statically; see the module docstring.
    """
    html = client.fetch_text(page_path)
    sources: list[str] = _SCRIPT_SRC_PATTERN.findall(html)
    found: list[str] = []
    seen: set[str] = set()
    for src in sources:
        url = src if src.startswith("http") else BASE_URL + src
        try:
            body = client.fetch_text(url)
        except DiabloTradeError:
            continue
        for action_id in _ACTION_ID_PATTERN.findall(body):
            if action_id not in seen:
                seen.add(action_id)
                found.append(action_id)
    return found


def encode_multipart(fields: dict[str, str]) -> tuple[bytes, str]:
    """Encode Server Action arguments as multipart/form-data.

    Next passes Server Action arguments as form fields, commonly a single field
    named "1" holding a JSON blob. Capture a real request to see the exact shape
    your target action expects.
    """
    boundary = f"----diablotrade{uuid.uuid4().hex}"
    parts: list[bytes] = []
    for name, value in fields.items():
        parts.append(f"--{boundary}\r\n".encode())
        parts.append(f'Content-Disposition: form-data; name="{name}"\r\n\r\n'.encode())
        parts.append(value.encode("utf-8"))
        parts.append(b"\r\n")
    parts.append(f"--{boundary}--\r\n".encode())
    return b"".join(parts), f"multipart/form-data; boundary={boundary}"


def invoke(client: Client, action: ServerAction, fields: dict[str, str]) -> str:
    """Call a Server Action and return its raw RSC response.

    The response is React Server Component wire format, not JSON: a sequence of
    `<id>:<payload>` lines. Callers get the raw text because what matters
    (usually a new id, or an error string) varies per action.

    Requires an authenticated client for anything account-scoped.
    """
    if not client.cookie:
        raise DiabloTradeError(
            "Server Actions are account-scoped. Construct Client(cookie=...) first."
        )
    body, content_type = encode_multipart(fields)
    return client.post_raw(
        action.page_path,
        body,
        {
            "Next-Action": action.action_id,
            "Content-Type": content_type,
            "Origin": BASE_URL,
            "Referer": BASE_URL + action.page_path,
        },
    )


def extract_short_id(rsc_response: str) -> str | None:
    """Best-effort pull of a newly minted short id out of an RSC response.

    Search creation redirects to /listings/items/<shortId>; the id shows up in
    the response body. Returns None when no such path is present.
    """
    match = re.search(r"/listings/items/([A-Za-z0-9]{5,12})", rsc_response)
    return match.group(1) if match else None
