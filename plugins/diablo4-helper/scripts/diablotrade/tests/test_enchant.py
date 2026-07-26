"""One-enchant planning. No network: every fixture is a literal payload."""

from __future__ import annotations

from diablotrade.filters import plan_enchant, plan_enchants
from diablotrade.models import Listing

CDR = "cdr"
CC = "crit-chance"
VUL = "vulnerable"
CDM = "crit-dmg-mult"
WANT = [CDR, CC, VUL, CDM]

JUNK = "junk"
LIFE = "max-life"


def make(
    affixes: dict[str, bool] | None = None,
    *,
    inherents: list[str] | None = None,
    price: int = 100,
    listing_id: str = "x",
) -> Listing:
    """`affixes` maps attribute id -> is_greater."""
    listing = Listing.parse(
        {
            "id": listing_id,
            "rawPrice": price,
            "item": {
                "affixes": [
                    {"attributeId": aid, "values": [1], "isGreater": greater}
                    for aid, greater in (affixes or {}).items()
                ],
                "inherents": [
                    {"attributeId": aid, "values": [1]} for aid in (inherents or [])
                ],
            },
        }
    )
    assert listing is not None
    return listing


class TestPlanEnchant:
    def test_complete_item_needs_no_enchant(self) -> None:
        plan = plan_enchant(make(dict.fromkeys(WANT, False)), WANT)
        assert plan.gap == 0
        assert not plan.needs_enchant
        assert plan.is_closable

    def test_one_missing_with_a_spare_affix_is_closable(self) -> None:
        plan = plan_enchant(
            make({CDR: False, CC: False, VUL: False, JUNK: False}), WANT
        )
        assert plan.missing == {CDM}
        assert plan.sacrifice == {JUNK}
        assert plan.is_closable
        assert not plan.greater_sacrifice_only

    def test_spare_affix_that_is_greater_is_flagged_not_hidden(self) -> None:
        # Still legal, but it costs a Greater Affix - the buyer must be told.
        plan = plan_enchant(make({CDR: False, CC: False, VUL: False, JUNK: True}), WANT)
        assert plan.is_closable
        assert plan.costs_a_greater_affix
        assert not plan.slot_state_unknown

    def test_no_junk_affix_is_flagged_rather_than_judged(self) -> None:
        # Every rolled affix is wanted, so there is nothing to overwrite. The item
        # either has a free slot (Add Affix, free) or is full (stuck), and the
        # payload cannot tell us which - so it must not be silently ranked as
        # either a bargain or a dead end.
        plan = plan_enchant(make({CDR: False, CC: False, VUL: False}), WANT)
        assert plan.score == 3
        assert plan.missing == {CDM}
        assert plan.sacrifice == frozenset()
        assert plan.slot_state_unknown
        assert not plan.costs_a_greater_affix

    def test_inherent_counts_as_matched_but_never_as_sacrifice(self) -> None:
        plan = plan_enchant(
            make({CC: False, VUL: False, JUNK: False}, inherents=[CDR]), WANT
        )
        assert CDR in plan.matched
        assert plan.sacrifice == {JUNK}

    def test_two_missing_is_beyond_one_enchant(self) -> None:
        plan = plan_enchant(make({CDR: False, CC: False, JUNK: False}), WANT)
        assert plan.gap == 2
        assert not plan.is_closable


class TestPlanEnchants:
    def test_unknown_slot_state_ranks_below_a_known_greater_affix_cost(self) -> None:
        # An unknown may turn out to be impossible, which is worse than a loss
        # you can price - so it must not be flattered by being cheaper.
        listings = [
            make({CDR: False, CC: False, VUL: False}, price=1, listing_id="unknown"),
            make(
                {CDR: False, CC: False, VUL: False, JUNK: True},
                price=900,
                listing_id="ga",
            ),
        ]
        assert [p.listing.id for p in plan_enchants(listings, WANT)] == [
            "ga",
            "unknown",
        ]

    def test_negotiable_listings_sort_last_not_first(self) -> None:
        # rawPrice 0 means "make an offer", not "free". Sorting it as 0 puts the
        # priciest items in the market at the top of a cheapest-first list.
        listings = [
            make(
                {CDR: False, CC: False, VUL: False, JUNK: False},
                price=0,
                listing_id="offer",
            ),
            make(
                {CDR: False, CC: False, VUL: False, JUNK: False},
                price=500,
                listing_id="priced",
            ),
        ]
        assert [p.listing.id for p in plan_enchants(listings, WANT)] == [
            "priced",
            "offer",
        ]

    def test_closable_only_drops_gaps_of_two_or_more(self) -> None:
        listings = [
            make({CDR: False, CC: False, JUNK: False}, listing_id="gap2"),
            make(
                {CDR: False, CC: False, VUL: False, JUNK: False}, listing_id="fixable"
            ),
        ]
        assert [p.listing.id for p in plan_enchants(listings, WANT)] == ["fixable"]

    def test_complete_outranks_one_enchant_away_even_when_dearer(self) -> None:
        listings = [
            make(
                {CDR: False, CC: False, VUL: False, JUNK: False},
                price=1,
                listing_id="near",
            ),
            make(dict.fromkeys(WANT, False), price=999, listing_id="done"),
        ]
        assert [p.listing.id for p in plan_enchants(listings, WANT)] == ["done", "near"]

    def test_cheap_greater_sacrifice_ranks_below_a_clean_one(self) -> None:
        listings = [
            make(
                {CDR: False, CC: False, VUL: False, JUNK: True},
                price=1,
                listing_id="costly",
            ),
            make(
                {CDR: False, CC: False, VUL: False, LIFE: False},
                price=500,
                listing_id="clean",
            ),
        ]
        assert [p.listing.id for p in plan_enchants(listings, WANT)] == [
            "clean",
            "costly",
        ]
