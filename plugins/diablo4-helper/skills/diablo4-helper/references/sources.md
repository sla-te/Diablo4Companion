# Diablo 4 source hierarchy and query templates

Ordered by authority for the job. Cross-check single-source claims against a
second source, and always note the publication/patch date of a guide - D4
guides go stale within a season.

## Sources

### Official Blizzard (highest authority for "is this current")
- **Patch notes / news**: `news.blizzard.com/en-us/diablo4` and the in-client
  patch notes. The only authoritative source for what a mechanic *currently*
  does, drop-rate changes, and hotfixes. When a third-party guide and a patch
  note disagree, the patch note wins.
- **Official forums** (`us.forums.blizzard.com/en/d4`): dev clarifications, bug
  acknowledgements.

### Build and gearing (highest authority for "how should I play")
- **Maxroll.gg** - the reference for builds, the build planner, stat priorities,
  tier lists, and season guides. The user's D4Companion imports Maxroll builds,
  so Maxroll is the canonical build source for this user. Prefer it for gear
  priority and "is X an upgrade."
- **Mobalytics** (`mobalytics.gg/diablo-4`) - second opinion on builds/tier
  lists.
- **Icy Veins** (`icy-veins.com/d4`) - build guides, mechanics writeups.

### Item / mechanic data
- **Maxroll** (item and aspect databases, planners).
- **D4 wiki - Fextralife** (`diablo4.wiki.fextralife.com`) - unique/aspect/charm
  pages, effect text, how-to-obtain. Good for item effects; verify numbers
  against current patch.
- **game8** (`game8.co/games/Diablo-4`) - how-to guides, recipes, farming.
  Often updated quickly for a new season.

### Community / edge cases
- **r/diablo4** (`reddit.com/r/diablo4`) - "does this still work," bug reports,
  crafting gotchas (e.g. "can find but can't craft" material requirements). Treat
  as leads to verify, not authority.

## Query templates

Season-scoped questions - resolve the season first:

```
"diablo 4 current season <YEAR> name theme"
"diablo 4 season <N> patch notes mechanics"
```

Build / gear:

```
"maxroll diablo 4 <class> <skill> build season <N> gear stat priority"
"diablo 4 <build> temper masterwork priority season <N>"
```

Item / unique / mythic:

```
"diablo 4 <item name> unique effect how to get season <N>"
"diablo 4 how to craft <mythic> horadric cube current season"
"diablo 4 <item> item power affixes"
```

Crafting / cube recipes (watch for material and Ancestral requirements):

```
"diablo 4 season <N> horadric cube recipes convert unique into charm"
"diablo 4 <mechanic> crafting materials cost season <N>"
```

Farming / loot filter:

```
"diablo 4 season <N> best <material/unique> farm route"
"diablo 4 season <N> loot filter import recommended"
```

## Notes
- The bundled `scripts/d4_search.py` applies these domains as
  `--include-domains` automatically; use it before hand-writing tavily calls.
- Blizzard news and Maxroll are JS-heavy - if a snippet search is thin, extract
  the specific page URL rather than relying on the search snippet.
