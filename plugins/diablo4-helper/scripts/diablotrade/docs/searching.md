# Creating a search without the browser

`GET /api/search/<shortId>` reads a saved search. Nothing *creates* one over
REST - the site does it through a Next.js Server Action. This is the record of
what was captured, what works, and what does not.

## Status

`diablotrade.search.create_search` returns **HTTP 500**. Do not assume it works.
The parts that are verified and reusable:

- `DEFAULT_FILTERS` - the exact 33-key filter object the site sends
- `stat_group()` / `build_filters()` - build that object correctly
- `Client.get_search(shortId)` - reading a search created in the UI

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
SEARCH once, and read back what it recorded:

```js
window.__cap = [];
const real = window.fetch;
window.fetch = async (input, init) => {
  try {
    const url = typeof input === "string" ? input : input.url;
    const headers = Object.fromEntries(new Headers(init?.headers ?? {}).entries());
    const fields = init?.body instanceof FormData ? [...init.body.entries()] : null;
    window.__cap.push({ url, headers, fields });
  } catch (e) { /* never break the page over instrumentation */ }
  return real(input, init);
};
```

What that produced on 2026-07-26:

```
POST /listings/items/<previousShortId>
  accept: text/x-component
  next-action: 6011b8ff26e7e4a3e8a8fb98460750d031dfceda87
  next-router-state-tree: <URL-encoded JSON route tree>
  x-deployment-id: <deploy hash>
  multipart/form-data, one field "_1_input" = the filter JSON
```

The action id is build-specific and rotates on every deploy. When it rotates,
re-capture rather than assuming the site changed shape.

## What has already been tried against the 500

1. **A subset of the filter keys.** Rejected by `build_filters` as unknown-key
   errors first, then sent in full. No change.
2. **Omitting `next-router-state-tree`.** Also 500, so the header is not the
   fix - but it is required in the real request, so it stays.
3. **Sending it URL-encoded via `quote()`.** Still 500.

## What to try next

- Capture the request **body bytes verbatim** (not just the FormData entries)
  and replay them byte-for-byte from Python. A boundary or a field-ordering
  difference would not show up in the `entries()` view.
- Compare the full outgoing header set, including the ones the browser adds
  automatically, against what `Client.post_raw` sends.
- POST to `/listings/items/<previousShortId>` (what the browser actually does)
  rather than the bare `/listings/items`.
- Read the 500 response body - Next often puts the real cause in it, and
  `post_raw` currently raises on the status code before anyone looks.

## The pagination question, unanswered

`/api/search/<shortId>` returns at most **500** listing ids. Whether the site
can reach past that, and how it pages, has not been established - the results
page has not been scrolled to the bottom to watch what it requests. Until that
is checked, treat 500 as a possible truncation, not a known total.
