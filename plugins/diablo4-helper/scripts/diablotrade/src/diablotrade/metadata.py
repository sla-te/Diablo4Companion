"""Working out which attribute UUID is which stat.

diablo.trade does not put stat names in the listing payload: every affix comes
back as `{"attributeId": "<uuid>", "displayText": "", "values": [...]}`. The
names live in an encrypted metadata blob the site stores in IndexedDB under
`d4-metadata-en-db`, keyed by a version hash from /api/d4/metadata.version. That
blob is not decodable from outside the site's own JS, so this package works in
UUIDs and gives you two ways to attach names to them.

1. Read them off the site (exact, ten seconds)
   ------------------------------------------
   Open the Affixes Filter on diablo.trade, type a stat name into the property
   search box, then run BROWSER_UUID_SNIPPET in the devtools console. Each
   option's `data-value` is "<uuid> <name>", e.g.
   "2843c662-6fca-4e43-8a40-ec8b88b138dc + Strength".

2. Infer them from the market (approximate, no browser)
   ----------------------------------------------------
   `frequency` reports how often each attribute appears across listings of one
   unique. Since Patch 3.1.0 every Unique has exactly 2 guaranteed affixes and
   randomises the rest, so attributes at 100% are the guaranteed pair and the
   rest are the roll pool. That identifies the SLOTS without naming them, which
   is often all you need: guaranteed affixes are dead weight in a filter.
"""

from __future__ import annotations

from collections.abc import Iterable
from dataclasses import dataclass

from .models import Listing

BROWSER_UUID_SNIPPET = (
    "[...document.querySelectorAll('[cmdk-item]')]"
    ".map(e => e.getAttribute('data-value'))"
)

# Confirmed by reading data-value off the site's own property picker.
#
# This is the base-stat layer only: plain affixes that any item can carry. The
# picker also exposes hundreds of aspect-scoped and unique-scoped variants
# ("Sescheron's Fury: Maximum Life %") which are deliberately not listed here -
# they are per-item and belong in a lookup that is generated, not hand-kept.
#
# Names are the picker's own wording, verbatim. Two conventions matter when
# matching: a leading "+" marks a flat additive roll, a trailing "%" marks a
# percentage, and "Multiplier %" is the separate multiplicative bucket - so
# "Critical Strike Damage %" and "Critical Strike Damage Multiplier %" are two
# different attributes, not two spellings of one.
KNOWN_ATTRIBUTES: dict[str, str] = {
    "f1ffbc6b-be52-44bf-9cc2-c1603095f131": "+ Armor",
    "242dd9dd-b4ce-4bca-bbad-83bd4ed8dd1c": "+ Armored Hide",
    "d9183230-ea1e-4584-a0f9-f1f16d75b3d5": "+ Cold Resistance",
    "bcb08a6e-abc4-4585-b75a-bd68d765b296": "+ Cyclone Armor",
    "e7691641-60d2-4e9e-b067-014c194f500d": "+ Fire Resistance",
    "5a2eb8b8-34c6-4ea6-934f-da5d366abf00": "+ Ice Armor",
    "1545a514-3958-43eb-8f62-0a46c5109802": "+ Martial Skills",
    "2ac58fe5-0b42-42c4-95aa-33e71e71900d": "+ Maximum Life",
    "ac67e597-952c-43c6-9c5e-34772749cdfc": "+ Maximum Fury",
    "e59b71bb-7b1c-47c3-ba71-0b9495369a89": "+ Maximum Resource",
    "9bc4f121-83d7-4a40-b9d7-b727fc1707b0": "+ Poison Resistance",
    "8c37fb75-2d2e-4569-a9fc-725af9d473d6": "+ Resistance to All Elements",
    "2843c662-6fca-4e43-8a40-ec8b88b138dc": "+ Strength",
    "43c4314a-11c6-4f19-aac2-ef2b46d35cc7": "+ War Cry",
    "fe7b8970-7858-4c3d-8a0e-71f106b07863": "+ Weapon Damage",
    "b8a270b4-bd63-4b7d-8b8b-333dc571161a": "+ Whirlwind",
    "3a8c972c-8567-416a-b674-4ba8ae0d6cda": "100% Main Hand Weapon Damage",
    "1eb2450a-6316-4839-adac-f5a12a020fb7": "All Damage Multiplier %",
    "60189b90-18a1-4b8a-b21d-a85b32e67381": "Blocked Damage Reduction %",
    "c6b16fe4-704e-4d94-a5a6-750afd035e8e": "Cooldown Reduction %",
    "a60b4f94-eb90-4c79-b588-49996cb506dc": "Crafting Material Drop Rate %",
    "7b4a1a3f-488e-4edb-9b31-bd79e221077a": "Critical Strike Chance %",
    "67488c9d-c305-4607-87a3-6fb9e6261a84": "Critical Strike Damage %",
    "36cf743c-6b6a-48c4-b6b8-e318e7f7b1da": "Critical Strike Damage Multiplier %",
    "269c5158-5cc1-4f2f-8afb-a51f3b69015d": "Damage Over Time Multiplier %",
    "7f5c06b3-0357-4dd5-b943-d9297e14fe87": "Damage Reduction %",
    "e949315a-beac-4138-bcb4-f698096dd74c": "Damage Reduction from Bleeding Enemies %",
    "4af4f3a5-1071-4058-9a35-2eebab571230": "Damage Reduction from Burning Enemies %",
    "e9a26d27-dfe9-4dcd-9d79-f75269ab6e85": "Damage Reduction from Corrupted Enemies %",
    "4a44133d-52e6-44a5-8460-50d82c0d28d3": "Damage Reduction from Poisoned Enemies %",
    "25440abb-905a-4d22-a2a6-fadc2c931c35": "Evade Grants Movement Speed for Seconds %",
    "7c208f72-55e1-4242-bd18-9c0301dd530a": "Gold Drop Rate %",
    "123172ca-2b95-49d0-9d46-68d1061975b0": "Impairment Reduction %",
    "62b21da0-d858-4b16-9a9d-2a7f1266f1eb": "Main Hand Weapon Damage %",
    "242472d8-12c5-4c79-9131-9d3d856b4481": "Maximum Life %",
    "cc93eeb3-2a08-4938-a7b6-728d57582614": "Movement Speed %",
    "205c3ee6-a993-47d8-80a2-67aa19451ceb": "Physical Damage Multiplier %",
    "a7c2f814-7ddb-4f04-85fc-fc49f3cfa8dd": "Resistance to All Elements %",
    "04051e5e-0eda-4e8a-9612-7692fdb379db": "Strength %",
    # The picker offers "Total Armor %" twice, under two ids. Which one a given
    # listing carries is not distinguishable from the payload, so both are kept
    # rather than one being guessed at - filter on both if you filter on either.
    "09b8536a-8570-4122-8f1d-b04a020cc795": "Total Armor %",
    "1345a58d-83fd-42a1-b6f8-17315a37aaef": "Total Armor %",
    "43442d2c-0cd4-4675-a4e0-8714e2e4f85d": "Vulnerable Damage Multiplier %",
}

# Confirmed from a listing's uniqueEquipmentId field.
KNOWN_UNIQUES: dict[str, str] = {
    "b11a2744-6331-4356-b608-b71e2a30f18d": "Ramaladni's Magnum Opus",
}


@dataclass(frozen=True, slots=True)
class AttributeStat:
    attribute_id: str
    count: int
    total: int
    example_values: tuple[float, ...]

    @property
    def pct(self) -> float:
        return 100.0 * self.count / self.total if self.total else 0.0

    @property
    def is_guaranteed(self) -> bool:
        """Present on every sampled listing.

        Treat with care: on a small sample this is suggestive, not proof.
        """
        return self.total > 0 and self.count == self.total

    @property
    def name(self) -> str | None:
        return KNOWN_ATTRIBUTES.get(self.attribute_id)


def frequency(listings: Iterable[Listing]) -> list[AttributeStat]:
    """How often each attribute appears, most common first."""
    sampled = list(listings)
    counts: dict[str, int] = {}
    examples: dict[str, tuple[float, ...]] = {}
    for listing in sampled:
        for attribute_id in listing.item.attribute_ids:
            counts[attribute_id] = counts.get(attribute_id, 0) + 1
            if attribute_id not in examples:
                examples[attribute_id] = listing.item.value_of(attribute_id) or ()
    total = len(sampled)
    stats = [
        AttributeStat(
            attribute_id=attribute_id,
            count=count,
            total=total,
            example_values=examples.get(attribute_id, ()),
        )
        for attribute_id, count in counts.items()
    ]
    return sorted(stats, key=lambda s: (-s.count, s.attribute_id))


def guaranteed(listings: Iterable[Listing]) -> list[str]:
    """Attribute ids present on every sampled listing."""
    return [s.attribute_id for s in frequency(listings) if s.is_guaranteed]


def roll_pool(listings: Iterable[Listing]) -> list[str]:
    """Attribute ids that vary, i.e. the ones worth filtering on."""
    return [s.attribute_id for s in frequency(listings) if not s.is_guaranteed]
