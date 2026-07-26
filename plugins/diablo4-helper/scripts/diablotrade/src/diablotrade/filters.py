"""Composable predicates over listings, applied client side.

Why client side: diablo.trade's affix filter is built from AND / OR / NOT groups,
which cannot express "at least N of these M affixes" in one group. In boolean
terms that shape is a conjunction over every (M choose M-N+1) subset, so asking
for 3-of-4 means six OR-groups clicked by hand. Pulling the listings and
filtering here is both easier and more expressive.

`at_least` is the predicate that motivated the package: an item missing exactly
one wanted affix is still worth buying, because the Occultist can enchant one
affix per item.
"""

from __future__ import annotations

from collections.abc import Callable, Iterable, Sequence
from dataclasses import dataclass
from itertools import combinations

from .models import Listing

Predicate = Callable[[Listing], bool]


def attribute(attribute_id: str) -> Predicate:
    """Item carries this attribute at all."""
    return lambda listing: attribute_id in listing.item.attribute_ids


def attribute_at_least(attribute_id: str, minimum: float) -> Predicate:
    """Item carries this attribute and its first value is >= minimum."""

    def check(listing: Listing) -> bool:
        values = listing.item.value_of(attribute_id)
        return bool(values) and values[0] >= minimum

    return check


def greater_affix(attribute_id: str) -> Predicate:
    """Attribute is present AND rolled as a Greater Affix."""
    return lambda listing: any(
        a.attribute_id == attribute_id and a.is_greater
        for a in (*listing.item.affixes, *listing.item.inherents)
    )


def unique_equipment(unique_equipment_id: str) -> Predicate:
    return lambda listing: listing.unique_equipment_id == unique_equipment_id


def min_power(power: int) -> Predicate:
    return lambda listing: (listing.item.power or 0) >= power


def ancestral() -> Predicate:
    return lambda listing: listing.item.is_ancestral


def min_greater_affixes(count: int) -> Predicate:
    return lambda listing: (listing.item.greater_affix_count or 0) >= count


def not_sold() -> Predicate:
    return lambda listing: not listing.sold and not listing.expired


def max_price(limit: int) -> Predicate:
    """Filter on rawPrice. Listings without one are excluded."""
    return lambda listing: listing.raw_price is not None and listing.raw_price <= limit


def is_negotiable(listing: Listing) -> bool:
    """No asking price set - the seller wants an offer.

    The site encodes this as rawPrice 0, which sorts ahead of every real price
    and floats these to the top of any cheapest-first ranking. They are not
    cheap; they are unpriced.
    """
    return not listing.raw_price


def sort_price(listing: Listing) -> float:
    """rawPrice for sorting, with negotiable listings pushed to the end."""
    return float("inf") if is_negotiable(listing) else float(listing.raw_price or 0)


# -- combinators -----------------------------------------------------------


def all_of(*predicates: Predicate) -> Predicate:
    return lambda listing: all(p(listing) for p in predicates)


def any_of(*predicates: Predicate) -> Predicate:
    return lambda listing: any(p(listing) for p in predicates)


def negate(predicate: Predicate) -> Predicate:
    return lambda listing: not predicate(listing)


def at_least(count: int, predicates: Sequence[Predicate]) -> Predicate:
    """True when at least `count` of `predicates` hold."""
    if count <= 0:
        return lambda _listing: True
    if count > len(predicates):
        raise ValueError(f"cannot satisfy {count} of only {len(predicates)} predicates")
    return lambda listing: sum(1 for p in predicates if p(listing)) >= count


def at_least_attributes(count: int, attribute_ids: Sequence[str]) -> Predicate:
    """The common case: at least `count` of these attribute UUIDs present."""
    return at_least(count, [attribute(a) for a in attribute_ids])


def as_or_groups(count: int, attribute_ids: Sequence[str]) -> list[tuple[str, ...]]:
    """The same "at least N of M" rule expressed as OR-groups for the site UI.

    Returns the groups that must all be satisfied (an AND of ORs). For 3-of-4
    that is the six pairs; for 4-of-4 it is four single-element groups.

    Useful when you want to reproduce the filter inside diablo.trade rather than
    run it here.
    """
    if count > len(attribute_ids):
        raise ValueError(f"cannot satisfy {count} of only {len(attribute_ids)}")
    missing_allowed = len(attribute_ids) - count
    return [tuple(g) for g in combinations(attribute_ids, missing_allowed + 1)]


def apply(listings: Iterable[Listing], predicate: Predicate) -> list[Listing]:
    return [listing for listing in listings if predicate(listing)]


@dataclass(frozen=True, slots=True)
class Match:
    """A listing plus which of the wanted attributes it actually had."""

    listing: Listing
    matched: frozenset[str]

    @property
    def score(self) -> int:
        return len(self.matched)


def rank(
    listings: Iterable[Listing], attribute_ids: Sequence[str], minimum: int = 0
) -> list[Match]:
    """Score listings by how many wanted attributes they carry, best first.

    Counting alone overstates an item: see `plan_enchants` for whether the last
    missing affix can actually be enchanted in, and at what cost.
    """
    wanted = frozenset(attribute_ids)
    matches = [
        Match(listing=listing, matched=wanted & listing.item.attribute_ids)
        for listing in listings
    ]
    return sorted(
        (m for m in matches if m.score >= minimum),
        key=lambda m: (-m.score, m.listing.raw_price or 0),
    )


@dataclass(frozen=True, slots=True)
class EnchantPlan:
    """What closing the last affix gap would actually cost on this listing.

    A match count says "3 of 4, one enchant away" and stops there. What it hides
    is the price of that enchant, which is set by what the item has spare:

    - A junk rolled affix: enchant it away, nothing of value lost.
    - Only a GREATER affix spare: legal, but you destroy a GA to fix the item.
    - Nothing spare at all: every rolled affix is already wanted. Either the item
      has a free slot (Cube Add Affix fills it for free, the best case) or it is
      full and permanently stuck. **The payload does not carry a slot count, so
      this code cannot tell those apart** - it flags them instead of guessing.

    Inherents count toward `matched` but never toward `sacrifice`: they are part
    of the base item and cannot be enchanted.
    """

    listing: Listing
    matched: frozenset[str]
    missing: frozenset[str]
    sacrifice: frozenset[str]
    greater_sacrifice_only: bool

    @property
    def score(self) -> int:
        return len(self.matched)

    @property
    def gap(self) -> int:
        return len(self.missing)

    @property
    def needs_enchant(self) -> bool:
        return self.gap == 1

    @property
    def is_closable(self) -> bool:
        """Complete already, or within one Occultist enchant / Cube Add Affix."""
        return self.gap <= 1

    @property
    def costs_a_greater_affix(self) -> bool:
        """Closing the gap means overwriting a Greater Affix."""
        return self.needs_enchant and self.greater_sacrifice_only

    @property
    def slot_state_unknown(self) -> bool:
        """Gap of one, but no junk affix to overwrite - inspect this by hand.

        Free slot means Add Affix and no loss; a full item means it is stuck.
        Nothing in the listing payload distinguishes the two.
        """
        return self.needs_enchant and not self.sacrifice


def plan_enchant(listing: Listing, attribute_ids: Sequence[str]) -> EnchantPlan:
    """Work out whether one enchant completes `attribute_ids` on this listing."""
    wanted = frozenset(attribute_ids)
    item = listing.item
    matched = wanted & item.attribute_ids

    # Only rolled affixes can be overwritten - inherents are part of the base.
    rolled = {a.attribute_id: a for a in item.affixes}
    spare = {aid: affix for aid, affix in rolled.items() if aid not in wanted}

    return EnchantPlan(
        listing=listing,
        matched=matched,
        missing=wanted - matched,
        sacrifice=frozenset(spare),
        greater_sacrifice_only=bool(spare)
        and all(a.is_greater for a in spare.values()),
    )


def plan_enchants(
    listings: Iterable[Listing],
    attribute_ids: Sequence[str],
    *,
    closable_only: bool = True,
) -> list[EnchantPlan]:
    """Rank listings by what they are worth AFTER one enchant, best first.

    Ordering: complete items first, then by what closing the gap costs - a junk
    affix, then a Greater Affix, then the unknown-slot-state case last. Unknown
    ranks below a known Greater Affix loss because it may turn out to be
    impossible, which is worse than a loss you can price. Price breaks ties, with
    negotiable listings last. `closable_only` drops gaps of two or more.
    """
    plans = [plan_enchant(listing, attribute_ids) for listing in listings]
    if closable_only:
        plans = [p for p in plans if p.is_closable]
    return sorted(
        plans,
        key=lambda p: (
            p.gap,
            p.slot_state_unknown,
            p.costs_a_greater_affix,
            sort_price(p.listing),
        ),
    )
