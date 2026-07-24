---
name: diablo4-helper
description: >-
  Use for any Diablo 4 (D4) gameplay question - builds, uniques, mythics,
  aspects, tempering, masterworking, the Horadric Cube, charms/talismans, the
  Pit, glyphs, seasonal mechanics, farming routes, loot filters, boss ladders,
  or gear/item comparisons. D4 patches roughly every three months, so training
  knowledge is stale and wrong: this skill forces every version-specific answer
  to be verified against live online sources before it is stated. Triggers
  include "Diablo 4", "D4", "what season is it", "best build for", "how do I
  craft", "is this item an upgrade", "how to get <unique/mythic>", "current
  season mechanic", "loot filter", "D4Companion". Do NOT use for other ARPGs
  (Path of Exile, Last Epoch) or non-game Diablo lore questions.
version: 0.1.0
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
- Any crafting recipe or its material cost (Horadric Cube, tempering,
  masterworking, charm conversion)
- Any build's skill/stat/gear priority or "is X an upgrade" verdict
- Any farming route, boss ladder, drop source, or loot-filter recommendation
- Whether a mechanic still works the way a prior patch described

If a fast lookup cannot confirm it, say so plainly - "I couldn't verify this for
the current patch" - rather than filling the gap from memory. A confident wrong
answer about a patched mechanic is the failure this skill exists to prevent.

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

**Always confirm the current season first** when a question is season-scoped and
the season has not already been established this session - use the `--official`
flag for this one lookup, since Blizzard news is the only authority for what
patch is live. Everything else (mechanics, drop sources, balance) hangs off it.

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

## Two recurring tasks (full reasoning in `references/traps.md`)

- **Grading gear ("is this an upgrade?")** - pull the exact build guide and
  variant (e.g. Midgame vs Endgame) live, score each item's stat-priority match,
  rank by gap, and state that stat-match is not a DPS simulation. See traps.md #4.
- **D4Companion overlay** - the user runs a fork whose overlay marks affixes
  against an imported Maxroll build. Key trap: a **red mark means "not a tracked
  stat priority," not "the build does not want this item"** - confirm against the
  guide, not the colour. See traps.md #2 and the memory note
  `project-d4companion-overlay-capture-feedback`.

## Additional resources

- **`references/sources.md`** - full source list, what each covers, and ready
  query templates for common D4 question shapes.
- **`references/traps.md`** - specific, durable failure modes (stale season
  number, overlay red-mark misread, random-damage-unique + high-hit-rate
  synergy, damage-bucket confusion) with the correct reasoning for each.
- **`scripts/d4_search.py`** - D4-scoped tavily-dynamic-search helper.
