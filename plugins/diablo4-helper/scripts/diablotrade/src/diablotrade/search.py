"""Creating searches without a browser.

diablo.trade has no REST route for creating a search - `/api/search/<id>` only
*reads* one, and POSTing to `/api/search`, `/api/listing/search` and friends all
404. Creation runs through the same Next.js Server Action machinery as posting.

The exact call was captured by patching `window.fetch` in the page and pressing
Search once:

    POST /listings/items[/<previousShortId>]
    Accept: text/x-component
    Next-Action: <42-hex action id>
    x-deployment-id: <deploy hash>
    multipart/form-data with a single field "_1_input" holding the filter JSON

The response is RSC wire format containing `/listings/items/<newShortId>`, which
is the id `Client.get_search` then reads. So one capture buys a permanent Python
path: build filters here, get a short id back, hydrate with the normal client.

Unlike listing creation this action is safe to call - it only mints a saved
search, and it works unauthenticated.

STATUS: NOT WORKING YET. `create_search` currently gets HTTP 500 back. The
filter payload and the action id are both verified against a real capture, and
adding the router state tree changed nothing, so the remaining difference is in
the request envelope, not the arguments. `docs/searching.md` records exactly
what was captured and what has already been ruled out - read it before changing
anything here. Everything else in this module (DEFAULT_FILTERS, stat_group,
build_filters) is capture-verified and useful on its own.
"""

from __future__ import annotations

import json
import re
from urllib.parse import quote

from .actions import ServerAction, encode_multipart, extract_short_id
from .client import BASE_URL, Client, DiabloTradeError

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
    "auctionType": "",
    # How recently a listing was posted. The site's own control is the
    # "RECENT LISTINGS" dropdown; empty means no age limit.
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

# UNVERIFIED. Only `""` (no age limit) has actually been observed in a captured
# payload; the rest are what the "RECENT LISTINGS" dropdown plausibly sends and
# have NOT been confirmed. Open the dropdown, pick each option, and read the
# resulting payload before relying on these - the server ignores an unrecognised
# value silently, so a wrong one reads as "no results" rather than as an error.
LIST_PERIODS: tuple[str, ...] = ("", "1", "3", "7", "14", "30")


def stat_group(
    attribute_ids: list[str],
    *,
    mode: str = "and",
    group_id: str = "00000000-0000-4000-8000-000000000001",
) -> dict[str, object]:
    """One affix group: AND / OR / NOT over a set of attribute ids.

    `as_or_groups` in `filters` turns an "at least N of M" rule into the list of
    OR groups that expresses it; feed each one through here with mode="or".
    """
    if mode not in {"and", "or", "not"}:
        raise ValueError(f"mode must be and/or/not, got {mode!r}")
    return {
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

    Raises DiabloTradeError if the response carries no short id - which almost
    always means the action id rotated with a deploy.
    """
    body, content_type = encode_multipart({"_1_input": json.dumps(filters)})
    headers = {
        "Next-Action": action_id,
        "Content-Type": content_type,
        "Accept": "text/x-component",
        "Next-Router-State-Tree": quote(json.dumps(_ROUTER_STATE_TREE, separators=(",", ":"))),
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
        raise DiabloTradeError(
            "search creation returned no short id - the Server Action id has "
            "most likely rotated with a site deploy. Re-capture it; "
            "see docs/searching.md."
        )
    return short_id
