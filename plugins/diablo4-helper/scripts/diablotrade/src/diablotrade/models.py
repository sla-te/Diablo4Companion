"""Typed views over diablo.trade's untyped JSON payloads.

The API returns plain JSON with no schema, so parsing is deliberately tolerant:
every field is read defensively and missing values become None rather than
raising. The point is that the rest of the package never touches a raw dict.
"""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import cast

Json = dict[str, object]


def _as_dict(value: object) -> Json:
    # isinstance narrows only to dict[Unknown, Unknown]; the cast records what
    # the endpoint actually returns rather than silencing a diagnostic.
    return cast(Json, value) if isinstance(value, dict) else {}


def _as_list(value: object) -> list[object]:
    return cast("list[object]", value) if isinstance(value, list) else []


def _as_str(value: object) -> str | None:
    return value if isinstance(value, str) else None


def _as_int(value: object) -> int | None:
    return value if isinstance(value, int) and not isinstance(value, bool) else None


@dataclass(frozen=True, slots=True)
class Affix:
    """One rolled stat on an item.

    `attribute_id` is the stable UUID the site filters on. The human-readable
    name is NOT in the payload: diablo.trade resolves it from an encrypted
    metadata blob held in IndexedDB, and `displayText` comes back empty. Use
    `diablotrade.metadata` for the discovery recipe.
    """

    attribute_id: str
    values: tuple[float, ...] = ()
    is_greater: bool = False

    @classmethod
    def parse(cls, raw: object) -> Affix | None:
        data = _as_dict(raw)
        attribute_id = _as_str(data.get("attributeId"))
        if attribute_id is None:
            return None
        values = tuple(v for v in _as_list(data.get("values")) if isinstance(v, (int, float)))
        return cls(
            attribute_id=attribute_id,
            values=values,
            is_greater=bool(data.get("isGreater")),
        )


@dataclass(frozen=True, slots=True)
class Item:
    power: int | None = None
    is_ancestral: bool = False
    sockets: int | None = None
    greater_affix_count: int | None = None
    equipment_type: str | None = None
    unique_equipment_id: str | None = None
    affixes: tuple[Affix, ...] = ()
    inherents: tuple[Affix, ...] = ()

    @classmethod
    def parse(cls, raw: object) -> Item:
        data = _as_dict(raw)
        return cls(
            power=_as_int(data.get("power")),
            is_ancestral=bool(data.get("isAncestral")),
            sockets=_as_int(data.get("sockets")),
            greater_affix_count=_as_int(data.get("greaterAffixes")),
            equipment_type=_as_str(data.get("equipmentType")),
            unique_equipment_id=_as_str(data.get("uniqueEquipmentId")),
            affixes=tuple(a for a in map(Affix.parse, _as_list(data.get("affixes"))) if a),
            inherents=tuple(a for a in map(Affix.parse, _as_list(data.get("inherents"))) if a),
        )

    @property
    def attribute_ids(self) -> frozenset[str]:
        """Every attribute on the item, affixes and inherents alike."""
        return frozenset(a.attribute_id for a in (*self.affixes, *self.inherents))

    def value_of(self, attribute_id: str) -> tuple[float, ...] | None:
        for affix in (*self.affixes, *self.inherents):
            if affix.attribute_id == attribute_id:
                return affix.values
        return None


@dataclass(frozen=True, slots=True)
class Listing:
    id: str
    name: str | None = None
    price: str | None = None
    raw_price: int | None = None
    rarity: str | None = None
    game_mode: str | None = None
    listing_mode: str | None = None
    unique_equipment_id: str | None = None
    seller: str | None = None
    sold: bool = False
    expired: bool = False
    item: Item = field(default_factory=Item)

    @classmethod
    def parse(cls, raw: object) -> Listing | None:
        data = _as_dict(raw)
        listing_id = _as_str(data.get("id"))
        if listing_id is None:
            return None
        return cls(
            id=listing_id,
            name=_as_str(data.get("name")),
            price=_as_str(data.get("price")),
            raw_price=_as_int(data.get("rawPrice")),
            rarity=_as_str(data.get("rarity")),
            game_mode=_as_str(data.get("gameMode")),
            listing_mode=_as_str(data.get("listingMode")),
            unique_equipment_id=_as_str(data.get("uniqueEquipmentId")),
            seller=_as_str(_as_dict(data.get("user")).get("name")),
            sold=bool(data.get("sold")),
            expired=bool(data.get("expired")),
            item=Item.parse(data.get("item")),
        )


@dataclass(frozen=True, slots=True)
class SavedSearch:
    """A search created in the site UI and addressed by its short id."""

    id: str
    filters: Json
    listing_ids: tuple[str, ...]

    @classmethod
    def parse(cls, raw: object) -> SavedSearch:
        data = _as_dict(raw)
        return cls(
            id=_as_str(data.get("id")) or "",
            filters=_as_dict(data.get("filters")),
            listing_ids=tuple(i for i in _as_list(data.get("listings")) if isinstance(i, str)),
        )
