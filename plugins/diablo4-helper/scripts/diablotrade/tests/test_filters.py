"""Filtering and parsing tests. No network: every fixture is a literal payload."""

from __future__ import annotations

import pytest

from diablotrade import metadata
from diablotrade.filters import (
    as_or_groups,
    at_least,
    at_least_attributes,
    attribute,
    attribute_at_least,
    greater_affix,
    rank,
)
from diablotrade.models import Listing

STR = "2843c662-6fca-4e43-8a40-ec8b88b138dc"
LIFE = "2ac58fe5-0b42-42c4-95aa-33e71e71900d"
FURY = "36cf743c-6b6a-48c4-b6b8-e318e7f7b1da"
CRIT = "80def26f-6fb5-4dd3-ae16-47b6bedcbefb"
GUARANTEED = "fe7b8970-7858-4c3d-8a0e-71f106b07863"


def make(attrs: dict[str, float], *, greater: set[str] | None = None, price: int = 0) -> Listing:
    greater = greater or set()
    listing = Listing.parse(
        {
            "id": "listing-" + "-".join(sorted(attrs)),
            "name": "Ramaladni's Magnum Opus",
            "rawPrice": price,
            "uniqueEquipmentId": "b11a2744-6331-4356-b608-b71e2a30f18d",
            "item": {
                "power": 900,
                "isAncestral": True,
                "greaterAffixes": len(greater),
                "affixes": [
                    {"attributeId": k, "values": [v], "isGreater": k in greater}
                    for k, v in attrs.items()
                ],
                "inherents": [],
            },
        }
    )
    assert listing is not None
    return listing


class TestParsing:
    def test_malformed_listing_is_dropped_not_raised(self) -> None:
        assert Listing.parse({"no": "id"}) is None
        assert Listing.parse("not a dict") is None

    def test_missing_item_yields_empty_item(self) -> None:
        listing = Listing.parse({"id": "x"})
        assert listing is not None
        assert listing.item.attribute_ids == frozenset()

    def test_affix_without_attribute_id_is_skipped(self) -> None:
        listing = Listing.parse({"id": "x", "item": {"affixes": [{"values": [1]}]}})
        assert listing is not None
        assert listing.item.affixes == ()

    def test_inherents_count_toward_attribute_ids(self) -> None:
        listing = Listing.parse(
            {"id": "x", "item": {"affixes": [], "inherents": [{"attributeId": STR}]}}
        )
        assert listing is not None
        assert STR in listing.item.attribute_ids


class TestPredicates:
    def test_attribute_presence(self) -> None:
        listing = make({STR: 180})
        assert attribute(STR)(listing)
        assert not attribute(LIFE)(listing)

    def test_attribute_at_least_threshold(self) -> None:
        listing = make({STR: 180})
        assert attribute_at_least(STR, 150)(listing)
        assert not attribute_at_least(STR, 200)(listing)

    def test_attribute_at_least_missing_attribute_is_false(self) -> None:
        assert not attribute_at_least(LIFE, 1)(make({STR: 180}))

    def test_greater_affix_requires_the_flag(self) -> None:
        assert greater_affix(STR)(make({STR: 180}, greater={STR}))
        assert not greater_affix(STR)(make({STR: 180}))


class TestAtLeast:
    def test_three_of_four_accepts_one_missing(self) -> None:
        predicate = at_least_attributes(3, [STR, LIFE, FURY, CRIT])
        assert predicate(make({STR: 1, LIFE: 1, FURY: 1}))
        assert predicate(make({STR: 1, LIFE: 1, FURY: 1, CRIT: 1}))

    def test_three_of_four_rejects_two_missing(self) -> None:
        predicate = at_least_attributes(3, [STR, LIFE, FURY, CRIT])
        assert not predicate(make({STR: 1, LIFE: 1}))

    def test_zero_is_always_true(self) -> None:
        assert at_least(0, [])(make({}))

    def test_asking_for_more_than_supplied_raises(self) -> None:
        with pytest.raises(ValueError, match="cannot satisfy"):
            at_least(5, [attribute(STR)])


class TestOrGroups:
    def test_three_of_four_is_the_six_pairs(self) -> None:
        groups = as_or_groups(3, [STR, LIFE, FURY, CRIT])
        assert len(groups) == 6
        assert all(len(g) == 2 for g in groups)

    def test_all_of_four_is_four_singletons(self) -> None:
        groups = as_or_groups(4, [STR, LIFE, FURY, CRIT])
        assert len(groups) == 4
        assert all(len(g) == 1 for g in groups)

    def test_groups_agree_with_the_predicate(self) -> None:
        """The CNF expansion must accept exactly what at_least accepts."""
        attrs = [STR, LIFE, FURY, CRIT]
        groups = as_or_groups(3, attrs)
        predicate = at_least_attributes(3, attrs)
        for size in range(len(attrs) + 1):
            for combo in _subsets(attrs, size):
                listing = make(dict.fromkeys(combo, 1.0))
                held = frozenset(combo)
                cnf = all(any(a in held for a in group) for group in groups)
                assert cnf == predicate(listing), combo


def _subsets(items: list[str], size: int) -> list[tuple[str, ...]]:
    from itertools import combinations

    return list(combinations(items, size))


class TestRank:
    def test_orders_by_match_count_then_price(self) -> None:
        best = make({STR: 1, LIFE: 1, FURY: 1}, price=500)
        cheap_partial = make({STR: 1, LIFE: 1}, price=10)
        results = rank([cheap_partial, best], [STR, LIFE, FURY], minimum=2)
        assert results[0].listing.id == best.id
        assert results[0].score == 3

    def test_minimum_filters_out_low_scores(self) -> None:
        results = rank([make({STR: 1})], [STR, LIFE, FURY], minimum=2)
        assert results == []


class TestMetadata:
    def test_guaranteed_is_attribute_on_every_listing(self) -> None:
        listings = [
            make({GUARANTEED: 155, STR: 180}),
            make({GUARANTEED: 128, LIFE: 1950}),
            make({GUARANTEED: 140, FURY: 19}),
        ]
        assert metadata.guaranteed(listings) == [GUARANTEED]
        assert set(metadata.roll_pool(listings)) == {STR, LIFE, FURY}

    def test_frequency_percentages(self) -> None:
        listings = [make({STR: 1}), make({STR: 1}), make({LIFE: 1})]
        stats = {s.attribute_id: s for s in metadata.frequency(listings)}
        assert stats[STR].pct == pytest.approx(66.67, abs=0.01)
        assert not stats[STR].is_guaranteed

    def test_empty_sample_has_no_guaranteed(self) -> None:
        assert metadata.guaranteed([]) == []
