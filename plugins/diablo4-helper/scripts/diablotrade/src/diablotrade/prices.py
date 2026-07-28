"""What did this actually sell for?

An asking price is an opinion; a completed sale is evidence. diablo.trade will
return sold listings - `statusFilters=["sold"]` - and each carries `soldPrice`
plus a `soldOption` describing what was actually handed over.

Two properties of that data decide how it must be summarised:

1. **The tail is enormous.** Observed IGNI sales run from 125M to 5B in the same
   week. A mean over that is meaningless - one 5B bundle drags the "average"
   above every real trade. So the headline number here is the MEDIAN, with an
   IQR-trimmed mean beside it, never a plain mean.
2. **`soldPrice` is not always the whole price.** A sale settled as
   "200M + 1 Jah" reports soldPrice=200000000 and lists the rune separately in
   `soldOption.items`. Counting that as a 200M sale understates the true clearing
   price, so mixed sales are separated out rather than averaged in.

A sale's date is taken from `relistedAt`/`createdAt`: the payload has no
"sold at" timestamp, so recency here means "how recently was this listed",
which is a proxy and is labelled as one.
"""

from __future__ import annotations

import math
import statistics
from collections.abc import Iterable, Mapping, Sequence
from dataclasses import dataclass
from datetime import UTC, datetime
from typing import cast

from .materials import GOLD_ID, as_seq

# Below this, a "price" is a placeholder rather than an offer - listings priced
# at 0 or a token amount are barter/negotiable ads, not gold trades.
MIN_REAL_GOLD = 100_000

# Scale factor that makes the MAD a consistent estimator of the standard
# deviation for normally-distributed data. Standard constant, 1/0.6745.
MAD_TO_SIGMA = 1.4826

# Same idea for the mean absolute deviation, used only when the MAD collapses
# to zero (which happens whenever more than half the sales share one round
# number - very common at 100M/150M/200M).
MEANAD_TO_SIGMA = 1.253314

# Iglewicz-Hoaglin's threshold for the modified z-score. 3.5 is their published
# recommendation; in log space it means "further than ~3.5 robust sigmas in
# RATIO terms", so it scales with the price rather than with absolute gold.
OUTLIER_Z = 3.5

# How far below centre an ASK has to sit before it is worth distrusting rather
# than buying. Deliberately looser than one sigma: about one honest sale in six
# falls below one sigma, so a one-sigma alarm would fire on precisely the
# listings worth having. Two sigma is roughly the cheapest 2% - by then a stale
# ad or a bait listing is the likelier explanation than a generous seller.
SUSPICION_SIGMAS = 2.0

# Half-life for time-decay weighting. A sale two weeks old counts half as much
# as one today.
#
# This was 5 days and that was too jumpy: against real ZAN data, 168 sales with
# a plain median of 30M produced a weighted "fair" of 50M, because so little
# weight survived past the last few days that a handful of sales set the price -
# the same failure a hard 7-day window has, arriving by a different route.
# Fourteen days keeps a real recency preference (a month-old sale still counts a
# quarter) while leaving enough weight spread across the history that no small
# cluster can swing it. Watch `effective` rather than `used` to see whether that
# is holding for a given material.
HALF_LIFE_DAYS = 14.0

# An undated sale cannot be placed in time. Rather than dropping it (which
# would throw away real evidence) or trusting it (which would let an ancient
# sale count as current), it is discounted to this many half-lives.
UNDATED_PENALTY_HALF_LIVES = 3.0


@dataclass(frozen=True, slots=True)
class Sale:
    """One completed sale, normalised."""

    listing_id: str
    gold: int
    extras: tuple[str, ...]
    listed_at: datetime | None

    @property
    def is_pure_gold(self) -> bool:
        """True when gold was the entire consideration.

        Only these are comparable to a gold asking price.
        """
        return self.gold >= MIN_REAL_GOLD and not self.extras


@dataclass(frozen=True, slots=True)
class PriceStats:
    """Robust summary of a set of gold sales."""

    count: int
    median: int
    trimmed_mean: int
    p25: int
    p75: int
    low: int
    high: int

    @property
    def spread(self) -> float:
        """p75/p25 - how wide the market is. Above ~2 means no real consensus."""
        return self.p75 / self.p25 if self.p25 else 0.0


def parse_sales(rows: Iterable[object]) -> list[Sale]:
    """Turn raw sold-listing payloads into Sale records."""
    sales: list[Sale] = []
    for row in rows:
        if not isinstance(row, Mapping):
            continue
        record = cast(Mapping[str, object], row)
        if not record.get("sold"):
            continue
        listing_id = record.get("id")
        if not isinstance(listing_id, str):
            continue
        gold, extras = _settlement(record)
        sales.append(
            Sale(
                listing_id=listing_id,
                gold=gold,
                extras=extras,
                listed_at=timestamp_of(record),
            )
        )
    return sales


def _settlement(record: Mapping[str, object]) -> tuple[int, tuple[str, ...]]:
    """Split what was paid into gold and everything else.

    Prefers the itemised `soldOption`, because that is the only place a mixed
    settlement is visible. Falls back to the scalar `soldPrice` when the sale
    was not itemised.
    """
    option = record.get("soldOption")
    gold = 0
    extras: list[str] = []
    if isinstance(option, Mapping):
        for item in as_seq(cast(Mapping[str, object], option).get("items")):
            if not isinstance(item, Mapping):
                continue
            entry = cast(Mapping[str, object], item)
            quantity = entry.get("quantity")
            quantity = quantity if isinstance(quantity, int) else 0
            if entry.get("isGold") or entry.get("materialId") == GOLD_ID:
                gold += quantity
                continue
            material = entry.get("material")
            name = (
                cast(Mapping[str, object], material).get("name")
                if isinstance(material, Mapping)
                else None
            )
            label = name if isinstance(name, str) else "?"
            extras.append(f"{quantity}x {label}")
    if gold == 0:
        fallback = record.get("soldPrice")
        if isinstance(fallback, int):
            gold = fallback
    return gold, tuple(extras)


def timestamp_of(record: Mapping[str, object]) -> datetime | None:
    """When a listing was posted, from relistedAt/createdAt. None if neither."""
    for key in ("relistedAt", "createdAt"):
        raw = record.get(key)
        if isinstance(raw, str):
            try:
                return datetime.fromisoformat(raw)
            except ValueError:
                continue
    return None


def summarise(amounts: Sequence[int]) -> PriceStats | None:
    """Median-first summary. None when there is nothing to summarise.

    The trimmed mean drops everything outside the interquartile fence, which is
    what keeps a single 5B bundle from setting the price target.
    """
    values = sorted(a for a in amounts if a >= MIN_REAL_GOLD)
    if not values:
        return None
    p25 = _percentile(values, 0.25)
    p75 = _percentile(values, 0.75)
    fence = 1.5 * (p75 - p25)
    kept = [v for v in values if p25 - fence <= v <= p75 + fence] or values
    return PriceStats(
        count=len(values),
        median=int(statistics.median(values)),
        trimmed_mean=int(statistics.fmean(kept)),
        p25=int(p25),
        p75=int(p75),
        low=values[0],
        high=values[-1],
    )


@dataclass(frozen=True, slots=True)
class PriceModel:
    """A robust, recency-weighted read of what something is worth.

    `fair` is the number to negotiate against. `low`/`high` bound the normal one
    sigma band. `floor` sits two sigma down and is the only line that means
    "distrust this": below it a price is more likely a stale listing, a
    mispriced stack, or a bait ad than a bargain.
    """

    fair: int
    median: int
    floor: int
    low: int
    high: int
    used: int
    effective: float
    rejected: tuple[int, ...]
    ratio_spread: float

    @property
    def total(self) -> int:
        return self.used + len(self.rejected)

    def classify(self, amount: int) -> str:
        """Where an asking price sits against the model.

        Cheap is not suspicious. One sigma below centre is where roughly one in
        six honest sales lands, so flagging that band would warn on exactly the
        listings worth buying. Only the two-sigma tail earns the doubt.
        """
        if amount < self.floor:
            return "suspicious"
        if amount < self.low:
            return "bargain"
        if amount <= self.fair:
            return "good"
        if amount <= self.high:
            return "fair"
        return "over"


def _log_scale(logs: Sequence[float], centre: float) -> float:
    """Robust sigma of log-prices, with a fallback when the MAD collapses.

    The MAD is zero whenever more than half the sales share one price, which
    round-number rune pricing makes common. Falling back to the mean absolute
    deviation keeps a usable scale instead of rejecting everything that is not
    exactly the median.
    """
    deviations = [abs(x - centre) for x in logs]
    mad = statistics.median(deviations)
    if mad > 0:
        return MAD_TO_SIGMA * mad
    mean_ad = statistics.fmean(deviations)
    return MEANAD_TO_SIGMA * mean_ad if mean_ad > 0 else 0.0


def _weight(sale: Sale, now: datetime, half_life_days: float) -> float:
    """Exponential time decay. Undated sales are discounted, not dropped."""
    if sale.listed_at is None:
        return 0.5**UNDATED_PENALTY_HALF_LIVES
    age_days = max(0.0, (now.timestamp() - sale.listed_at.timestamp()) / 86400)
    return 0.5 ** (age_days / half_life_days)


def effective_sample_size(weights: Sequence[float]) -> float:
    """Kish's effective sample size: (sum w)^2 / sum(w^2).

    How many equally-weighted sales the weighted estimate is really worth. With
    decay weighting the raw count flatters badly - 168 sales spread over months
    can carry the statistical weight of half a dozen, and reporting 168 would
    present a handful of recent trades as a deep market.
    """
    total = sum(weights)
    squares = sum(w * w for w in weights)
    return (total * total / squares) if squares > 0 else 0.0


def weighted_median(values: Sequence[float], weights: Sequence[float]) -> float:
    """Median of `values` under `weights`. Both must be the same length.

    A weighted median rather than a weighted mean: the whole point of this
    module is that a mean is hostage to one extreme sale, and weighting does
    nothing to fix that.
    """
    if not values:
        raise ValueError("no values to take a weighted median of")
    pairs = sorted(zip(values, weights, strict=True))
    total = sum(w for _, w in pairs)
    if total <= 0:
        return statistics.median([v for v, _ in pairs])
    seen = 0.0
    for value, weight in pairs:
        seen += weight
        if seen >= total / 2:
            return value
    return pairs[-1][0]


def fit(
    sales: Iterable[Sale],
    *,
    now: datetime,
    half_life_days: float = HALF_LIFE_DAYS,
    threshold: float = OUTLIER_Z,
) -> PriceModel | None:
    """Fit a price model to completed sales. None when there is no evidence.

    Works in LOG space throughout. Prices here are multiplicative - the gap
    between 70M and 140M is the same kind of move as between 700M and 1.4B -
    so a linear outlier fence is wrong twice over: it waves through cheap bait
    and it treats every large sale as extreme. Rejecting on the log-space MAD
    instead makes the test scale-free, and it is what stops a single 5B bundle
    or a 6.75M stale ad from moving the target.
    """
    usable = [s for s in sales if s.gold >= MIN_REAL_GOLD]
    if not usable:
        return None

    logs = [math.log(s.gold) for s in usable]
    centre = statistics.median(logs)
    sigma = _log_scale(logs, centre)

    if sigma > 0:
        keep = [
            s
            for s, x in zip(usable, logs, strict=True)
            if abs(x - centre) <= threshold * sigma
        ]
        drop = [
            s.gold
            for s, x in zip(usable, logs, strict=True)
            if abs(x - centre) > threshold * sigma
        ]
    else:
        # Every sale is at one price; nothing can be an outlier.
        keep, drop = list(usable), []
    if not keep:  # threshold too tight for this shape - trust the data instead
        keep, drop = list(usable), []

    kept_logs = [math.log(s.gold) for s in keep]
    kept_centre = statistics.median(kept_logs)
    kept_sigma = _log_scale(kept_logs, kept_centre)
    weights = [_weight(s, now, half_life_days) for s in keep]

    # The band is centred on `fair`, not on the plain median. Spread is a
    # property of the market so it is measured unweighted, but hanging it off
    # the unweighted centre would let the band drift away from the target it is
    # meant to describe - and if recent sales ran cheap, `fair` could end up
    # below `low`, so a price at exactly the target would classify as a bargain.
    fair_log = weighted_median(kept_logs, weights)

    # round, never truncate: exp(log(50_000_000)) is 49999999.999..., and int()
    # would report every round price one gold light.
    return PriceModel(
        fair=round(math.exp(fair_log)),
        median=round(math.exp(kept_centre)),
        floor=round(math.exp(fair_log - SUSPICION_SIGMAS * kept_sigma)),
        low=round(math.exp(fair_log - kept_sigma)),
        high=round(math.exp(fair_log + kept_sigma)),
        used=len(keep),
        effective=effective_sample_size(weights),
        rejected=tuple(sorted(drop)),
        ratio_spread=math.exp(kept_sigma),
    )


def _percentile(values: Sequence[int], fraction: float) -> float:
    """Linear-interpolated percentile over a pre-sorted sequence."""
    if len(values) == 1:
        return float(values[0])
    position = fraction * (len(values) - 1)
    lower = int(position)
    upper = min(lower + 1, len(values) - 1)
    weight = position - lower
    return values[lower] * (1 - weight) + values[upper] * weight


def recent(sales: Iterable[Sale], *, days: int, now: datetime) -> list[Sale]:
    """Sales listed within the last `days`. Undated sales are excluded."""
    cutoff = now.timestamp() - days * 86400
    return [
        s
        for s in sales
        if s.listed_at is not None and s.listed_at.timestamp() >= cutoff
    ]


def utcnow() -> datetime:
    return datetime.now(UTC)
