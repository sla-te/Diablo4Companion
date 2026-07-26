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


# How much each slot inflates a Legendary Aspect's roll range.
#
# Derived empirically, not from documentation: two searches (one Crushing with a
# [45-65] base range, one Heavenly Strength with [30-40]) were grouped by
# equipmentType, and every slot below reproduced its base range times this
# factor across hundreds of listings.
SLOT_ASPECT_SCALE: dict[str, float] = {
    "AMULET": 1.5,
    "BOW": 2.0,
    "CROSSBOW": 2.0,
    "POLEARM": 2.0,
    "STAFF": 2.0,
    "TWO_HANDED_AXE": 2.0,
    "TWO_HANDED_MACE": 2.0,
    "TWO_HANDED_SWORD": 2.0,
}

# Slots whose label covers BOTH a one-handed and a two-handed weapon, so the
# scale cannot be read off equipmentType alone - the same "SWORD" label carries
# rolls at 1x and at 2x. Anything here gets no base roll rather than a guess.
AMBIGUOUS_ASPECT_SLOTS: frozenset[str] = frozenset(
    {"SWORD", "MACE", "AXE", "SCYTHE"}
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
    effect_affix: Affix | None = None

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
            effect_affix=Affix.parse(data.get("effectAffix")),
        )

    @property
    def aspect_roll(self) -> float | None:
        """The item's Legendary Aspect roll, as displayed on the item.

        This is NOT comparable across slots. Amulets scale an aspect's range by
        1.5x and two-handed weapons by 2x, so a 54 on an amulet is a worse base
        roll than a 40 on a chest. Use `base_aspect_roll` to compare.
        """
        if self.effect_affix is None or not self.effect_affix.values:
            return None
        return self.effect_affix.values[0]

    @property
    def aspect_scale(self) -> float | None:
        """How much this slot inflates an aspect's roll range, or None.

        None means the slot is one of AMBIGUOUS_ASPECT_SLOTS and the scale is
        genuinely not knowable from the payload.
        """
        slot = self.equipment_type or ""
        if slot in AMBIGUOUS_ASPECT_SLOTS:
            return None
        return SLOT_ASPECT_SCALE.get(slot, 1.0)

    @property
    def base_aspect_roll(self) -> float | None:
        """The aspect roll normalised back to its base (1x) range.

        The Codex of Power stores the base roll, so this is the number that
        decides whether salvaging an item improves your Codex entry. Comparing
        raw `aspect_roll` across slots silently ranks a mediocre amulet above a
        perfect chest.
        """
        roll = self.aspect_roll
        scale = self.aspect_scale
        return None if roll is None or scale is None else roll / scale

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
