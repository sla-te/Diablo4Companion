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


def rank(listings: Iterable[Listing], attribute_ids: Sequence[str], minimum: int = 0) -> list[Match]:
    """Score listings by how many wanted attributes they carry, best first."""
    wanted = frozenset(attribute_ids)
    matches = [
        Match(listing=listing, matched=wanted & listing.item.attribute_ids)
        for listing in listings
    ]
    return sorted(
        (m for m in matches if m.score >= minimum),
        key=lambda m: (-m.score, m.listing.raw_price or 0),
    )
