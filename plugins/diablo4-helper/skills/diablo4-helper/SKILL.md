---
name: diablo4-helper
description: >-
  This skill should be used for Diablo 4 (D4) gameplay questions - "best build
  for", "is this item an upgrade", "should I reroll", "which prism", "how do I
  craft X", "what season is it", "how to get <unique/mythic>" - covering builds,
  items, aspects, the Horadric Cube, tempering, masterworking, enchanting,
  charms, glyphs, the Pit, farming routes, loot filters, diablo.trade, and
  D4Companion. D4 patches quarterly, so every version-specific fact must be
  verified against live sources before it is stated. Not for other ARPGs (Path of
  Exile, Last Epoch) or Diablo lore.
version: 0.2.0
---

# Diablo 4 Helper

## Purpose

Answer Diablo 4 questions correctly despite a training cutoff that is always
behind the live game. D4 ships a new season with reworked mechanics, items, and
balance roughly every three months, plus mid-season hotfixes. Any answer stated
from memory about the current season, an item's stats, a crafting recipe, a
build's stat priority, or a farming route is likely stale and has already caused
wrong answers (see `references/traps.md`).

The single rule this skill enforces: **treat every season-specific,
item-specific, build-specific, or patch-specific fact as unknown until verified
online this session.** Stable ARPG concepts (what a multiplicative bucket is,
that Whirlwind hits many times per second) may be reasoned from directly. The
version-dependent specifics may not.

## The one hard rule

Before stating any of the following, verify it online **this session** - do not
answer from training memory:

- The current season number, name, or theme
- Any item's affixes, unique effect, power roll, or item power
- Any crafting recipe, material cost, or which Tuning Prism to use
- Any build's skill/stat/gear priority or "is X an upgrade" verdict
- Any farming route, boss ladder, drop source, or loot-filter recommendation
- Whether a mechanic still works the way a prior patch described

If a fast lookup cannot confirm it, say so plainly - "I couldn't verify this for
the current patch" - rather than filling the gap from memory. A confident wrong
answer about a patched mechanic is the failure this skill exists to prevent.

## Confidence tagging is mandatory

The five researched references - `itemization.md`, `damage-model.md`,
`affix-pools.md`, `crafting-decisions.md`, `barbarian-whirlwind.md` - tag every
claim `[Certain]`, `[Likely]` or `[Unverified]`, and each ends with a `Sources`
and a `Gaps` section. Carry those tags through into answers. A `[Likely]` fact
must not be presented to the player as settled, because they spend irreversible
materials on what you tell them.

`horadric-cube.md`, `traps.md` and `sources.md` are **untagged** - they are a
dated recipe snapshot, a procedural guardrail list and a source index
respectively. Treat the recipe snapshot's costs and seasonal recipes as
`[Likely]` and re-verify before a player spends on them.

See "Known contradictions" below for the live disputes.

## Question routing - open the right reference first

| The question is about | Open |
|---|---|
| Item quality, Ancestral, Item Power, Greater Affixes, masterworking, tempering counts, enchant rules, Mythics 3.0 | `references/itemization.md` |
| Damage buckets, `[x]` vs `[+]`, stat scaling, crit/vulnerable/DoT, armour, resistances, Toughness, Maximum Life interactions | `references/damage-model.md` |
| Every Horadric Cube recipe and its cost | `references/horadric-cube.md` |
| Tuning Prism categories, which prism does what, per-slot affix pools, what a slot can legally roll | `references/affix-pools.md` |
| "Which crafting system do I use to achieve X", order of operations, what bricks an item | `references/crafting-decisions.md` |
| Barbarian, Arsenal, weapon expertise, Whirlwind stat priority, Mythic build variants | `references/barbarian-whirlwind.md` |
| Where to look something up, query templates | `references/sources.md` |
| Recurring reasoning failures to avoid | `references/traps.md` |

Read the reference **before** searching the web. It may already answer the
question, and it tells you what is disputed so the web lookup can target the gap
rather than re-covering settled ground.

## How to research (routing)

The entry point for every lookup is the `research` skill - never a bare
`WebSearch`/`WebFetch`, and never a bare `tvly`/`tavily-*` call chosen directly.
This is the machine-wide research rule and it is not overridden here. For a D4
web question `research` routes to `tavily-dynamic-search`, which filters results
inside a Python process so raw guide HTML never floods context.

`scripts/d4_search.py` is **not a way around that routing** - it is the concrete
D4-scoped form of the `tavily-dynamic-search` pattern, the same tvly-in-Python
method that skill already uses, with D4's authoritative domains and filtering
pre-applied. Once `research` has landed on the `tavily-dynamic-search` step for a
D4 query, run this script as that step's execution rather than hand-writing the
Python each time. Run it with `uv run` (bare `python3` is blocked on this
machine):

```bash
uv run python3 "${CLAUDE_PLUGIN_ROOT}/scripts/d4_search.py" \
    "<class> <skill> build gear priority season <N>" --extract
```

`${CLAUDE_PLUGIN_ROOT}` is the plugin's install directory, set by Claude Code.
If it is empty in your shell, the script sits at `scripts/d4_search.py` under this
plugin's root (the directory two levels up from this `skills/diablo4-helper/`
folder) - resolve and run it from there.

Without `--extract` it returns triaged titles + snippets (cheap); with
`--extract` it pulls and filters the top pages. Add `--official` to restrict to
Blizzard news/forums. See the script header for all flags. For anything outside
this D4-scoped pattern, let `research` pick the tool per `references/sources.md`.

**Always confirm the current season and patch first** when a question is
season-scoped and the patch has not already been established this session - use
`--official` for that one lookup, since Blizzard news is the only authority for
what is live. Everything else hangs off it.

**Known limitation of the script:** its line filter drops short generic table
rows, which is why per-slot affix tables ("+X% Attack Speed | All") extract
poorly. When you need a dense reference table, fetch the page and read it rather
than relying on the filtered output, and say so.

**Two site-specific warnings.** Icy Veins build pages frequently extract as walls
of image URLs; prefer maxroll.gg and mobalytics for build data. Blizzard's
`news.blizzard.com` has failed extraction through this script before - fall back
to `tavily-extract` on the article URL directly.

## Source hierarchy (what to trust for what)

Full detail in `references/sources.md`. Short version, most authoritative first:

| Need | Go to |
|---|---|
| Whether a mechanic/number is current | **Official Blizzard patch notes / news** |
| Builds, stat priorities, planners, tier lists | **Maxroll.gg** (then Mobalytics, Icy Veins) |
| Item/unique/aspect data, crafting recipes | **Maxroll**, D4 wiki (fextralife), game8 |
| Edge cases, "does this still work", bug reports | **r/diablo4**, official forums |

Prefer official patch notes for "is this still true," Maxroll for "how should I
build/gear." Cross-check a single-source claim against a second source before
stating it as fact. Note the source's date - a guide written for an earlier
season may not have been updated.

## Known contradictions - do not silently pick a side

Never silently pick a side of a dispute. Say it is disputed, then give the player
the cheapest in-game experiment that settles it. The evidence lives in the
reference that owns each one - open it, do not answer from this index:

- Which side a Tuning Prism names on Chaotic Reroll, victim or replacement -
  `horadric-cube.md` warning block, `affix-pools.md`
- Ancestral / Mythic Item Power ceiling, 800 vs 900 vs 925 - `itemization.md`
- Whether Damage Over Time is a dead stat on a 2H Sword Barbarian -
  `damage-model.md`, `barbarian-whirlwind.md`

One entry is **not** disputed but is listed because most sources are stale on it:
the **crafted-Mythic equip limit was removed in patch 3.1.1a**. Any number may be
worn at once. If a guide states the old one-at-a-time limit, the guide is out of
date - that alone is a reason to distrust the rest of the page.

## Recurring tasks

### Grading gear ("is this an upgrade?")

Pull the exact build guide and variant (e.g. Midgame vs Endgame) live, score each
item's stat-priority match, rank by gap, and state plainly that stat-match is not
a DPS simulation. See `references/traps.md` #4.

Then run the crafting layer, because an item is not judged only on what it is:

1. Run the checklist at the end of `references/horadric-cube.md`. The item may be
   crafting fodder, a three-of-a-kind input, or one recipe away from the item
   actually wanted. A wanted item the player does not own may be craftable rather
   than farmable.
2. **Assume the Occultist is always available.** Grade the item BODY - base
   DPS/armour, affix lines, temper budget, sockets, item power, slot - never the
   aspect it happened to drop with, since imprinting replaces that at will.
3. **Count the repair budget before recommending anything.** Per item the player
   gets exactly **one** enchanted affix, **one** tempering affix, and Cube
   rerolls whose output they cannot choose. An item must arrive with nearly
   everything right; you fix one thing. This is the single most useful frame for
   "is it worth buying/keeping".
4. Before recommending a body swap, check whether an **enchant on the currently
   equipped item** closes the same gap while keeping its investment (masterwork
   rank, sockets, temper).

### Planning a craft

Go to `references/crafting-decisions.md` first, not to the recipe table. The
question "which recipe" is downstream of "what is the goal, and what order do I
do things in". Always state:

- Which step is **irreversible** and what it forecloses.
- What the player is **spending** (the bottleneck resource, not just the gold).
- The **stop condition** - the point at which further rolling can only lose value.

### Buying gear (diablo.trade)

The plugin bundles a full diablo.trade client at
`${CLAUDE_PLUGIN_ROOT}/scripts/diablotrade`. It is a self-contained uv project,
not a loose script, so it must be invoked through `uv run --project`. It is never
on `PATH`:

```bash
uv run --project "${CLAUDE_PLUGIN_ROOT}/scripts/diablotrade" \
    diablotrade <subcommand> [args]
```

The first such call resolves and installs its one runtime dependency
(`curl_cffi`) automatically; no separate `uv sync` step is needed. The
subcommands are `learn`, `filter`, `enchant`, `groups`, `aspects`, `market` and
`actions`; run it with `--help` for the full usage block. Read
`${CLAUDE_PLUGIN_ROOT}/scripts/diablotrade/README.md` and
`${CLAUDE_PLUGIN_ROOT}/scripts/diablotrade/docs/searching.md` before a first
real search.

The key idea is already baked into that tool and matches the repair-budget frame
above: the query you want is **"at least N of these M affixes"**, because an item
missing exactly one wanted affix is still worth buying - the Occultist fixes one.
`diablotrade enchant` prices that fix, separating items with a junk affix to
overwrite (free) from items whose only spare roll is a Greater Affix (you destroy
a Greater Affix to fix it) from items with no spare roll at all.

Workflow: define the wanted affix set from the build guide, search, filter by
match count, then rank by enchant cost - never by match count alone.

`DIABLO_COOKIE` must be exported for session-authenticated operations. Never pass
a cookie on the command line.

### D4Companion overlay

The user runs a fork whose overlay marks affixes against an imported Maxroll
build. Key trap: a **red mark means "not a tracked stat priority," not "the build
does not want this item"** - confirm against the guide, not the colour. See
`references/traps.md` #2.

The overlay preset may also be **mislabelled**: check whether the imported
profile matches its display name before trusting its marks, and check whether the
player is running a Mythic build variant that was never imported.

### The player's gear journal

`loadout/STATE.md` at the repo root is a hand-maintained, gitignored snapshot of
the player's current gear, stash and open decisions. Read it before answering a
gearing question, and update it when gear changes. `loadout/README.md` has the
record template. It is gitignored on purpose - never commit it, and keep
real-life personal details out of it regardless.

## Additional resources

Both live under `${CLAUDE_PLUGIN_ROOT}/scripts/`, which is the **plugin** root -
one level above this skill directory, so a bare relative `scripts/...` path from
here will not resolve. Always use `${CLAUDE_PLUGIN_ROOT}`.

- **`${CLAUDE_PLUGIN_ROOT}/scripts/d4_search.py`** - D4-scoped
  tavily-dynamic-search helper. Run with `uv run python3`.
- **`${CLAUDE_PLUGIN_ROOT}/scripts/diablotrade/`** - diablo.trade search,
  filtering, enchant-cost pricing and market analysis. A uv project; run with
  `uv run --project`. Has its own README, docs and test suite.
