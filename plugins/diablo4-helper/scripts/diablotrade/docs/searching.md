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

## The pagination question, unanswered

`/api/search/<shortId>` returns at most **500** listing ids. Whether the site
can reach past that, and how it pages, has not been established - the results
page has not been scrolled to the bottom to watch what it requests. Until that
is checked, treat 500 as a possible truncation, not a known total.

## listPeriod, unconfirmed

Only `""` (no age limit) has been observed in a captured payload. The other
values in `search.LIST_PERIODS` are guesses. Open the "RECENT LISTINGS"
dropdown, pick each option, and read the resulting `_1_input` before relying on
them: the server ignores an unrecognised value silently, so a wrong one reads as
"no results" rather than as an error.
