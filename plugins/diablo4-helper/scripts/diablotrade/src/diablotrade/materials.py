"""Material ids - runes, keys and gold - so a search can be built by name.

diablo.trade filters runes through `materialId`, not through the item name or
any rune-specific field: a rune search is `build_filters(materialId=<uuid>)`.
The uuid is not derivable from the rune's name, so it has to be captured once.

Unlike affix attribute ids (see `metadata`), material ids ARE recoverable from
listing payloads: every priceGroup and soldOption entry embeds the full
`material` record, name included. `harvest` pulls them out, so this table grows
from real traffic rather than by hand-transcription.

Everything in KNOWN_MATERIALS below was read off a live payload on 2026-07-28.
Nothing here is inferred from a rune's in-game name.
"""

from __future__ import annotations

from collections.abc import Iterable, Mapping
from typing import cast

# Gold is a material like any other: a gold price is a priceGroup item whose
# materialId is this and whose `isGold` is true.
GOLD_ID = "00000000-0000-0000-0000-00000000600d"

# name (upper-case) -> materialId. Captured from the site's own filter payload
# and from `material` records embedded in listings.
KNOWN_MATERIALS: dict[str, str] = {
    "GOLD": GOLD_ID,
    "IGNI": "df82f572-1e7b-493a-9b6e-52fc2c34ded3",
    "JAH": "b7ce07a4-c801-46c1-be61-b021ef19cbd4",
    "SUPERIOR LAIR KEY": "7ae1f291-249e-4d75-bbf0-912be173e36d",
    "TEB": "b2ffbc7e-24ce-4e6a-8ea2-dd969ffdac7c",
    "ZAN": "5770a139-a11d-4ed9-b82f-f8b16d1c7f5d",
}


def material_id(name: str) -> str:
    """Look up a material id by name, case-insensitively.

    Raises KeyError naming what IS known, because the failure mode this guards
    against is a silent empty search from a wrong uuid.
    """
    key = name.strip().upper()
    if key not in KNOWN_MATERIALS:
        known = ", ".join(sorted(KNOWN_MATERIALS))
        raise KeyError(f"unknown material {name!r}. Known: {known}")
    return KNOWN_MATERIALS[key]


def harvest(rows: Iterable[object]) -> dict[str, str]:
    """Pull every `material` record out of raw listing payloads.

    Listings carry materials in two places - `priceGroups[].items[].material`
    for an asking price and `soldOption.items[].material` for a completed sale.
    Both embed `{id, name, type, rarity}`, so any listing that was ever priced
    in a rune names that rune for us.

    Returns name -> id, ready to be folded into KNOWN_MATERIALS.
    """
    found: dict[str, str] = {}
    for row in rows:
        if not isinstance(row, Mapping):
            continue
        record = cast(Mapping[str, object], row)
        groups = list(as_seq(record.get("priceGroups")))
        sold = record.get("soldOption")
        if isinstance(sold, Mapping):
            groups.append(cast(Mapping[str, object], sold))
        for group in groups:
            if not isinstance(group, Mapping):
                continue
            for item in as_seq(cast(Mapping[str, object], group).get("items")):
                if not isinstance(item, Mapping):
                    continue
                material = cast(Mapping[str, object], item).get("material")
                if not isinstance(material, Mapping):
                    continue
                entry = cast(Mapping[str, object], material)
                name, ident = entry.get("name"), entry.get("id")
                if isinstance(name, str) and isinstance(ident, str):
                    found[name.upper()] = ident
    return found


def as_seq(value: object) -> list[object]:
    """A JSON array as a list, or an empty list. Shared with `prices`."""
    return cast("list[object]", value) if isinstance(value, list) else []
