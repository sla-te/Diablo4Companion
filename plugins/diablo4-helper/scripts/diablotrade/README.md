# diablotrade

Programmatic search, filtering and listing tools for [diablo.trade](https://diablo.trade).
`src` layout. One runtime dependency: `curl_cffi`, which replays a real
browser's TLS fingerprint, lets non-2xx bodies and redirect headers be
inspected instead of raised away, and keeps a cookie jar across requests. All
three matter for the Server Action path - see `src/diablotrade/client.py`.

## Why this exists

The query you actually want when hunting gear is **"at least N of these M
affixes"**: an item missing exactly one wanted affix is still worth buying,
because the Occultist can enchant one affix per item.

The site *can* express that - reading its bundles shows OR groups carry
`minMatches` / `maxMatches`, so 3-of-4 is one group server side. Two reasons to
filter locally anyway:

1. **Creating a search needs a logged-in session.** Reading one does not. The
   local route works anonymously against any short id.
2. **A match count is not the answer.** It ignores what the enchant costs, and
   the site cannot rank by that at all. See below.

`diablotrade groups` still prints the rule as the six OR pairs, for rebuilding
it by hand in the UI without touching `minMatches`.

A match count alone still overstates an item, because it ignores what the enchant
costs. `diablotrade enchant` prices that: it separates items with a junk affix to
overwrite (free) from ones whose only spare roll is a **Greater Affix** (you
destroy a GA to fix it) from ones carrying no junk affix at all, which are either
a free Add Affix into an empty slot or permanently stuck. The listing payload has
no slot count, so that last case is flagged for inspection rather than guessed at.
Inherents count toward matches but are never enchantable.

## What the site actually exposes

Established by watching the site's own network traffic and grepping its JS
bundles, not from documentation - diablo.trade publishes none.

| Operation | Mechanism | Usable from a script |
|---|---|---|
| Read a saved search | `GET /api/search/<shortId>` | yes, anonymously, **capped at 500 ids** |
| Hydrate listings | `GET /api/listing/get?ids=a,b,c` | yes, anonymously, batches of 50 |

The 500 cap has no way past it. `?page`, `?offset`, `?limit`, `?skip` and
`?cursor` are all ignored and return the identical 500, and the only `nextCursor`
in the site's bundles belongs to the notifications feed. A capped search is a
floor, not a count, so `SavedSearch.is_truncated` says so and the CLI warns.
Narrow the filter instead.
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

# Same, but priced by what the Occultist enchant actually costs you
diablotrade enchant 3UcbpB --attrs CDR,CRIT,VULN,CRITDMG

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
(`build_filters`, `stat_group`) and calls the site's Server Action to mint a
saved search, returning its short id for `Client.get_search` to hydrate.

**`create_search` needs a logged-in session.** The site issues no anonymous
session, so pass `Client(cookie=...)` with a browser session cookie; an
anonymous call is refused up front. Never persist that cookie to disk.
See [docs/searching.md](docs/searching.md) for the captured contract.

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
