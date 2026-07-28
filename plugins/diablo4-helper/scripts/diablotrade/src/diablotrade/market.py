"""Price a material from completed sales, then find the live asks under target.

This is the whole loop in one place: mint a sold-search and a live-search for
one material, summarise what it actually sold for, and rank the current asking
prices against that number. No browser step - `search.create_search` mints both
searches, so a material only needs its id (see `materials`).
"""

from __future__ import annotations

from collections.abc import Iterable
from dataclasses import dataclass
from datetime import datetime

from .client import Client
from .prices import (
    HALF_LIFE_DAYS,
    MIN_REAL_GOLD,
    PriceModel,
    PriceStats,
    Sale,
    fit,
    parse_sales,
    summarise,
    timestamp_of,
)
from .search import build_filters, create_search

BATCH_SIZE = 50

# Below this many surviving sales, any centre is noise dressed as a number.
# Measured on ZAN 2026-07-28: a hard 7-day window left four sales and put the
# "target" at 60M against an all-time median of 30M over 169. Time-decay
# weighting replaced that window, but a thin sample still gets labelled.
MIN_CONFIDENT_SAMPLE = 8


@dataclass(frozen=True, slots=True)
class Ask:
    """One live listing, with the fields a buyer actually decides on."""

    listing_id: str
    gold: int
    seller: str
    listed_at: datetime | None
    online: bool

    @property
    def url(self) -> str:
        return f"https://diablo.trade/listings/{self.listing_id}"


@dataclass(frozen=True, slots=True)
class MarketReport:
    name: str
    model: PriceModel | None
    raw: PriceStats | None
    mixed_sales: tuple[Sale, ...]
    asks: tuple[Ask, ...]
    sold_search_id: str
    live_search_id: str
    online_only: bool = False
    dropped_offline: int = 0

    @property
    def target_is_thin(self) -> bool:
        """Too little real evidence to state a price with a straight face.

        Keys off the EFFECTIVE sample size, not the raw count: decay weighting
        means a long history can still rest on a handful of recent trades, and
        the raw count would hide exactly the case this warns about.
        """
        return self.model is not None and self.model.effective < MIN_CONFIDENT_SAMPLE

    @property
    def target(self) -> int | None:
        """What to aim to pay.

        This is the model's recency-weighted, outlier-rejected centre. There is
        no separate "recent window" any more: weighting by age subsumes it and
        avoids the cliff a hard cutoff creates, where a quiet week leaves the
        target resting on three sales.
        """
        return self.model.fair if self.model else None

    def bargains(self) -> tuple[Ask, ...]:
        """Live asks at or below target, cheapest first."""
        target = self.target
        if target is None:
            return self.asks
        return tuple(a for a in self.asks if a.gold <= target)


def _raw_rows(client: Client, ids: Iterable[str]) -> list[object]:
    listing_ids = [i for i in ids if i]
    rows: list[object] = []
    for start in range(0, len(listing_ids), BATCH_SIZE):
        batch = ",".join(listing_ids[start : start + BATCH_SIZE])
        payload = client.get_json(f"/api/listing/get?ids={batch}")
        if isinstance(payload, list):
            rows.extend(payload)
        elif isinstance(payload, dict):
            inner = payload.get("listings")
            if isinstance(inner, list):
                rows.extend(inner)
    return rows


def parse_asks(rows: Iterable[object]) -> list[Ask]:
    asks: list[Ask] = []
    for row in rows:
        if not isinstance(row, dict):
            continue
        if row.get("sold") or row.get("expired"):
            continue
        listing_id, price = row.get("id"), row.get("rawPrice")
        if not isinstance(listing_id, str) or not isinstance(price, int):
            continue
        if price < MIN_REAL_GOLD:
            continue  # barter / negotiable, not a gold ask
        raw_user = row.get("user")
        user: dict[str, object] = raw_user if isinstance(raw_user, dict) else {}
        seller = user.get("name")
        asks.append(
            Ask(
                listing_id=listing_id,
                gold=price,
                seller=seller if isinstance(seller, str) else "?",
                listed_at=timestamp_of(row),
                # `status` is the listing's own lifecycle (ACTIVE either way);
                # seller presence lives on the user record. Verified 2026-07-28
                # against online-only and offline-only searches.
                online=user.get("online") is True,
            )
        )
    return sorted(asks, key=lambda a: a.gold)


def report(
    client: Client,
    name: str,
    material_id: str,
    *,
    now: datetime,
    half_life_days: float = HALF_LIFE_DAYS,
    online_only: bool = False,
) -> MarketReport:
    """Build the full picture for one material.

    `online_only` restricts the live asks to sellers who are online, which is
    what makes a listing tradeable right now rather than eventually. It filters
    at BOTH ends on purpose: the search is minted with `statusFilters=["online"]`
    so the server narrows it, and the hydrated `user.online` is checked again.
    The server snapshots presence when the search is created, and it drifts -
    an offline-only search was observed returning a seller who had already come
    back online - so hydration is the fresher read of the two.

    Sold listings are never filtered this way. A completed sale is evidence
    whatever its seller is doing now, and dropping those sellers would thin the
    price model for no reason.
    """
    sold_id = create_search(
        client, build_filters(materialId=material_id, statusFilters=["sold"])
    )
    live_filters = build_filters(materialId=material_id)
    if online_only:
        live_filters = build_filters(materialId=material_id, statusFilters=["online"])
    live_id = create_search(client, live_filters)

    sales = parse_sales(_raw_rows(client, client.get_search(sold_id).listing_ids))
    pure = [s for s in sales if s.is_pure_gold]

    asks = parse_asks(_raw_rows(client, client.get_search(live_id).listing_ids))
    kept = [a for a in asks if a.online] if online_only else asks

    return MarketReport(
        name=name,
        model=fit(pure, now=now, half_life_days=half_life_days),
        raw=summarise([s.gold for s in pure]),
        mixed_sales=tuple(s for s in sales if s.gold and s.extras),
        asks=tuple(kept),
        sold_search_id=sold_id,
        live_search_id=live_id,
        online_only=online_only,
        dropped_offline=len(asks) - len(kept),
    )
