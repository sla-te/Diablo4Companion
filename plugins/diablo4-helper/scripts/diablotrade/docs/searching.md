# Creating a search without the browser

`GET /api/search/<shortId>` reads a saved search. Nothing *creates* one over
REST - the site does it through a Next.js Server Action. This is the record of
what was captured and what it means.

## Status

`diablotrade.search.create_search` **works**, given an authenticated client.
Verified 2026-07-26 against a re-taken capture.

Two things had to be fixed, and only the first was the HTTP 500:

1. The argument encoding was incomplete (see below).
2. The action requires a logged-in session. Anonymous callers get a 303 with
   `X-Action-Redirect: /session-expired;replace`, and `GET /api/session` returns
   `{"status":"unauthenticated"}` - the site mints no anonymous session at all.
   `create_search` now refuses a cookie-less client up front rather than letting
   that 303 masquerade as a protocol bug.

## What was probed and ruled out

| Route | Result |
|---|---|
| `POST /api/search` | 404 |
| `GET /api/search` | 404 |
| `POST /api/listing/search` | 404 |
| `GET /api/listing/search?...` | 404 |
| `POST /api/listing/get` | 405 (GET only) |

So there is no REST creation route to find. The Server Action is the only path.

## The capture

Install a `window.fetch` interceptor on an open `/listings/items` page, press
SEARCH once, and read back what it recorded. **Record the whole body, not just
`body.entries()`** - reading only the entries is what hid the bug for a whole
session:

```js
window.__cap = [];
const real = window.fetch;
window.fetch = async (input, init) => {
  try {
    if (init && init.method === "POST") {
      const url = typeof input === "string" ? input : input.url;
      const headers = Object.fromEntries(new Headers(init.headers ?? {}).entries());
      delete headers.cookie; delete headers.authorization;
      const fields = init.body instanceof FormData ? [...init.body.entries()] : null;
      // the serialized body, which is what actually goes on the wire
      const raw = init.body instanceof FormData
        ? await new Response(init.body).text()
        : init.body;
      window.__cap.push({ url, headers, fields, raw });
    }
  } catch (e) { /* never break the page over instrumentation */ }
  return real(input, init);
};
```

Never write the captured `cookie` / `authorization` values anywhere. The snippet
above drops them before they are ever stored.

What that produced on 2026-07-26:

```text
POST /listings/items          <- the bare page, even while showing a saved search
  accept: text/x-component
  next-action: 6011b8ff26e7e4a3e8a8fb98460750d031dfceda87
  next-router-state-tree: <URL-encoded JSON route tree>
  x-deployment-id: <deploy hash>
  multipart/form-data with THREE fields:
    _1_input      = the filter JSON
    _1_requestId  = "1"
    0             = [{"error":""},"$K1"]
```

The action id is build-specific and rotates on every deploy. When it rotates,
re-capture rather than assuming the site changed shape.

## Why the 500 happened

React encodes Server Action arguments as form fields, and **field `"0"` is the
argument array**. Here it is `[{"error":""},"$K1"]`:

- `{"error":""}` is the previous state - this action is a `useActionState`
  handler, so its signature is `(previousState, formData)`.
- `"$K1"` is React's reference to a FormData argument. That FormData's own
  entries are flattened into sibling fields named `_1_<key>`, which is where
  `_1_input` and `_1_requestId` come from.

The first capture recorded only `_1_input`, so the request carried no field
`"0"`. The server-side decoder therefore reconstructed no arguments, the action
threw, and Next returned a 500 whose body was the RSC error stream
`1:E{"digest":"..."}` - a digest with no message, because the deployment is in
production mode.

That digest-only body is itself the useful signal: **an RSC error stream means
the action was found and executed**. A wrong action id or a malformed envelope
does not get that far. So the 500 was always about the arguments, never about
the envelope - which is the opposite of what this document used to claim, and
the cost of a transport that raised on the status code before anyone read the
body. Read the body first.

## Reading the outcome

On success the action answers `200` with a short RSC body containing
`/listings/items/<newShortId>`; `actions.extract_short_id` pulls it out. There
is no `X-Action-Redirect` on the success path - that header only appears on the
`/session-expired` bounce.

Identical filters appear to resolve to the same short id rather than minting a
new one each time, so repeated calls are cheap.

## Pagination: there is none

`/api/search/<shortId>` returns at most **500** listing ids and there is no way
past it. Probed directly: `?page=2`, `?offset=500`, `?limit=1000`, `?skip=500`
and `?cursor=500` are all ignored and return the identical 500 ids. The only
`nextCursor` anywhere in the site's bundles belongs to the "what's new"
notifications feed, not to search.

So a search that comes back with exactly 500 is truncated, and its total is a
floor rather than a count. `SavedSearch.is_truncated` reports it and the CLI
warns on stderr. The fix is a narrower filter, not a second request.

## `isAncestral: false` empties every search

Measured 2026-07-28 by bisecting the filter object against a browser-made
search: the identical payload returned **182 hits without the key and 0 with
it**. It is not a tri-state - sending `false` does not mean "don't care", it
means "no results". Every other key tested (`excludeGenericClass`,
`favoritesOnly`, `itemRarityExclude`, `sortAttributeDirection`, `listPeriod`,
`itemCategory`) was harmless.

This sat in `DEFAULT_FILTERS` and silently zeroed every programmatic search,
which is exactly the failure mode the module docstring warns about: an empty
result set is indistinguishable from a filter that matched nothing. It now lives
in `OPTIONAL_FILTERS`, is never sent by default, and `build_filters` raises on
`isAncestral=False` rather than letting it travel.

Corollary worth keeping: identical filters resolve to the same short id, so a
Python-built payload that reproduces a browser search returns that search's id -
which is a free way to prove the two payloads are equivalent.

## Sold listings: `statusFilters: ["sold"]`

Completed sales are readable, and they are the only honest price signal - an ask
is an opinion. `statusFilters: ["sold"]` returns them; the value is
case-sensitive (`"SOLD"` is rejected outright, minting no search at all), and
`["online", "offline", "sold"]` returns live and sold together.

Each sold listing carries:

| Field | Meaning |
|---|---|
| `soldPrice` | scalar sale amount |
| `soldOption.items[]` | what was actually handed over, itemised, with `isGold` |
| `createdAt` / `relistedAt` | when it was LISTED - there is no sold-at timestamp |

Two traps in that data, both handled in `diablotrade.prices`:

1. **The tail is enormous.** Observed IGNI sales ran 125M to 5B in one week, so
   a mean is meaningless. Summaries are median-first with an IQR-trimmed mean.
2. **`soldPrice` is not always the whole price.** A sale settled as "200M + 1
   Jah" reports `soldPrice: 200000000` and puts the rune in `soldOption.items`.
   Averaging that in understates the clearing price, so mixed settlements are
   reported separately rather than counted as gold sales.

Recency is therefore "how recently was it listed", not "how recently did it
sell" - a proxy, and labelled as one.

## listPeriod is a free-form string

The site's own zod schema types it `z.string().optional()` - there is no enum to
enumerate, so the value set cannot be recovered from the bundle. Only `""` (no
age limit) has been observed in a real payload. An unrecognised value is ignored
silently, which reads as "no results" rather than as an error, so confirm any
value against a real capture before trusting it.

## The site supports "at least N of M" natively

An OR group carries `minMatches` / `maxMatches`; the UI builds them as
`{type:"or", minMatches:"1", maxMatches:"", affixes:[]}`, and the group
normaliser round-trips `{id, type, minMatches, maxMatches, affixes, disabled}`.
`search.stat_group(..., mode="or", min_matches="3")` emits that shape, so a
3-of-4 rule is one group server side rather than the six pairs
`filters.as_or_groups` builds for the click-it-by-hand route.
