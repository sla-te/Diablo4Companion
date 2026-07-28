# Diablo 4 crafting decision tree

Snapshot date **2026-07-28**. Describes **Season 14 "Death Awakening"**, patch line
**3.1.x** (patch notes for 3.1.1 dated 2026-07-14 were the newest confirmed during
research), Lord of Hatred expansion. Systems get reworked most seasons (Masterworking
alone changed its rank cap and reroll behavior across Seasons 4, 11, and later). **Do
not carry these numbers into a new season without re-verifying.** Costs and prism
mechanics for the Horadric Cube itself are covered in `horadric-cube.md`; this file is
the decision layer on top - which system to reach for, in what order, and what wastes
materials or bricks an item.

## Why this file exists

The Horadric Cube is one of at least eight systems that can change an item: Enchanting,
Tempering, Masterworking, Aspect Imprinting, Socketing/Jeweler, Runewords, Blacksmith
upgrades, and the Cube's own recipes. They interact, they gate each other, and two of
them (Transfiguration, Enchanting) make a permanent, hard-to-reverse commitment the
first time you use them. Grading an item, or planning how to finish one, requires
knowing which system is reversible, which one is a one-shot, and the order that avoids
paying twice.

## The master order of operations

This is the sequence Maxroll's own crafting-overview page recommends, condensed and
annotated. Source: <https://maxroll.gg/d4/resources/item-crafting> (Certain, quoted
directly below).

1. **Build the item base while it is Magic or Rare.** Do all Horadric Cube
   Add Affix / Remove Affix / Focused Reroll / Chaotic Reroll work now. `Remove Affix`
   only works on Magic or Rare items - once the item becomes Legendary you can only
   **add**, never remove, an affix via the Cube. [Certain] Quote: "You can only remove
   affixes while the item is Magic or Rare rarity, meaning you can only change the
   affixes on Legendary items" (by adding, not removing).
2. **Imprint the Aspect only once affix modification is finished.** Imprinting promotes
   the item to Legendary rarity, which closes off Cube-based affix removal from step 1.
   [Certain] Quote: "Imprinting an item with an aspect turns it into Legendary rarity,
   so wait with imprinting until you are done modifying the aspects!"
3. **Add sockets (Jeweler) whenever convenient, but always before Transfiguration.**
   Sockets do not depend on rarity, but skipping this step is the single most common
   finishing mistake - see the Transfiguration warning below.
4. **Masterwork before Tempering if you are chasing the Masterwork Capstone bonus on a
   specific affix.** [Certain] Quote: "If your item has a specific affix that needs a
   Masterwork Crit, Masterwork your item before tempering to reduce potential
   outcomes." Doing it in this order keeps the Capstone's random-affix roll away from
   competing with a Tempered affix that does not exist yet. If you do not care which
   affix gets the Capstone bonus, order between Masterworking and Tempering does not
   otherwise matter - they are accessed at the same NPC (the Blacksmith) and neither
   gates the other. [Likely, icy-veins.com/d4/guides/masterworking-guide states
   Tempering is "NOT required" before Masterworking]
5. **Temper.** Adds one build-defining affix (the current patch caps an item at exactly
   one Tempered affix - see Tempering section below).
6. **Enchant only the affix you are certain about**, because the first Enchant you
   commit permanently binds that item's one enchant slot to that specific affix. See
   "The enchant lock" below before doing this step.
7. **Transfigure last, if at all.** [Certain] Quote: "Transfiguring is the final step
   of your item's crafting journey, and it makes the item unmodifiable... you cannot
   add a Socket, Temper, Enchant, Aspect, or Masterwork the item after you have
   transfigured it." The site's own checklist: "Make sure to Socket, Temper, Enchant,
   Imprint Aspect, and Masterwork an item before you Transfigure it."

Caveat found in one source (Game8, masterworking guide) claims Tempering must precede
Masterworking as a hard prerequisite ("The first step in masterworking your equipment
is to temper it"). This conflicts with icy-veins's explicit "not required" statement
and appears to carry over language from an older patch that also referenced a 20-rank
Masterworking cap (since raised to 25) and two Tempered affixes (since reduced to one).
**Treat the Game8 ordering claim as stale and follow the Maxroll/icy-veins position: no
hard prerequisite, but do Masterworking first if a specific affix must get the
Capstone.** [Unverified as a hard rule either way - flagged as a Gap]

## What each system actually does

| System | NPC | What it changes | Reversible? |
|---|---|---|---|
| Horadric Cube recipes | Temis (Cube) | Add/remove/reroll random affixes; transmute; Transfigure; upgrade rarity | Add/Remove: yes, while Magic/Rare. Transfigure: no |
| Enchanting | Occultist | Reroll one existing affix to a new one, from options | No - binds to that affix slot permanently after first commit |
| Tempering | Blacksmith | Adds exactly one extra affix from a learned Manual's category list | Reroll: yes, using more charges. Manual choice: yes, can be changed anytime before charges run out |
| Masterworking | Blacksmith | Raises Quality 0 to 25 (scales all affix and base values +1%/rank); rank 25 grants a +50% Capstone bonus to one random affix | Rank: resettable (pay to lower, no refund of prior materials). Capstone target: rerollable at a cost |
| Aspect Imprinting | Occultist | Replaces the item's Legendary Aspect/Unique-adjacent power slot | No - old aspect is gone once overwritten |
| Socketing / Jeweler | Jeweler | Adds sockets; crafts/upgrades gems and Runes; free gem removal | Sockets: permanent once added (cannot remove a socket, only what is inside it). Gem removal: free, non-destructive |
| Runewords | Jeweler (socketing) | Combine a Ritual Rune + Invocation Rune in a 2-socket item for a passive effect | Unverified whether individual runes survive removal - see Gaps |
| Blacksmith repair/upgrade | Blacksmith | Repairs durability; legacy "item power upgrade" has been superseded by Masterworking | N/A |
| Salvaging | Blacksmith | Destroys an item for base materials | No - item is gone |

## "I want a specific affix on this item. What do I use?"

- **The affix does not exist on the item at all, and the item is still Magic or
  Rare:** use the Cube's `Add Affix` (with a category Prism if you want to narrow
  which pool it draws from). [Certain, horadric-cube.md]
- **The affix exists but is the wrong one for the slot, and rarity allows removal
  (Magic/Rare only):** use `Remove Affix` then `Add Affix`, or `Chaotic Reroll` /
  `Focused Reroll` to swap it directly - see horadric-cube.md for the unresolved
  question of which side the Prism names.
- **The item is already Legendary and you want to swap ONE existing affix:** this is
  what Enchanting is for. It shows two new options for a single chosen affix (or lets
  you keep the roll, at the same cost). [Certain] But understand the lock first (next
  section) - this is a one-time-per-item-forever choice of WHICH affix stays
  reroll-able.
- **You want a wholly new 5th affix that is not in the item's normal affix pool for
  that slot:** Tempering is the only route. It requires owning the Manual for that
  affix's category, and it does not touch the item's existing 4 affixes at all -
  it purely adds. [Certain]
- **None of the above can produce the combination you want on this base:** consider
  whether a different base item (via 3-to-1 Transmutation, farming, or Upgrade to
  Legendary) is cheaper than chasing an affix pool that cannot roll what you want.
  Slot-gated affixes exist (Movement Speed only on Boots/Amulets is one documented
  example from the Transfiguration pool) - verify the target affix can even appear on
  that slot before spending materials.

## "I want to REMOVE an affix." Which systems can delete vs only swap?

- **Horadric Cube `Remove Affix`:** the only recipe that deletes an affix outright
  without replacing it in the same action, and only works on Magic or Rare rarity.
  [Certain, horadric-cube.md]
- **Chaotic Reroll / Focused Reroll:** delete-and-replace in one action (not a pure
  removal), also Magic/Rare only.
- **Enchanting:** presents new options for one existing affix; you are not removing an
  affix from the item, you are replacing its identity while the slot count stays the
  same. Not usable to reduce total affix count.
- **Tempering:** cannot remove anything. It only adds.
- **Masterworking:** cannot remove anything, only scales existing values and can boost
  one affix via the Capstone.
- **Once an item is Legendary+ rarity, no system in the game can delete an affix
  outright.** [Certain] Quote: "you can only change the affixes on Legendary items"
  by adding, meaning an unwanted affix on a Legendary is permanent unless you
  Transfigure and get lucky with the "Replace existing affix" outcome (~10% chance,
  cannot target Enchanted affixes, Tempers, or Mythic Uniques - see horadric-cube.md).

## "I want to raise an affix's VALUE, not change it."

- **Masterworking** is the primary tool: every rank adds +1% to all affix values and
  base stats (up to +25% at rank 25), and the rank-25 Capstone gives one random affix
  (which can be a Regular, Tempered, or Greater affix) an additional flat +50%.
  [Certain, maxroll.gg/d4/resources/masterworking-guide]
- **Greater Affix status** is not something you can force onto an arbitrary existing
  affix - it is rolled at drop time, or has a chance to appear when you Temper an
  Ancestral item, or (per the existing Cube reference) via Transfiguration's "Upgrade
  to Greater Affix" outcome (~15% without a Prism). There is no direct "make this
  affix Greater" recipe outside those three chances.
- **Reroll Set Charm** is the intended route to farm Greater Affixes specifically on
  Charms (~4% chance per affix per reroll). [Certain, horadric-cube.md]
- **Unique Power Reroll** (Cube) only randomizes the VALUE of an Ancestral Unique's
  power within its range - it does not touch normal affixes and cannot change which
  Unique Power you have. [Certain, horadric-cube.md]
- Do not confuse "raise the value" with "change the affix." Enchanting and Cube reroll
  recipes change WHICH affix occupies a slot; only Masterworking (and luck on Temper /
  Transfigure) raises the VALUE of an affix that is already correct.

## "I want a specific Unique or Mythic. What route, what does it cost?"

Two structurally different Mythic-crafting routes exist and they are easy to confuse:

- **Cube "Upgrade to Mythic" (random, slot-only):** feed any Unique with 850+ Item
  Power of the target slot, plus Pandemonium Fragments (patch 3.1.1 lowered this to
  4, down from 5), and get a **random** Mythic for that slot. The input item's
  identity, affixes, and quality are completely discarded - feed your worst 850+
  Unique for that slot, never a good one. [Certain, horadric-cube.md +
  mobalytics.gg patch-notes-3-1-1]
- **Rune-based deterministic Mythic crafting (specific result):** craft a **named**
  Mythic Unique using Resplendent Spark plus a specific combination of named Ritual
  and Invocation Runes tied to that exact Mythic. Sources disagree on the exact rune
  count: Maxroll states 3x Legendary + 3x Rare + 3x Magic Runes of the specific names
  plus 3x Resplendent Spark; icy-veins states "10 runes of three different types...
  along with a Resplendent Spark." [Likely - the mechanism (Spark + named-rune set
  determines the SPECIFIC Mythic) is corroborated by both sources; the exact
  quantities are a Gap, flagged below]. This is the route to use when you want ONE
  named Mythic and are willing to farm the specific Runes for it, rather than gambling
  on the Cube's random slot recipe.
- **Craft Unique Charm** (Cube): converts a dead Ancestral Unique **equipment** item
  into a Unique **Charm**, for a specific eligibility list of shared and class Unique
  items. Use this when the equipment form of a Unique is obsolete but its Charm form
  is wanted. [Certain, horadric-cube.md]
- **Equip-slot limit: REMOVED in patch 3.1.1a. There is no longer any limit.**
  [Certain] The season-launch article said "You can only equip one crafted Mythic
  Unique. You can equip as many Mythic Uniques you want that were acquired from drops
  or caches, but you can only have one crafted Mythic Unique equipped at a time."
  **That text is stale and many guides still repeat it.** A Blizzard blue post for
  patch 3.1.1a announces "removing equip limits for Mythic Uniques" - see
  `itemization.md` for the verbatim quote. Confirmed in the field 2026-07-28: three
  `Crafted`-tagged Mythics equipped simultaneously with no penalty. Craft freely.
- **All Mythic Uniques are Ancestral, roll Unique Power +30%, and all affixes roll at
  maximum.** [Certain] This means a crafted Mythic never needs Masterworking's value
  boost as urgently as a normal item, though the Capstone bonus still applies.

## "This item is nearly perfect. What is safe, and in what order?"

Follow the master order above. The two irreversible steps (Enchanting and
Transfiguration) should be the last two things you do, in that order, and only after
you are certain about every other affix and the aspect.

Before Enchanting, ask: "if I could only ever reroll ONE affix on this item again for
the rest of its life, which one would I pick?" That is the affix to Enchant. Do not
Enchant an affix you are only mildly unhappy with while a worse one sits untouched -
the enchant slot is not reusable on a different affix. [Certain, per the direct quote
in the Enchanting section below]

Before Transfiguring, confirm: sockets added (including BOTH sockets on 2H
weapons/Helm/Chest/Pants if you want a Runeword there later), aspect finalized,
Tempered affix finalized, Masterwork rank/Capstone finalized, Enchant already
committed. Transfiguration removes the ability to do any of these afterward except
swapping gems/Runes already in existing sockets. [Certain, horadric-cube.md +
maxroll.gg/d4/resources/item-crafting]

## The enchant lock: what it protects, and what is unresolved

**What Enchanting does, confirmed:** [Certain, quoted directly from
maxroll.gg/d4/resources/item-crafting] "Enchanting allows you to re-roll one
undesirable affix on your gear for a chance at good stats. Once a stat has been
enchanted, the item becomes account-bound if it wasn't already, and you can only
attempt to re-roll that specific stat in the future." You are shown two new options
per attempt (or may keep the current roll, at the same Gold cost) and can keep
rerolling that ONE stat as many times as you can afford; Gold cost rises with no cap
each time you reroll.

**What Enchant-locking protects against:** [Certain, same source] the enchanted affix
is safe from being touched by the Horadric Cube's `Remove Affix` / `Focused Reroll` /
`Chaotic Reroll` recipes when those recipes are run **without a Tuning Prism**. This is
the trick the existing Cube reference documents: Enchant a Greater Affix you want to
keep, then use the Cube to reroll a different, unwanted stat in the same Prism category
without risking the good one.

**What is genuinely unresolved (do not assume):**

- Whether a Cube reroll run **with a Tuning Prism** that targets the enchanted affix's
  own category can still override the enchant-lock. horadric-cube.md already flags an
  unrelated but structurally similar dispute (which side of a Chaotic Reroll a Prism
  names) as unresolved in-game; the enchant-lock's Prism interaction was not
  independently confirmed by this research and should be treated the same way - test
  on a junk item before trusting it on a geared one. [Unverified]
- Whether re-enchanting a slot that already carries the Masterwork Capstone's +50%
  bonus keeps that bonus on the newly chosen affix, or requires a fresh Capstone
  reroll to reattach it. Confirmed for Tempering (rerolling the Tempered affix after
  Masterworking keeps the item's Masterwork rank applied to the new roll, per
  icy-veins), but no source confirmed the equivalent for Enchanting. [Unverified,
  Gap]
- Enchant-lock does **not** protect against Masterworking or Tempering, because
  neither of those systems deletes or swaps existing affixes - Masterworking only
  scales values and Tempering only adds a new slot, so there is nothing for the lock
  to guard against there.

## Cost efficiency and the bottleneck resource

- **Season 14's specifically-called-out bottleneck was Pandemonium Fragments** (used
  by the Cube's Upgrade to Mythic recipe). Patch 3.1.1 (2026-07-14) reduced the cost
  from 5 to 4 Fragments and buffed drop sources specifically because Fragments were
  identified as a bottleneck: the Corrupted Reaper now drops up to two (scaling with
  Torment), and the repeatable Glints of Hope reputation reward now guarantees one.
  [Certain, mobalytics.gg/diablo-4/guides/patch-notes-3-1-1-season-14]
- **Resplendent Spark** gates the rune-based deterministic Mythic-crafting route
  (see above) and is a separate, likely much scarcer currency historically tied to
  Uber-boss drops. [Likely - not independently re-verified this session for current
  drop sources; treat its scarcity as the reason to prefer the Cube's random route
  unless you specifically want one named Mythic. Gap: current Spark drop sources for
  Season 14 were not confirmed.]
- **Obducite** is the Masterworking currency, sourced from Nightmare Dungeons
  (especially Treasure Breach goblin sigils shared in a 4-player rotation), Kurast
  Undercity with Tribute of Refinement, Infernal Hordes, and Mercenary bartering for a
  Masterworking Cache. [Certain, maxroll.gg/d4/resources/masterworking-guide] Cost
  scales up with rank: one source states a linear formula from 10 Obducite at Quality
  0 to 100 Obducite at Quality 24, plus 250 Obducite for the Capstone
  [Certain, maxroll.gg]; a second source describes cost "increasing exponentially" in
  broader bands without giving the same linear formula [Likely, game8.co - the two
  sources do not fully agree on the shape of the curve; treat the Maxroll linear
  figures as the more current/authoritative one, and the "exponential" language as a
  looser paraphrase of the same trend rather than a contradiction].
- **Gold** is an uncapped, ever-rising sink on repeated Enchant rerolls and on Aspect
  Imprinting (1,000,000 Gold for an Ancestral imprint, 250,000 for non-Ancestral) and
  on Masterwork Capstone rerolls (10,000,000 Gold at 900 Item Power on an Ancestral
  item). [Certain, maxroll.gg/d4/resources/item-crafting and
  masterworking-guide] Do not chase a "perfect" Enchant roll without a large Gold
  buffer; cost has no ceiling.
- **Masterwork rank reset** (the in-game "reset" control next to a Masterworked item)
  has a **100% success chance** but does **not refund** previously spent Obducite -
  it costs a flat additional bundle of basic materials and Gold (one source: 30
  Iron Chunks/Rawhide, 20 Veiled Crystals, 5 Forgotten Souls, 5,000,000 Gold) to lower
  the rank back down so you can redo an earlier step. [Likely, game8.co - could not
  cross-confirm the exact flat cost against a second source]. This makes
  over-investing Masterwork ranks before finishing Temper/Aspect a **soft trap**, not
  a hard brick: recoverable, but every Obducite already spent on the ranks you reset
  away is gone for good.

## Known community tricks

- **Add-affix-twice-then-remove for a 50/50.** Add a wanted affix (e.g. All
  Resistance) twice via the Cube, then use Remove Affix once - this gives roughly even
  odds of keeping the wanted roll instead of the other one, cheaper than fishing with
  single Add/Remove cycles. [Certain, horadric-cube.md]
- **Enchant-lock a Greater Affix before rerolling a sibling stat in the same Prism
  category** at the Cube, so the Cube's random target cannot touch the good roll while
  you fish for the second stat. [Certain, item-crafting.md quote above; see the
  unresolved Prism-override caveat before relying on this against Prism-targeted
  rerolls]
- **Feed the worst 850+ Unique you own into Cube "Upgrade to Mythic."** Input identity,
  affixes, and quality are entirely discarded, so a beautifully rolled Unique gains
  nothing by being fed in - use your junkiest one for that slot. [Certain,
  horadric-cube.md]
- **Use 3-to-1 Transmutation on items bricked while chasing Transfiguration outcomes**,
  so a bad ~10%-chance Transfiguration roll is not pure waste - it becomes fodder for
  a random new item of the same type. [Certain, horadric-cube.md]
- **Spread utility/defensive stats across multiple item pieces rather than stacking
  several onto one item.** Rolling one stat at a time per item is far more
  material-efficient than trying to force several affixes onto a single piece.
  [Certain, horadric-cube.md]
- **Masterwork before Tempering when chasing the Capstone on a known affix**, per the
  master-order section above - this keeps the Capstone's competing-affix pool smaller
  at the moment you commit to it.

## Traps that waste materials

1. **Masterworking or Tempering an item right before feeding it to a Cube recipe that
   discards everything** - Upgrade to Mythic, 3-to-1 Transmutation, and Recycle
   Uniques all consume the input item and ignore any Masterwork/Temper investment on
   it. Finalize whether an item is a KEEPER or FODDER before spending Obducite or
   Tempering materials on it.
2. **Forgetting sockets before Transfiguration.** [Certain, maxroll.gg/d4/resources
   /item-crafting] Once transfigured, the item is unmodifiable and a socket can never
   be added afterward - only what is already in a socket can be swapped. This is
   explicitly called out as an easy mistake, especially for 2H Weapons, Helm, Chest,
   and Pants, which need BOTH of their sockets filled to run a Runeword.
3. **Transfiguring before the item is actually finished.** Transfiguration blocks
   every other system afterward (Socket, Temper, Enchant, Aspect swap, Masterwork),
   so doing it early locks in whatever state the item was in, including an unwanted
   or placeholder aspect.
4. **Committing an Enchant to the wrong affix.** Because the enchant slot binds to
   one specific affix forever (see above), enchanting a merely-okay stat while a
   truly bad one sits untouched wastes the item's only reroll-able slot for its
   remaining lifetime.
5. **Chasing an affix a slot's pool cannot produce.** Affix pools are slot-gated -
   the Transfiguration pool alone documents Movement Speed as Boots/Amulet-only and
   Life on Hit as Offensive/Jewelry-only. Verify the target affix can appear on that
   slot at all (via a build guide or the game's own affix list) before spending
   Tempering Manuals or Cube materials chasing it.
6. **Extracting an Aspect from an item that could have used the free Codex version.**
   Codex of Power aspects are reusable indefinitely at a fixed, generally lower roll;
   an extracted Aspect from a Legendary item preserves its (possibly better) roll but
   can only be imprinted once and consumes the source item. If the Codex roll is good
   enough for the build, salvaging instead of extracting saves the extraction cost and
   keeps the source item's materials.
7. **Salvaging an item before extracting a needed Aspect.** Salvage and Aspect
   extraction are separate actions at the Occultist/Blacksmith; salvaging an
   un-extracted Legendary destroys the Aspect along with the item. [Certain,
   mobalytics.gg beginner guide via the Blacksmith/Occultist research pass]

## Sources

- <https://maxroll.gg/d4/resources/item-crafting> - master order-of-operations
  quotes, enchant-lock mechanic, socket-before-Transfigure warning, 2-socket slot
  list, Aspect imprint cost, Tempering cost table, Transfiguration outcome summary.
  Retrieved 2026-07-28.
- <https://maxroll.gg/d4/resources/masterworking-guide> - Quality 0-25 system,
  Capstone bonus mechanics, Obducite linear-cost formula, Capstone reroll cost,
  Obducite sources. Page changelog notes: "Updated with correct Masterworking costs,"
  "Updated for Season 11 - Rewrote the article for patch 2.5.0 Masterworking
  updates." Retrieved 2026-07-28.
- <https://www.icy-veins.com/d4/guides/masterworking-guide> - "Tempering is NOT
  required to Masterwork" statement; Masterworking rarity/eligibility. Season 14
  guide. Retrieved 2026-07-28.
- <https://www.icy-veins.com/d4/guides/runewords-guide> - Runeword mechanics, Ritual
  vs Invocation Rune roles, Offering resource, rune-based Mythic Unique crafting
  (10 runes across 3 types + Resplendent Spark figure). Page shows recent images
  dated as late as 2026-04. Retrieved 2026-07-28.
- <https://maxroll.gg/d4/resources/runewords-overview> - confirms 2-socket-only
  Runeword slots (Helm, Chest, Legs, 2H Weapon), gems-vs-runes mutual exclusivity per
  item, rune crafting at the Jeweler (3-of-a-kind), rune-based Mythic Unique crafting
  (3x Legendary + 3x Rare + 3x Magic Runes + 3x Resplendent Spark figure - conflicts
  with icy-veins's "10 runes" figure, see Gaps). Retrieved 2026-07-28.
- <https://diablo4.wiki.fextralife.com/Runewords> - Ritual/Invocation rune tables,
  Offering costs, crafting recipes for individual Legendary Runes. Retrieved
  2026-07-28.
- <https://maxroll.gg/d4/resources/jeweler-gems-socketing> - Jeweler functions
  (socket, craft/upgrade gems, free unsocketing), socketable slot list (Helm, Chest,
  Pants, Rings, Amulet, Weapons), gem tier chain, rune crafting at the Jeweler.
  Retrieved 2026-07-28.
- <https://game8.co/games/Diablo-4/archives/417621> - Masterwork rank-reset cost and
  100% success-chance claim; also contains apparently stale cross-patch language
  (references a 20-rank cap and "temper before masterwork" ordering not corroborated
  elsewhere) - used only for the reset-cost claim, flagged Likely rather than
  Certain. Retrieved 2026-07-28.
- <https://mobalytics.gg/diablo-4/guides/patch-notes-3-1-1-season-14> - Pandemonium
  Fragment cost reduction (5 to 4) and drop-source buffs, dated 2026-07-14.
  Retrieved 2026-07-28.
- <https://maxroll.gg/d4/resources/season-guide> - crafted-vs-dropped Mythic Unique
  equip-slot limit quote; Mythic Unique baseline stats (Ancestral, +30% Unique Power,
  max affix rolls). Season 14 - Death Awakening. Retrieved 2026-07-28.
- <https://mobalytics.gg/diablo-4/guides/absolute-beginner-guide> - salvage-before-
  extraction trap description, salvage material table basics. Retrieved 2026-07-28.
- `horadric-cube.md` (this skill's sibling reference) - all Horadric Cube recipe
  costs, Tuning Prism mechanics, Transfiguration outcome table and affix pool, Rune
  crafting recipes, Amalgamation table. Not re-derived here; treated as authoritative
  for anything Cube-specific.

## Gaps

- **Enchant-lock vs Tuning Prism override:** unconfirmed whether a Prism-targeted Cube
  reroll can still hit an enchant-locked affix. Test on a disposable item before
  trusting it on a geared one, same caution as the existing unresolved Prism-side
  dispute in horadric-cube.md.
- **Enchant reroll vs Masterwork Capstone interaction:** unconfirmed whether
  re-enchanting a slot that already carries the Capstone's +50% keeps the bonus on
  the new stat or requires a fresh Capstone reroll. Confirmed for Tempering, not for
  Enchanting.
- **Exact rune quantities for deterministic Mythic Unique crafting:** Maxroll states
  3x Legendary + 3x Rare + 3x Magic Runes (named) + 3x Resplendent Spark; icy-veins
  states "10 runes of three different types" + 1 Resplendent Spark. Could not
  reconcile the two figures to a single confirmed number this session.
  In-game confirmation should be trusted over either write-up.
- **Current-season Resplendent Spark drop sources** were not independently
  re-verified for Season 14; treat any specific farming-route claim about it as
  unconfirmed until checked live.
- **Exact Masterworking Obducite cost curve:** Maxroll gives a linear 10-to-100
  formula across Quality 0-24 plus 250 for the Capstone; Game8 describes the same
  trend as "exponential" in bands without matching numbers. The linear Maxroll figures
  are treated as more current, but the discrepancy itself was not resolved.
  Corroborates but does not fully match one of Game8's numeric details (200 Obducite,
  x1 Neathiron for the Capstone reroll) against Maxroll's 200 Obducite
  (x400 for 2H)/x1 Neathiron (x2 for 2H)/10,000,000 Gold figure - Gold cost is only in
  the Maxroll figure.
- **Precise max socket count per remaining slot types** (Gloves, Boots, Rings,
  Amulet, 1H Weapons): confirmed that 2H Weapons, Helm, Chest, and Pants can carry
  two sockets; the Jeweler page lists Helm, Chest, Pants, Rings, Amulet, and Weapons
  as socketable at all, which by omission suggests Gloves and Boots may not be
  socketable in the current patch, but this was not directly confirmed with a
  positive statement either way.
- **Whether removing a Rune from a Runeword slot destroys the Rune or returns it
  intact** (parallel to gem removal, which is confirmed free and non-destructive) was
  not found in any source checked this session.
- **Whether a Masterwork rank-reset is a per-item flat cost or scales with the
  item's current rank/rarity** - only a single flat figure (Game8, uncorroborated)
  was found.
- Patch number for the framing in this file (3.1.x, Season 14 "Death Awakening") was
  corroborated by a subagent's research pass (Mobalytics patch-notes page dated
  2026-07-14) but was not independently re-checked by the primary research pass in
  this file; treat the season/patch label as Likely rather than independently
  re-verified twice.
