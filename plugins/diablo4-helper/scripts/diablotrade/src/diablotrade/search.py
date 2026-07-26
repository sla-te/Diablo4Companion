"""Creating searches without a browser.

diablo.trade has no REST route for creating a search - `/api/search/<id>` only
*reads* one, and POSTing to `/api/search`, `/api/listing/search` and friends all
404. Creation runs through the same Next.js Server Action machinery as posting.

The exact call was captured by patching `window.fetch` in the page and pressing
Search once:

    POST /listings/items
    Accept: text/x-component
    Next-Action: <42-hex action id>
    x-deployment-id: <deploy hash>
    multipart/form-data with three fields: "_1_input", "_1_requestId" and "0"

The response is RSC wire format containing `/listings/items/<newShortId>`, which
is the id `Client.get_search` then reads. So one capture buys a permanent Python
path: build filters here, get a short id back, hydrate with the normal client.

Unlike listing creation this action is safe to call - it only mints a saved
search - but it does require a logged-in session. Anonymous callers get a 303 to
`/session-expired`, and `/api/session` confirms the site mints no anonymous
session at all. Pass `Client(cookie=...)`; never persist that cookie to disk.

STATUS: working, against a capture re-taken 2026-07-26. The earlier HTTP 500 was
a malformed argument list, not a bad envelope: the first capture read the
FormData `entries()` and recorded only "_1_input", missing the sibling
"_1_requestId" and - critically - field "0", which is the argument array React's
server-side decoder needs. With no field "0" the action was invoked with no
arguments and threw. `docs/searching.md` has the full capture.
"""

from __future__ import annotations

import json
import re
from urllib.parse import quote

from .actions import ServerAction, encode_multipart, extract_short_id
from .client import BASE_URL, AuthRequiredError, Client, DiabloTradeError

SEARCH_PAGE = "/listings/items"

# Captured 2026-07-26. Server Action ids are build-specific and rotate on every
# deploy - when this stops minting a short id, re-capture (see docs/searching.md)
# rather than assuming the site changed shape.
KNOWN_SEARCH_ACTION_ID = "6011b8ff26e7e4a3e8a8fb98460750d031dfceda87"

_DEPLOYMENT_ID_PATTERN = re.compile(r'data-dpl-id="([0-9a-f]{20,})"')

# The real request carries this, so it is reproduced here. Sending it did NOT
# clear the 500, so do not read it as "the fix" - it is one captured detail out
# of several. The nesting mirrors the app's route groups for /listings/items.
_ROUTER_STATE_TREE = [
    "",
    {
        "children": [
            "(main)",
            {
                "children": [
                    "(app)",
                    {
                        "children": [
                            "(session)",
                            {
                                "children": [
                                    "listings",
                                    {
                                        "children": [
                                            "(search)",
                                            {
                                                "children": [
                                                    "items",
                                                    {
                                                        "children": [
                                                            "__PAGE__",
                                                            {},
                                                            None,
                                                            None,
                                                            0,
                                                        ]
                                                    },
                                                    None,
                                                    None,
                                                    0,
                                                ]
                                            },
                                            None,
                                            None,
                                            0,
                                        ]
                                    },
                                    None,
                                    None,
                                    0,
                                ]
                            },
                            None,
                            None,
                            0,
                        ]
                    },
                    None,
                    None,
                    0,
                ]
            },
            None,
            None,
            0,
        ]
    },
    None,
    None,
    0,
]

# Every key the site sends. Sending a subset works until it does not, so the
# full shape is kept and overridden selectively.
DEFAULT_FILTERS: dict[str, object] = {
    "listingType": "ITEM",
    "uniqueEquipmentId": "",
    "materialId": "",
    "aspectId": "",
    "equipmentSetId": "",
    "itemTypeVariantId": "",
    "requiredSetId": "",
    "gameMode": "SEASONAL_SOFTCORE",
    "listingMode": "SELLING",
    "statusFilters": ["online", "offline"],
    "itemCategory": "",
    "itemRarity": "",
    "itemRarityExclude": False,
    "sockets": "",
    "greaterAffixesMin": "",
    "greaterAffixesMax": "",
    "classType": "",
    # Read out of the site's own zod schema, which types it as a boolean next to
    # classType. Absent from the captured payload because the capture was taken
    # with the control untouched.
    "excludeGenericClass": False,
    "auctionType": "",
    # How recently a listing was posted, from the "RECENT LISTINGS" dropdown.
    # The site's schema types this as a free-form optional string, NOT an enum,
    # so there is no value set to enumerate - only "" (no age limit) has been
    # observed. An unrecognised value is ignored silently, which reads as "no
    # results" rather than as an error, so confirm any value from a real payload
    # before trusting it.
    "listPeriod": "",
    "priceMin": "",
    "priceMax": "",
    "priceMaterialId": "",
    "priceQuantityMin": "",
    "priceQuantityMax": "",
    "itemPowerMin": "",
    "itemPowerMax": "",
    "levelRequirementMin": "",
    "levelRequirementMax": "",
    "isAncestral": False,
    "favoritesOnly": False,
    "statFilters": [],
    "priceVisibility": "ANY",
    "sortAttributeDirection": "desc",
}

# React encodes Server Action arguments as form fields, and field "0" holds the
# argument array. This action is a `useActionState` handler, so it takes
# (previousState, formData): the first element is the previous state and "$K1"
# is React's reference to a FormData argument, whose own entries are flattened
# into sibling fields named "_1_<key>".
#
# Sending only "_1_input" - which is what an earlier capture recorded, because
# it read the entries and not the whole request - left the server with no
# argument array at all, so the action threw and returned HTTP 500.
_ACTION_ARGS = json.dumps([{"error": ""}, "$K1"], separators=(",", ":"))

# The site increments this per search to discard responses from superseded
# requests. Nothing reads it back, so a constant is fine for a one-shot call.
_REQUEST_ID = "1"

_NEEDS_SESSION = (
    "creating a search needs a logged-in session: pass Client(cookie=...). "
    "The site mints no anonymous session. See docs/searching.md."
)

_NO_SHORT_ID = (
    "search creation returned no short id - the Server Action id has most "
    "likely rotated with a site deploy. Re-capture it; see docs/searching.md."
)


def stat_group(
    attribute_ids: list[str],
    *,
    mode: str = "and",
    group_id: str = "00000000-0000-4000-8000-000000000001",
    min_matches: str = "",
    max_matches: str = "",
) -> dict[str, object]:
    """One affix group: AND / OR / NOT over a set of attribute ids.

    An "or" group also accepts `min_matches` / `max_matches`, which is the
    site's own "at least N of these" support - its UI creates OR groups as
    `{type:"or", minMatches:"1", maxMatches:""}`. That makes a 3-of-4 rule ONE
    group server side, not the six pairs `filters.as_or_groups` builds for the
    manual route. Both are kept: this one needs a session to create a search,
    the local route needs nothing.
    """
    if mode not in {"and", "or", "not"}:
        raise ValueError(f"mode must be and/or/not, got {mode!r}")
    if (min_matches or max_matches) and mode != "or":
        raise ValueError("min_matches/max_matches only apply to an 'or' group")
    return {
        "minMatches": min_matches,
        "maxMatches": max_matches,
        "disabled": False,
        "id": group_id,
        "type": mode,
        "affixes": [
            {
                "title": "",
                "enabled": True,
                "greaterAffixEnabled": False,
                "key": attribute_id,
                "min": "",
                "max": "",
                "variableValues": [{"min": "", "max": ""}],
            }
            for attribute_id in attribute_ids
        ],
    }


def build_filters(**overrides: object) -> dict[str, object]:
    """DEFAULT_FILTERS with overrides applied, rejecting unknown keys.

    Unknown keys are an error rather than a silent no-op: a typo'd filter that
    the server ignores looks exactly like a filter that found nothing.
    """
    unknown = set(overrides) - set(DEFAULT_FILTERS)
    if unknown:
        raise ValueError(f"unknown filter keys: {sorted(unknown)}")
    return {**DEFAULT_FILTERS, **overrides}


def discover_deployment_id(client: Client) -> str | None:
    """Scrape the current deploy hash, which the action call echoes back."""
    match = _DEPLOYMENT_ID_PATTERN.search(client.fetch_text(SEARCH_PAGE))
    return match.group(1) if match else None


def create_search(
    client: Client,
    filters: dict[str, object],
    *,
    action_id: str = KNOWN_SEARCH_ACTION_ID,
) -> str:
    """Create a saved search and return its short id.

    Needs an authenticated client: the site mints no anonymous session, so a
    cookie-less call is answered with a 303 to `/session-expired` rather than
    anything that looks like a permission error.

    Raises DiabloTradeError if the response carries no short id - which almost
    always means the action id rotated with a deploy.
    """
    if not client.cookie:
        raise AuthRequiredError(_NEEDS_SESSION)
    # Field order mirrors the browser's. React's decoder builds a map, so this
    # is not known to matter, but there is no reason to differ from the capture.
    body, content_type = encode_multipart(
        {
            "_1_input": json.dumps(filters),
            "_1_requestId": _REQUEST_ID,
            "0": _ACTION_ARGS,
        }
    )
    headers = {
        "Next-Action": action_id,
        "Content-Type": content_type,
        "Accept": "text/x-component",
        "Next-Router-State-Tree": quote(
            json.dumps(_ROUTER_STATE_TREE, separators=(",", ":"))
        ),
        "Origin": BASE_URL,
        "Referer": BASE_URL + SEARCH_PAGE,
    }
    deployment_id = discover_deployment_id(client)
    if deployment_id:
        headers["x-deployment-id"] = deployment_id

    action = ServerAction(page_path=SEARCH_PAGE, action_id=action_id)
    response = client.post_raw(action.page_path, body, headers)
    short_id = extract_short_id(response)
    if short_id is None:
        raise DiabloTradeError(_NO_SHORT_ID)
    return short_id
