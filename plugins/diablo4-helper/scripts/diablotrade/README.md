# diablotrade

Programmatic search, filtering and listing tools for [diablo.trade](https://diablo.trade).
Stdlib only, `src` layout, no runtime dependencies.

## Why this exists

diablo.trade's affix filter is built from AND / OR / NOT groups. That cannot
express **"at least N of these M affixes"** in a single group, which is the query
you actually want when hunting gear: an item missing exactly one wanted affix is
still worth buying, because the Occultist can enchant one affix per item.

Expressed as groups, "at least 3 of 4" is an AND over all six pairs - six groups
clicked by hand, and it gets worse as M grows. Pulling the listings and filtering
locally is both easier and more expressive.

## What the site actually exposes

Established by watching the site's own network traffic and grepping its JS
bundles, not from documentation - diablo.trade publishes none.

| Operation | Mechanism | Usable from a script |
|---|---|---|
| Read a saved search | `GET /api/search/<shortId>` | yes, anonymously |
| Hydrate listings | `GET /api/listing/get?ids=a,b,c` | yes, anonymously, batches of 50 |
| Create a search | Next.js Server Action | awkward, see below |
| Post a listing | Next.js Server Action | awkward, see below |

There is no REST route for writes. `/listings/create` loads 67 script chunks
whose only `/api/` references are chat and session; the write path runs through
`createServerReference("<40 hex>")` Server Actions instead.

## Usage

Build one **broad** search in the site UI (just the item, no affix filter), press
Search, and copy the short id from the URL `diablo.trade/listings/items/<id>`.
Everything else happens here.

```bash
# Which attributes does this unique actually roll, and how often?
diablotrade learn 3UcbpB --unique b11a2744-6331-4356-b608-b71e2a30f18d

# Rank listings by how many wanted attributes they carry
diablotrade filter 3UcbpB --attrs STR,LIFE,FURY,CRIT --min-matches 3

# Same rule as OR-groups, if you would rather rebuild it in the site UI
diablotrade groups --attrs STR,LIFE,FURY,CRIT --min-matches 3

# Rank aspect carriers by BASE roll, so an amulet's 1.5x range does not win
diablotrade aspects 2H96O4 --max-price 100000000
```

As a library:

```python
from diablotrade import Client, filters, metadata

client = Client()
listings = client.search_listings("3UcbpB")

# Guaranteed affixes are dead weight in a filter - drop them.
for attribute_id in metadata.guaranteed(listings):
    print("every copy has", attribute_id)

hits = filters.rank(listings, wanted_uuids, minimum=3)
```

## Attribute names

Affixes come back as `{"attributeId": "<uuid>", "displayText": "", ...}`. The
names live in an encrypted blob the site keeps in IndexedDB, so this package
works in UUIDs. Two ways to attach names:

1. **Exact.** Open the Affixes Filter on the site, type a stat name, and run
   `metadata.BROWSER_UUID_SNIPPET` in the devtools console. Options come back as
   `"2843c662-6fca-4e43-8a40-ec8b88b138dc + Strength"`.
2. **Approximate, no browser.** `diablotrade learn` reports per-attribute
   frequency. Since Patch 3.1.0 every Unique has exactly 2 guaranteed affixes and
   randomises the rest, so attributes at 100% are the guaranteed pair. That
   identifies the slots without naming them, which is usually enough.

## Searching without the browser

`diablotrade.search` builds the site's exact filter payload from Python
(`build_filters`, `stat_group`) and holds the captured Server Action contract for
creating a search. **`create_search` does not work yet - it returns HTTP 500.**
See [docs/searching.md](docs/searching.md) for what was captured, what has been
ruled out, and what to try next. Until it works, create searches in the site UI
and drive everything after that from here.

## Posting

Not wired up, deliberately. See [docs/posting.md](docs/posting.md). The transport
and action-id discovery are implemented and working; what is missing is the
mapping from action id to "create listing", which cannot be determined statically
and is not something to guess at when the consequence is posting under your
account.

## Development

```bash
uv venv && uv pip install -e ".[dev]"
uv run pytest
```

Tests are offline - every fixture is a literal payload.
