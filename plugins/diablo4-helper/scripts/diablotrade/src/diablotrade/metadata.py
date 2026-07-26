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
KNOWN_ATTRIBUTES: dict[str, str] = {
    "2843c662-6fca-4e43-8a40-ec8b88b138dc": "+ Strength",
    "04051e5e-0eda-4e8a-9612-7692fdb379db": "Strength %",
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
