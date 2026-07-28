# Tuning Prisms and Affix Pools - Diablo 4 Season 14 supplement

Snapshot date: **2026-07-28**. Describes **Season 14 "Death Awakening", patch 3.1.x, Lord of
Hatred**. This is a research supplement to `horadric-cube.md`, not a replacement for it - read
that file first for the full recipe table. **Re-verify all of this against live sources before
quoting it in a new season or patch**, especially the affix-category tables, which the
community has had to reverse-engineer from in-game Loot Filters rather than from an official
Blizzard document.

All web lookups for this file were run through the project's `d4_search.py` helper (Tavily
search + extract, scoped to `maxroll.gg`, `mobalytics.gg`, `icy-veins.com`,
`diablo4.wiki.fextralife.com`, `game8.co`, `news.blizzard.com`, plus a few incidental domains
the tool surfaced: `reddit.com`, `sportskeeda.com`, `us.forums.blizzard.com`).

## The central question: which side does the Chaotic Reroll prism name?

**Verdict: (b) - the Prism sets the Category of the affix you RECEIVE (the replacement). The
existing `horadric-cube.md` snapshot's interpretation is the one the strongest single source
supports, but the evidence is not unanimous. Confidence: [Likely], not [Certain].**

The clearest single statement found is from Icy-Veins' Season 14 Horadric Cube guide, which
gives a worked example rather than just restating the ambiguous recipe blurb:

> "Chaotic Reroll | ... | Randomly rerolls an affix to a different category, use a Tuning Prism
> to determine **which affix type the outcome will focus**. Example: **You can use an offensive
> tuning prism to turn all other affixes into offensive**"
>
> - <https://www.icy-veins.com/d4/guides/horadric-cube-overview>

"The outcome" and "turn all other affixes into offensive" both point at the prism controlling
the category of the **result**, with the victim drawn from whatever the item currently has that
is *not* already in that category. `[Likely]`

Maxroll's own phrasing is the ambiguous one the task flagged, and on a careful re-read it is
genuinely ambiguous, not just re-statable in only one direction:

> "Changes a random affix to one of another Category. Use a Tuning Prism to determine the
> Category of the random chosen Affix."
>
> - <https://maxroll.gg/d4/resources/horadric-cube>

"The random chosen Affix" can be parsed either as the affix that was randomly chosen *to be
removed* (victim reading, **a**) or the affix that gets *chosen as the replacement* (replacement
reading, **b**). Both parses are grammatically defensible. `[Unverified - ambiguous by design of
the source, not a translation error]`

Working against the (b) verdict: a Reddit PSA thread with an apparent real reproduction reads
the opposite way:

> "So how would I go about replacing lightning resistance with critical strike damage? It seems
> every protectors prism I use, only replaces max life for something else but leaves lightning
> resistance alone every time"
>
> - <https://www.reddit.com/r/diablo4/comments/1t1ha1g/psa_what_affixes_are_covered_by_which_tuning>

Read plainly, this player used a **Protector's** (Defensive) prism hoping to convert **Lightning
Resistance** away, and instead the recipe kept consuming **Maximum Life** (also a
Protector's-category affix) and replacing it with something unrelated. That is easiest to
explain if the Protector's prism was selecting the **victim** from the item's existing
Defensive-category affixes (interpretation **a**) rather than setting the destination category

- under interpretation (b), converting a Defensive stat (Max Life) into another Defensive stat
via a same-side Defensive prism would arguably violate the recipe's own "changes to **another**
category" rule.

**Net call:** the icy-veins worked example is the most explicit statement found and is the basis
for the (b) verdict above, but the Reddit report is real-world testing evidence pointing at (a).
Do not treat this file's (b) verdict as settled; treat it as "best available reading, actively
disputed by at least one field report." `[Likely]` overall, downgraded from what would otherwise
be `[Certain]`.

### Focused Reroll: the question is largely moot

Icy-Veins: "Focused Reroll | ... | Rerolls a specified affix category on a piece of gear to a
**different stat**. Example: **Turning Crit Chance on gear to Attack Speed**." Crit Chance and
Attack Speed are both Aggressive-category stats per the affix chart below, so this recipe swaps
one stat for a *different* stat **inside the same single category** - source and destination
category are identical. There is no victim-vs-replacement ambiguity to resolve here: the prism
simply names the one category both the old and new affix must belong to. `[Likely]`, matches
Maxroll's "Changes an affix to one of the same Category" phrasing exactly.

## Full affix-category chart (verified against Game8's dedicated chart)

Game8 publishes the most complete chart found, and it corroborates the existing
`horadric-cube.md` table with one correction (see Drift below):

| Prism | Affixes it covers (per Game8, verbatim categories) |
|---|---|
| Aggressive | Core Stat, Weapon Damage, Thorns, Any Element Damage Multiplier, All Damage Multiplier, Damage Over Time Multiplier, Critical Strike Damage Multiplier, Critical Strike Chance, **Attack Speed**, **Vulnerable Damage Multiplier** |
| Pragmatic | Cooldown Reduction, Lucky Hit Chance, Healing Received, Thorns, Life Regen, Movement Speed, Maximum Evade Charges, Evade Grants Movement Speed, Attacks Reduce Evade Cooldown |
| Protector's | Maximum Life, Armor, **All Elemental Resistance**, One Elemental Resist, Dodge Chance, (Fortify/Life on Hit/Life on Kill per prior snapshot, not re-confirmed this pass) |
| Resourceful | Resource Regen, Resource on Kill, Resource (list truncated by the extraction tool; not fully re-verified this pass) |
| Chromatic | **Any Elemental Res (All Resist NOT included)** |
| Adept's | Core Stat, Skill Ranks |

Source: <https://game8.co/games/Diablo-4/archives/599262> ("Tuning Prisms Guide: Affix Chart and
How to Use", Game8, dated with 2026 season-14 nav content present). `[Likely]`

### Drift from the existing `horadric-cube.md` snapshot

The prior snapshot's table says `Chromatic | All Resistance, Specific Resistances`. Two newer
sources both correct this:

- Game8's chart explicitly annotates Chromatic as "Any Elemental Res **(All Resist not
  included)**".
- A Reddit PSA (quoting a related Sportskeeda writeup) states: "Some players have made the
  mistake in thinking that the Resource Prisms give Resistance to All, but **that comes from the
  Protector's Tuning Prism**" - <https://www.sportskeeda.com/mmo/all-diablo-4-affix-categories-tuning-prism-crafts>
  (published with May 2026 imagery).

**Correction: All Resistance is a Protector's-prism affix, not a Chromatic one. Chromatic only
reaches individual/specific elemental resistances.** `[Likely]` - flag this to whoever
maintains `horadric-cube.md`, since that file's own table should be corrected on its next pass.

Aggressive, Pragmatic, and Adept's lists check out against the prior snapshot with no material
drift detected this pass. Resourceful and the Protector's non-resistance entries (Fortify
Generation, Life on Hit, Life on Kill, Life Regeneration) were not independently re-confirmed
this session - the extraction tool truncated those rows before a fresh read could capture them.
`[Unverified]` for those specific sub-entries; treat the prior snapshot as still authoritative
for them pending a future check.

## Transfiguration prisms (Entropic, Kullean)

Confirmed consistent with the existing snapshot. Game8's chart adds the Transfigure-specific
option pool explicitly: "Core Stat, Item Quality, All Stats, Resistance to All Elements, Maximum
Life, Total Armor, Movement Speed" plus "Any Legendary Aspect for your Class" when using
Entropic/Kullean with Transfigure Item. `[Likely]`

## Where prisms are farmed in Season 14

Confirmed: **War Plans Cache Rewards from Tree of Whispers Elite Monsters** is the source listed
for Chromatic, Adept, Entropic, and Kullean prisms alike, per Icy-Veins' Horadric Cube page
(<https://www.icy-veins.com/d4/guides/horadric-cube-overview>, images dated 2026-02/2026-04). The
page's table repeats the identical location string for each prism row, so it reads as one
farming method covering the whole prism family, not per-prism-unique routes. `[Likely]`

**Could not confirm:** the task's premise that Undercity Bargains are a Tuning Prism source.
No source found in this session's searches names Kurast Undercity or Undercity Bargains as a
prism drop location; Kurast Undercity came up only as an S-tier farming activity for Mythic
Tribute of Armaments and general endgame loot, unrelated to prisms specifically. This is a real
gap, not a confirmation either way - see Gaps.

## Excluded-affix rules for Cube rerolls

**Enchanted affixes are locked out of Chaotic Reroll / Focused Reroll / Remove Affix**, per
Maxroll's Item Crafting guide, describing the trick directly:

> "Another trick you can do is to Enchant the stat you want to keep, but not change the stat.
> This 'locks' the stat from being changed when you use the **Remove Affix / Focused Reroll /
> Chaotic Reroll** options at the Horadric Cube **without using a Tuning [Prism]**."
>
> - <https://maxroll.gg/d4/resources/item-crafting>

`[Likely]` - note the "without using a Tuning Prism" qualifier: this reads as the lock holding
against the *random* component of these recipes, not necessarily against a prism that
specifically targets the enchanted affix's own category. That nuance (does a matching prism
override the enchant-lock?) was not separately confirmed - see Gaps.

**Tempered affixes**: confirmed excluded specifically from Transfiguration's "Replace affix"
outcome (already documented in `horadric-cube.md`: "Cannot hit Enchanted affixes, Tempers, or
Mythic Uniques"). Whether Tempered affixes are *also* excluded as Chaotic/Focused Reroll victims
(as opposed to just the Transfiguration outcome) was **not found** in any source this session.
`[Unverified]` - treat as a gap, do not assume the Transfiguration-only exclusion generalizes.

## Duplicate-affix exclusion

Not independently re-confirmed this session with a fresh source, but this is standard,
uncontested D4 itemization behavior referenced in passing by the existing snapshot ("Normal
affix rules still apply: you cannot add a second Maximum Life affix to an item that already has
it"). No source contradicted this. `[Likely]`, carried over from the existing snapshot rather
than freshly sourced.

## Slot gating: can a ring roll Attack Speed? Vulnerable Damage? Can body armor roll Crit Strike Damage?

**Ring + Vulnerable Damage: yes, confirmed directly.** Maxroll's Item Crafting guide uses a Ring
as its worked example and explicitly names Vulnerable Damage as a legal ring affix:

> "If we wanted to add offensive affixes onto this ring, such as **Vulnerable Damage Multiplier**
> or Critical Strike Damage Multiplier, we would use the following recipes..."
>
> - <https://maxroll.gg/d4/resources/item-crafting>

`[Likely]` (single clear primary-guide statement, not cross-confirmed by a second independent
source this session, hence not `[Certain]`).

**Ring + Attack Speed: probably yes, but not pinned down with an equally explicit quote.**
Evidence chain: Attack Speed is confirmed as an Aggressive/Offensive-category affix (Game8's
chart, above). Maxroll's slot-pool overview states "Offensive stats mostly come from your
Weapons, Gloves, and Jewelry" (Jewelry = Ring + Amulet) -
<https://maxroll.gg/d4/getting-started/stats-for-beginners>. Icy-Veins' Basic Cleave build guide
lists Attack Speed as a live Gloves affix priority ("Gloves | 1. Critical Strike Chance 2.
**Attack Speed** 3. Strength 4. Critical Strike Damage Multiplier"), confirming Attack Speed sits
in the same offensive pool Rings also draw from. No source found says "Ring cannot roll Attack
Speed", and none of the Game8 per-slot affix-list pages fetched this session contradicted it, but
none stated it as a bare line either (short generic rows like "+X% Attack Speed | All" were
filtered out by the extraction tool's line-length heuristic before reaching this file's author -
see Gaps). `[Likely]`, one step down from the Vulnerable Damage verdict.

**Body armor (chest/pants/boots/helm) + Critical Strike Damage: probably NOT legal.** Maxroll's
slot-pool overview enumerates the Armor-piece pool explicitly and Critical Strike Damage is not
in it:

> "Armor Pieces (Helm, Chest, Pants, Boots) generally grant you defensive and utility stats.
> These affixes can include +x Max Life, +x to Primary Stats, +x to Skill Ranks, Resistances,
> and Movement Speed... Weapon Pieces... give direct access to damage multipliers, such as
> Critical Strike Damage and Damage Over Time."
>
> - <https://maxroll.gg/d4/getting-started/stats-for-beginners>

Game8's dedicated Two-Handed Sword affix list independently confirms Critical Strike Damage as
an **Inherent** (guaranteed) affix on weapons specifically ("+ X% Critical Strike Damage
(Inherent) | All" - <https://game8.co/games/Diablo-4/archives/410538>), reinforcing that Crit
Damage's home slot is Weapons (and, per the Ring finding above, Jewelry), not body Armor.
`[Likely]` that body armor cannot roll Critical Strike Damage, but this is an inference from "not
listed in the Armor pool" rather than an explicit "Chest cannot roll Critical Strike Damage"
statement - no source stated the negative directly.

## Does Chaotic/Focused Reroll accept a Horadric Seal (Talisman) as input?

**Not confirmed either way.** The Horadric Cube recipe table (per both the existing snapshot and
every source re-checked this session) specifies Chaotic Reroll / Focused Reroll / Remove Affix /
Add Affix inputs as "1x Magic, Rare, or Legendary Item" without naming Talismans. The **3 to 1
Transmutation** recipe explicitly does name "equipment, Talisman, or Rune" as eligible input,
which is the only recipe in every source checked that names Talismans by name. This asymmetry
suggests Horadric Seals are likely NOT eligible for the four Affix Modification recipes and only
participate in the Transmutation-family recipes, but no source explicitly states this exclusion

- it is an inference from an absence, not a confirmation. `[Unverified]` - flagged as a real gap,
see below.

## Sources

- Maxroll, "Horadric Cube" - <https://maxroll.gg/d4/resources/horadric-cube> - page banner
  "Updated for Season 14", reviewed by Icytroll/Avarilyn/Wudijo (same page the prior snapshot
  used, re-checked this session).
- Maxroll, "Item Crafting" - <https://maxroll.gg/d4/resources/item-crafting> - undated on the
  page itself; cross-links to the current Horadric Cube and Masterworking pages.
- Maxroll, "Stats for Beginners" - <https://maxroll.gg/d4/getting-started/stats-for-beginners> -
  undated on the page itself.
- Icy-Veins, "Horadric Cube" - <https://www.icy-veins.com/d4/guides/horadric-cube-overview> -
  page images dated 2026-02 and 2026-04 upload paths, current Season 14 nav present.
- Icy-Veins, "Basic Cleave - Barbarian Build" -
  <https://www.icy-veins.com/d4/guides/basic-cleave-barbarian-build> - marked "Season 14", 2025-04
  artwork upload path.
- Game8, "Tuning Prisms Guide: Affix Chart and How to Use" -
  <https://game8.co/games/Diablo-4/archives/599262> - Season 14 nav banner present at fetch time.
- Game8, "List of Two-Handed Sword Affixes" -
  <https://game8.co/games/Diablo-4/archives/410538> - Season 14 nav banner present.
- Game8, "List of Ring Affixes" - <https://game8.co/games/Diablo-4/archives/415830> - Season 14
  nav banner present.
- Reddit, r/diablo4, "PSA: What affixes are covered by which tuning prism" -
  <https://www.reddit.com/r/diablo4/comments/1t1ha1g/psa_what_affixes_are_covered_by_which_tuning>
  - undated (Reddit does not expose post date via this extraction path).
- Sportskeeda, "All Diablo 4 Affix categories for Tuning Prism crafts" -
  <https://www.sportskeeda.com/mmo/all-diablo-4-affix-categories-tuning-prism-crafts> - image
  upload paths dated 2026-05.
- Blizzard Forums (community post, not a dev post), "Cube Basics 101 - Your cube and You" -
  <https://us.forums.blizzard.com/en/d4/t/cube-basics-101-your-cube-and-you/250459> - undated at
  fetch time; player-written guide, not an official Blizzard statement, cited here only for the
  Remove-Affix-rarity-limit confirmation and as the source that surfaced the Reddit PSA thread.
- Diablo 4 Wiki (Fextralife), "Horadric Cube" - <https://diablo4.wiki.fextralife.com/Horadric+Cube>
  - page title banner reads "All Recipes for Season 13" at fetch time, i.e. this page had **not**
  yet been relabeled for Season 14 when checked; treat its specifics as lagging by one season and
  prefer the Maxroll/Icy-Veins pages above where they conflict.

## Gaps

Everything below could not be verified this session and should not be treated as settled:

1. **The Chaotic Reroll victim-vs-replacement question is not unanimously resolved.** This file
   gives (b) as its best-available verdict, but a real field report (Reddit PSA thread) reads as
   (a). Do not present the (b) verdict as beyond dispute.
2. **Resourceful and part of the Protector's affix lists** were not freshly re-verified this
   session (Fortify Generation, Life on Hit, Life on Kill, Life Regeneration for Protector's;
   the full Resourceful list beyond "Resource Regen, Resource on Kill"). The prior snapshot's
   values are carried forward unverified for these specific entries.
3. **Undercity Bargains as a Tuning Prism source** - the task's premise could not be confirmed.
   No source found named Kurast Undercity/Undercity Bargains as a prism drop location.
4. **Whether a matching Tuning Prism can override an Enchant-lock** on Chaotic/Focused Reroll -
   Maxroll's phrasing ("without using a Tuning Prism") implies this might be possible but does
   not spell it out.
5. **Whether Tempered affixes are excluded as Chaotic/Focused Reroll victims** (as opposed to
   just excluded from the Transfiguration "Replace affix" outcome, which is confirmed) - no
   source addressed this directly.
6. **Ring + Attack Speed** and **body armor cannot roll Critical Strike Damage** are both
   inferences from category-level slot-pool descriptions, not from an explicit per-affix,
   per-slot enumeration. A definitive itemization datamine/table (if one exists for Season 14)
   would settle both cleanly; none was surfaced by this session's searches.
7. **Horadric Seal (Talisman) eligibility for Chaotic Reroll / Focused Reroll / Add Affix /
   Remove Affix** - inferred "probably not" from the recipe cost table never naming Talismans
   for these four recipes, but no source states the exclusion explicitly.
8. **Full generic (non-legacy, non-class-conditional) per-slot affix tables for Ring, Amulet,
   and Chest Armor** could not be cleanly extracted this session - the `d4_search.py` tool's
   line-filtering heuristic (keeps only lines over 40 characters containing a D4 keyword)
   systematically drops short generic rows like "+X% Attack Speed | All", so the Game8 per-slot
   list pages kept surfacing only their longer class-conditional/Legacy rows instead of the
   core generic list. A future pass should fetch those pages by a method that preserves short
   table rows (e.g. `--include-raw-content` without the keyword filter) rather than reformulating
   the search query further.
