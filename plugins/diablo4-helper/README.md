# diablo4-helper

A Claude Code plugin for Diablo 4 crafting, itemization and gear-grading
decisions. Its purpose is narrow: D4 ships a reworked season roughly every three
months, so a model answering from training memory is confidently wrong about
recipes, affix pools and stat priorities - and the player pays for that in
irreversible materials. This plugin forces every version-specific claim to be
verified against live sources first, and tags what it could not verify.

## What is in it

A single skill, `diablo4-helper`, with eight reference files and two scripts.

| Reference | Covers |
|---|---|
| `itemization.md` | Item Power, Ancestral, Greater Affixes, Mythics 3.0, masterworking, tempering, enchanting |
| `damage-model.md` | Damage buckets, summed multipliers, stat scaling, armour and resistance curves |
| `affix-pools.md` | Tuning Prism categories, per-slot affix pools, what a slot can legally roll |
| `horadric-cube.md` | Every Cube recipe with its cost, plus the gear-grading checklist |
| `crafting-decisions.md` | "Which system do I use to get stat X", order of operations, what bricks an item |
| `barbarian-whirlwind.md` | Arsenal, weapon expertise, Whirlwind stat priority, Mythic variants |
| `sources.md` | Source hierarchy and query templates |
| `traps.md` | Durable reasoning failures with the correct reasoning for each |

The five researched references tag every claim `[Certain]`, `[Likely]` or
`[Unverified]` and end with `Sources` and `Gaps` sections. The other three are a
dated recipe snapshot, a source index and a guardrail list, and are untagged by
design.

Build coverage is currently **Barbarian / Whirlwind only**. Everything else -
itemization, the damage model, crafting - is class-agnostic.

## Scripts

Both live under `scripts/` at the plugin root, and both need
[uv](https://docs.astral.sh/uv/). Neither is on `PATH`; invoke them through uv.

**`scripts/d4_search.py`** - a D4-scoped form of the tavily-dynamic-search
pattern, with D4's authoritative domains and result filtering pre-applied. Needs
the `tvly` CLI authenticated.

```bash
uv run python3 "${CLAUDE_PLUGIN_ROOT}/scripts/d4_search.py" \
    "barbarian whirlwind gear priority season 14" --extract
```

**`scripts/diablotrade/`** - a diablo.trade client: search, affix filtering,
enchant-cost pricing, market analysis. A self-contained uv project, so run it
with `uv run --project`. Its one runtime dependency (`curl_cffi`) resolves on
first call; no separate `uv sync` needed.

```bash
uv run --project "${CLAUDE_PLUGIN_ROOT}/scripts/diablotrade" diablotrade --help
```

Its guiding idea is that an item missing exactly one wanted affix is still worth
buying, because the Occultist can enchant one affix per item. So the useful query
is "at least N of these M affixes", and listings are ranked by what that one
enchant would cost - not by raw match count. See
`scripts/diablotrade/README.md` and `scripts/diablotrade/docs/searching.md`.

Session-authenticated operations need a `DIABLO_COOKIE` environment variable.
Export it; never pass a cookie on the command line.

## Install

This repository is itself a Claude Code marketplace
(`.claude-plugin/marketplace.json` at the repo root).

```text
/plugin marketplace add <this-repo>
/plugin install diablo4-helper@diablo4-companion
```

Install from git, not from a directory copy or zip - `scripts/diablotrade/`
generates a `.venv/` in-tree that is gitignored, and a raw copy would carry a
thousand-odd files of someone else's virtualenv with it.

## Licence

Inherits the licence of the containing repository.
