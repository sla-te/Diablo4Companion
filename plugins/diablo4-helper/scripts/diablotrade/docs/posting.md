# Posting listings

## Why this is not wired up

diablo.trade has no REST route for creating a listing. `/listings/create` loads
67 client bundles whose only `/api/` references are chat and session endpoints.
Writes run through **Next.js Server Actions**: a POST to the page URL carrying a
`Next-Action` header whose value is a 40-hex id.

`diablotrade actions listings/create` currently reports **63** distinct action
ids on that page. Two properties make picking one unsafe:

- **They are build-specific.** The ids change on every deploy, alongside the
  `?dpl=` hash on every asset. Anything hardcoded rots.
- **They are unlabelled.** The bundle records that a server reference exists,
  not what it does. Nothing distinguishes "create listing" from "delete listing"
  or "report user" without calling it.

Calling the wrong one under your own account is not a guess worth making. So the
package ships the transport and the discovery, and stops short of the binding.

## Wiring it up from one captured request

You only need to do this once per site deploy, and `discover_action_ids` lets
you re-anchor afterwards.

1. Open devtools on diablo.trade, Network tab, preserve log on.
2. Create one real listing through the UI.
3. Find the `POST` to `/listings/create` (type: fetch, not a document load).
4. From **Headers**, copy the `Next-Action` value. That is your action id.
5. From **Payload**, copy the form fields. Next commonly passes arguments as a
   single field named `1` holding a JSON blob; confirm against what you see.

Then:

```python
from diablotrade import Client
from diablotrade.actions import ServerAction, invoke

client = Client(cookie="<your session cookie>")
action = ServerAction(page_path="/listings/create", action_id="<captured id>")

response = invoke(client, action, {"1": '<captured JSON payload>'})
print(response)   # React Server Component wire format, not JSON
```

`invoke` refuses to run without a cookie, because every write is account-scoped.

## Getting the session cookie

Devtools -> Application -> Cookies -> `https://diablo.trade`. Pass the whole
`name=value; name2=value2` string as `Client(cookie=...)`. Verify it took:

```python
client.session()   # returns your session object, empty/anonymous if the cookie is stale
```

Treat that cookie as a password. Do not commit it, do not paste it into a shared
file - pass it via an environment variable and read it at run time.

## Re-anchoring after a deploy

When the ids rotate, the captured id 404s or silently no-ops. Re-run:

```bash
diablotrade actions listings/create
```

If your previously captured id is gone from the list, the site redeployed and
you need one fresh capture. There is no way around this short of diablo.trade
publishing an actual API.
