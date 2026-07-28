"""Offline tests for sold-price summarising. Every fixture is a literal payload
shaped like a real one, captured 2026-07-28."""

from __future__ import annotations

from datetime import datetime, timedelta

import pytest

from diablotrade.market import Ask, MarketReport
from diablotrade.materials import GOLD_ID, harvest, material_id
from diablotrade.prices import (
    MIN_REAL_GOLD,
    PriceModel,
    Sale,
    effective_sample_size,
    fit,
    parse_sales,
    recent,
    summarise,
    timestamp_of,
    weighted_median,
)
from diablotrade.search import DEFAULT_FILTERS, OPTIONAL_FILTERS, build_filters

NOW = datetime.fromisoformat("2026-07-28T12:00:00+00:00")


def gold_item(quantity: int) -> dict[str, object]:
    return {
        "materialId": GOLD_ID,
        "material": {"id": GOLD_ID, "name": "Gold", "type": "GOLD"},
        "quantity": quantity,
        "isGold": True,
    }


def rune_item(name: str, quantity: int = 1) -> dict[str, object]:
    return {
        "materialId": "b7ce07a4-c801-46c1-be61-b021ef19cbd4",
        "material": {
            "id": "b7ce07a4-c801-46c1-be61-b021ef19cbd4",
            "name": name,
            "type": "RUNE",
        },
        "quantity": quantity,
        "isGold": False,
    }


def sale_row(
    listing_id: str,
    items: list[dict[str, object]],
    *,
    sold_price: int = 0,
    created: str = "2026-07-27T00:00:00.000Z",
) -> dict[str, object]:
    return {
        "id": listing_id,
        "sold": True,
        "soldPrice": sold_price,
        "soldOption": {"items": items},
        "createdAt": created,
        "relistedAt": created,
    }


class TestParseSales:
    def test_pure_gold_sale(self) -> None:
        sales = parse_sales([sale_row("a", [gold_item(125_000_000)])])
        assert len(sales) == 1
        assert sales[0].gold == 125_000_000
        assert sales[0].extras == ()
        assert sales[0].is_pure_gold

    def test_mixed_settlement_is_not_pure_gold(self) -> None:
        """A sale of "200M + 1 Jah" understates at 200M, so it must not count."""
        sales = parse_sales([sale_row("b", [gold_item(200_000_000), rune_item("Jah")])])
        assert sales[0].gold == 200_000_000
        assert sales[0].extras == ("1x Jah",)
        assert not sales[0].is_pure_gold

    def test_falls_back_to_sold_price_when_not_itemised(self) -> None:
        row = {
            "id": "c",
            "sold": True,
            "soldPrice": 5_000_000_000,
            "createdAt": "2026-07-26T15:39:56.515Z",
        }
        assert parse_sales([row])[0].gold == 5_000_000_000

    def test_unsold_rows_are_skipped(self) -> None:
        assert parse_sales([{"id": "d", "sold": False, "soldPrice": 1}]) == []

    def test_junk_rows_are_skipped(self) -> None:
        assert parse_sales(["nonsense", None, {"sold": True}]) == []


class TestSummarise:
    def test_median_not_dragged_by_an_outlier(self) -> None:
        """The whole point: one 5B bundle must not set the price target."""
        stats = summarise([120_000_000, 125_000_000, 130_000_000, 5_000_000_000])
        assert stats is not None
        assert stats.median == 127_500_000
        # a plain mean would be ~1.34B; the trimmed mean must stay near the body
        assert stats.trimmed_mean < 200_000_000
        assert stats.high == 5_000_000_000

    def test_ignores_placeholder_prices(self) -> None:
        stats = summarise([0, 1, 20, 100_000_000, 120_000_000])
        assert stats is not None
        assert stats.count == 2

    def test_returns_none_when_nothing_real(self) -> None:
        assert summarise([]) is None
        assert summarise([0, 20, MIN_REAL_GOLD - 1]) is None

    def test_single_value(self) -> None:
        stats = summarise([70_000_000])
        assert stats is not None
        assert stats.median == stats.p25 == stats.p75 == 70_000_000

    def test_spread_flags_a_market_with_no_consensus(self) -> None:
        tight = summarise([100_000_000, 105_000_000, 110_000_000])
        wide = summarise([100_000_000, 400_000_000, 900_000_000])
        assert tight is not None and wide is not None
        assert tight.spread < 2
        assert wide.spread > 2


class TestRecency:
    def test_recent_filters_by_window(self) -> None:
        fresh = sale_row("f", [gold_item(10_000_000)], created="2026-07-27T00:00:00Z")
        stale = sale_row("s", [gold_item(10_000_000)], created="2026-06-01T00:00:00Z")
        sales = parse_sales([fresh, stale])
        assert [s.listing_id for s in recent(sales, days=7, now=NOW)] == ["f"]

    def test_undated_sales_are_excluded(self) -> None:
        sales = parse_sales([{"id": "u", "sold": True, "soldPrice": 10_000_000}])
        assert recent(sales, days=7, now=NOW) == []

    def test_timestamp_prefers_relisted(self) -> None:
        stamp = timestamp_of(
            {"createdAt": "2026-07-01T00:00:00Z", "relistedAt": "2026-07-20T00:00:00Z"}
        )
        assert stamp is not None
        assert stamp.day == 20

    def test_bad_timestamp_is_none(self) -> None:
        assert timestamp_of({"createdAt": "not a date"}) is None
        assert timestamp_of({}) is None

    def test_window_boundary(self) -> None:
        edge = (NOW - timedelta(days=7)).isoformat()
        sales = parse_sales([sale_row("e", [gold_item(10_000_000)], created=edge)])
        assert len(recent(sales, days=7, now=NOW)) == 1


class TestAncestralTrap:
    """isAncestral=False silently empties every search - it must be unsendable."""

    def test_not_in_defaults(self) -> None:
        assert "isAncestral" not in DEFAULT_FILTERS
        assert "isAncestral" in OPTIONAL_FILTERS

    def test_false_is_rejected(self) -> None:
        with pytest.raises(ValueError, match="zero results"):
            build_filters(isAncestral=False)

    def test_true_is_allowed(self) -> None:
        assert build_filters(isAncestral=True)["isAncestral"] is True

    def test_unknown_keys_still_rejected(self) -> None:
        with pytest.raises(ValueError, match="unknown filter keys"):
            build_filters(nonsenseKey="x")


def sales_at(amounts: list[int], *, days_ago: float = 0.0) -> list[Sale]:
    """Sale records at a given age, for exercising the decay weighting."""
    stamp = NOW - timedelta(days=days_ago)
    return [
        Sale(listing_id=f"s{i}", gold=a, extras=(), listed_at=stamp)
        for i, a in enumerate(amounts)
    ]


class TestRobustFit:
    def test_a_single_huge_sale_does_not_move_the_price(self) -> None:
        """The 5B IGNI bundle case - one extreme sale must be rejected."""
        body = sales_at([140_000_000, 150_000_000, 145_000_000, 155_000_000] * 3)
        model = fit([*body, *sales_at([5_000_000_000])], now=NOW)
        assert model is not None
        assert 140_000_000 <= model.fair <= 160_000_000
        assert model.rejected == (5_000_000_000,)

    def test_a_single_tiny_sale_does_not_move_the_price(self) -> None:
        """The cheap-bait case. A linear IQR fence misses these; log space does not."""
        body = sales_at([140_000_000, 150_000_000, 145_000_000, 155_000_000] * 3)
        model = fit([*body, *sales_at([200_000])], now=NOW)
        assert model is not None
        assert 140_000_000 <= model.fair <= 160_000_000
        assert model.rejected == (200_000,)

    def test_rejection_is_symmetric_in_ratio_terms(self) -> None:
        """Half price and double price are the same size of move in log space."""
        body = [100_000_000] * 8
        low = fit(sales_at([*body, 100_000]), now=NOW)
        high = fit(sales_at([*body, 100_000_000_000]), now=NOW)
        assert low is not None and high is not None
        assert len(low.rejected) == len(high.rejected) == 1

    def test_ordinary_variation_is_kept(self) -> None:
        """Trimming must not eat the real spread - only genuine extremes go."""
        model = fit(
            sales_at([70_000_000, 100_000_000, 150_000_000, 200_000_000]), now=NOW
        )
        assert model is not None
        assert model.rejected == ()
        assert model.used == 4

    def test_identical_prices_do_not_reject_everything(self) -> None:
        """MAD is zero when most sales share a round number - a real rune case."""
        model = fit(sales_at([100_000_000] * 9), now=NOW)
        assert model is not None
        assert model.rejected == ()
        assert model.fair == 100_000_000

    def test_mostly_identical_with_one_outlier(self) -> None:
        """MAD is still zero here, so the mean-AD fallback has to do the work."""
        model = fit(sales_at([*[100_000_000] * 9, 9_000_000_000]), now=NOW)
        assert model is not None
        assert model.fair == 100_000_000
        assert model.rejected == (9_000_000_000,)

    def test_no_usable_sales(self) -> None:
        assert fit([], now=NOW) is None
        assert fit(sales_at([0, 20]), now=NOW) is None

    def test_normal_band_brackets_the_centre(self) -> None:
        model = fit(sales_at([80_000_000, 100_000_000, 125_000_000]), now=NOW)
        assert model is not None
        assert model.low < model.median < model.high


class TestRecencyWeighting:
    def test_recent_sales_outweigh_old_ones(self) -> None:
        """Same count each side; the fresh cluster must win."""
        old = sales_at([50_000_000] * 6, days_ago=30)
        new = sales_at([100_000_000] * 6, days_ago=0)
        model = fit([*old, *new], now=NOW)
        assert model is not None
        assert model.fair == 100_000_000
        # the plain median is blind to age and sits between the two clusters
        assert model.median < model.fair

    def test_a_stale_market_still_prices(self) -> None:
        """Everything old: weights shrink but their ratio still resolves."""
        model = fit(sales_at([50_000_000] * 5, days_ago=90), now=NOW)
        assert model is not None
        assert model.fair == 50_000_000

    def test_half_life_is_tunable(self) -> None:
        old = sales_at([50_000_000] * 6, days_ago=10)
        new = sales_at([100_000_000] * 5, days_ago=0)
        patient = fit([*old, *new], now=NOW, half_life_days=100.0)
        impatient = fit([*old, *new], now=NOW, half_life_days=1.0)
        assert patient is not None and impatient is not None
        assert patient.fair == 50_000_000  # long memory: the older bloc still wins
        assert impatient.fair == 100_000_000  # short memory: only today counts

    def test_undated_sales_are_discounted_not_dropped(self) -> None:
        dated = sales_at([100_000_000] * 3)
        undated = [Sale("u", 50_000_000, (), None)]
        model = fit([*dated, *undated], now=NOW)
        assert model is not None
        assert model.used == 4  # kept as evidence
        assert model.fair == 100_000_000  # but outweighed by dated sales


class TestEffectiveSampleSize:
    def test_equal_weights_give_the_full_count(self) -> None:
        assert effective_sample_size([1.0] * 10) == pytest.approx(10.0)

    def test_one_dominant_weight_collapses_to_about_one(self) -> None:
        assert effective_sample_size([1000.0, 1.0, 1.0, 1.0]) < 1.1

    def test_no_weight_is_zero(self) -> None:
        assert effective_sample_size([0.0, 0.0]) == 0.0

    def test_long_stale_history_has_a_small_effective_size(self) -> None:
        """The ZAN shape: many sales, but nearly all of them faded out.

        Prices vary - a column of identical numbers would put the spread
        estimate at zero and get the minority rejected as outliers.
        """
        old = sales_at([28_000_000 + 250_000 * i for i in range(160)], days_ago=120)
        new = sales_at([48_000_000 + 250_000 * i for i in range(8)], days_ago=1)
        model = fit([*old, *new], now=NOW)
        assert model is not None
        assert model.used == 168
        # the raw count would claim a deep market; the effective size does not
        assert model.effective < 20


class TestClassify:
    @staticmethod
    def model() -> PriceModel:
        model = fit(sales_at([90_000_000, 100_000_000, 110_000_000] * 3), now=NOW)
        assert model is not None
        return model

    def test_buckets(self) -> None:
        model = self.model()
        assert model.classify(model.floor - 1) == "suspicious"
        assert model.classify(model.low - 1) == "bargain"
        assert model.classify(model.fair) == "good"
        assert model.classify(model.high) == "fair"
        assert model.classify(model.high + 1) == "over"

    def test_merely_cheap_is_not_suspicious(self) -> None:
        """The bug this band exists to fix: warning on the good buys."""
        model = self.model()
        assert model.floor < model.low
        assert model.classify(model.low - 1) != "suspicious"

    def test_floor_is_two_sigma_down(self) -> None:
        model = self.model()
        # low is one sigma down, so the floor sits at the same ratio again
        ratio = model.fair / model.low
        assert model.floor == pytest.approx(model.fair / ratio**2, rel=1e-6)

    def test_bands_stay_ordered_when_recent_sales_ran_cheap(self) -> None:
        """The band hangs off `fair`, so a falling market cannot invert it."""
        old = sales_at([190_000_000 + 1_000_000 * i for i in range(40)], days_ago=90)
        new = sales_at([48_000_000 + 1_000_000 * i for i in range(40)], days_ago=0)
        model = fit([*old, *new], now=NOW)
        assert model is not None
        assert model.fair < model.median  # the market did fall
        assert model.floor < model.low < model.fair < model.high
        assert model.classify(model.fair) == "good"


class TestTarget:
    @staticmethod
    def build(amounts: list[int]) -> MarketReport:
        return MarketReport(
            name="X",
            model=fit(sales_at(amounts), now=NOW),
            raw=summarise(amounts),
            mixed_sales=(),
            asks=(),
            sold_search_id="a",
            live_search_id="b",
        )

    def test_thin_sample_is_flagged(self) -> None:
        rep = self.build([60_000_000] * 4)
        assert rep.target_is_thin

    def test_deep_sample_is_not_flagged(self) -> None:
        rep = self.build([150_000_000] * 32)
        assert not rep.target_is_thin
        assert rep.target == 150_000_000

    def test_no_sales_at_all(self) -> None:
        rep = self.build([])
        assert rep.target is None
        assert not rep.target_is_thin

    def test_bargains_filters_by_target(self) -> None:
        cheap = Ask("c", 10_000_000, "seller", None)
        dear = Ask("d", 90_000_000, "seller", None)
        rep = MarketReport(
            name="X",
            model=fit(sales_at([30_000_000] * 20), now=NOW),
            raw=None,
            mixed_sales=(),
            asks=(cheap, dear),
            sold_search_id="a",
            live_search_id="b",
        )
        assert [a.listing_id for a in rep.bargains()] == ["c"]


class TestWeightedMedian:
    def test_matches_plain_median_at_equal_weights(self) -> None:
        values = [1.0, 2.0, 3.0, 4.0, 5.0]
        assert weighted_median(values, [1.0] * 5) == 3.0

    def test_weight_shifts_the_centre(self) -> None:
        assert weighted_median([1.0, 2.0, 3.0], [1.0, 1.0, 50.0]) == 3.0

    def test_zero_weights_fall_back_to_plain_median(self) -> None:
        assert weighted_median([1.0, 2.0, 3.0], [0.0, 0.0, 0.0]) == 2.0

    def test_empty_is_an_error(self) -> None:
        with pytest.raises(ValueError, match="no values"):
            weighted_median([], [])

    def test_length_mismatch_is_an_error(self) -> None:
        with pytest.raises(ValueError):
            weighted_median([1.0, 2.0], [1.0])


class TestMaterials:
    def test_lookup_is_case_insensitive(self) -> None:
        assert material_id("igni") == material_id("IGNI")

    def test_unknown_material_names_what_is_known(self) -> None:
        with pytest.raises(KeyError, match="ZAN"):
            material_id("NOTARUNE")

    def test_harvest_reads_names_out_of_payloads(self) -> None:
        rows = [
            sale_row("a", [gold_item(1), rune_item("Jah")]),
            {"priceGroups": [{"items": [rune_item("Lith")]}]},
        ]
        found = harvest(rows)
        assert found["JAH"] == "b7ce07a4-c801-46c1-be61-b021ef19cbd4"
        assert "LITH" in found
        assert found["GOLD"] == GOLD_ID
