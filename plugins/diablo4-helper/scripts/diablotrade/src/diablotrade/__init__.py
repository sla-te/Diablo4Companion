"""Programmatic search, filtering and listing tools for diablo.trade.

Quick start - find every Ramaladni's carrying at least 3 of 4 wanted affixes:

    from diablotrade import Client, filters

    client = Client()
    listings = client.search_listings("3UcbpB")   # short id from the site URL
    wanted = ["<str-uuid>", "<life-uuid>", "<fury-uuid>", "<critdmg-uuid>"]
    hits = filters.rank(listings, wanted, minimum=3)

Reading is anonymous. Writing (posting a listing) needs a session cookie and
goes through Next.js Server Actions - see `diablotrade.actions`.
"""

from __future__ import annotations

from . import actions, filters, metadata
from .client import (
    AuthRequiredError,
    Client,
    DiabloTradeError,
)
from .models import Affix, Item, Listing, SavedSearch

__all__ = [
    "Affix",
    "AuthRequiredError",
    "Client",
    "DiabloTradeError",
    "Item",
    "Listing",
    "SavedSearch",
    "actions",
    "filters",
    "metadata",
]

__version__ = "0.1.0"
