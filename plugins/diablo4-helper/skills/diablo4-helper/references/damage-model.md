# Diablo 4 damage and defence model

Snapshot date: 2026-07-28. Describes patch 3.1.2, Season 14 "Death Awakening"
(Lord of Hatred expansion). Damage/stat mechanics change with major patches -
re-verify this document at the start of the next season before relying on it.

## 1. Damage buckets - additive vs multiplicative (the core question)

**[Certain]** Diablo 4 groups damage bonuses into a small number of "buckets."
Bonuses inside the same bucket sum together (additive); the buckets then
multiply against each other. Source: Maxroll "In-Depth Damage Guide"
(`https://maxroll.gg/d4/resources/in-depth-damage-guide`) - the guide's own
stated general formula is a product of several summed groups: Weapon Damage
x Skill% x Main Stat x Additive Damage Multiplier x Critical Strike x
Vulnerable x each active Global Multiplier x Overpower.

**[Certain] The specific question asked: two same-type `[x]` multipliers on
the same character.** Since Season 13, Blizzard added a distinct affix
wording, "**x[amount]% [Affix] Multiplier**" (for example "x56% Critical
Strike Damage Multiplier"), that is explicitly different from a bare
"x[amount]% [Affix]" line. Per Maxroll:

> "The Multiplier bit is important as it sets these affixes apart from other
> descriptions that use e.g. 'x[amount]% Critical Strike Damage', which is a
> separate Global Multiplier, while the first example with Multiplier in the
> description sums up all similar affixes to separate Global Multiplier[s]."

Worked example from the same source: three "Physical Damage Multiplier"
affixes of x15%, x24%, x11% do **not** compound as
`1.15 * 1.24 * 1.11`. They sum first into one bucket, then that bucket is
the multiplier: `100% + 15% + 24% + 11% = 150% = x1.5`. The same worked
example is given for Critical Strike Damage Multiplier (x35/x20/x50 ->
x2.05) and Vulnerable Damage Multiplier (x20/x23 -> x1.43).

So: **a "x25% Physical Damage Multiplier" affix and a second "x24% Physical
Damage Multiplier" affix on the same character add together into one x1.49
bucket - they are not two independent multiplicative layers.** This is a
narrower rule than plain `[x]` Global Multipliers in general (see below),
and only applies to affixes carrying the literal word "Multiplier" in their
tooltip.

**[Certain] Plain `[x]` Global Multipliers (no "Multiplier" word) are
independent and DO compound multiplicatively with each other and with the
Summed-Multiplier buckets above.** Example from Maxroll: 65%[x] from one
Aspect and 150%[x] from a Mythic Unique combine as
`(1+0.65) * (1+1.50) = 4.125`, not `1+0.65+1.50`. These sources are
typically Legendary Aspects, Unique/Mythic powers, and some Paragon Legendary
nodes with an explicit condition ("while Fortified", "against Close
enemies", etc).

**[Likely] Practical rule of thumb**: if the tooltip literally says
"...Multiplier" (Critical Strike Damage Multiplier, Vulnerable Damage
Multiplier, All Damage Multiplier, Damage Over Time Multiplier, an
elemental "X Damage Multiplier" affix), stack it freely - copies add. If a
Legendary Aspect/Unique/Paragon node grants a conditional `[x]` bonus
**without** the word "Multiplier" in its own text, treat it as its own
separate multiplicative layer, and do not expect it to add with another such
source of a different name.

## 2. Notation: `[x]` vs `[+]` vs bare `+`

**[Certain]**

- `[+]` (or a bare percentage with no bracket) = additive. It is summed
  into one of the additive buckets (Additive Damage Multiplier, Additive
  Vulnerable Damage, Additive Critical Strike Damage, etc) before that
  bucket's sum is used as a single multiplier.
- `[x]` = multiplicative. Either it sums into a "Summed Multiplier" bucket
  if the affix text contains the word "Multiplier" (see section 1), or it
  is applied as its own independent Global Multiplier if it does not.
- Enabling "Advanced Tooltip Information" in Options -> Gameplay shows the
  `[x]`/`[+]` tags in-game; without it the distinction is not visible on
  tooltips.
- Source: Maxroll "Damage for Beginners"
  (`https://maxroll.gg/d4/getting-started/damage-for-beginners`) and the
  In-Depth Damage Guide above.

## 3. Where each stat sits

**[Certain]**, per Maxroll's In-Depth Damage Guide and Damage-for-Beginners
guide:

- **Weapon Damage** - base term, scales with item power; dual-wielders draw
  from both weapons.
- **Skill %** - the coefficient shown with Advanced Tooltips; increased
  only by skill ranks (and +Skill Rank affixes).
- **Main Stat (Strength for Barbarian)** - a separate multiplier, not part
  of the additive bucket. See section 4.
- **Critical Strike Chance** - a chance to apply the Critical Strike
  Damage bucket; base 5%, + stat scaling + affixes.
- **Critical Strike Damage** - additive within its own bucket (all `[+]CSD%`
  sources sum), then that summed bucket applies as one multiplier on a
  critical hit; baseline inherent x50% is a fixed multiplicative floor
  that cannot be increased.
- **Vulnerable Damage** - same structure: `[+]Vulnerable Damage%` sources
  sum into one bucket; baseline inherent x20% is fixed and separate.
- **Overpower Damage** - not a bucket at all; a flat 3% per-attack chance
  (not increasable) that adds a burst of Additive Damage scaled from your
  Life + Fortify pool, then is affected by the rest of the formula same as
  any other hit (contradicts the popular claim that Overpower "ignores"
  all buckets - Maxroll's current guide places its bonus explicitly inside
  the Additive Damage Multiplier bucket, not as an exempt separate hit
  type; an older, pre-Season-13 TheGamer guide describes Overpower as
  fully exempt from all buckets, so treat that older claim as superseded).
- **All Damage** - additive, folds into the general Additive Damage bucket
  unless the specific affix explicitly says "Multiplier" (see section 1).
- **Element-specific damage** (Physical/Fire/Cold/Lightning/Poison/Shadow)
  - additive within its own element's bucket, or Summed-Multiplier if
  tagged "Multiplier."
- **Damage Over Time damage** - additive/Summed-Multiplier by the same
  rules as direct damage; DoT ticks are calculated with the same general
  formula, just spread over the DoT's duration (see section 6).
- **Attack Speed** - does not touch the damage formula; it only changes
  how many hits/ticks land per second (DPS multiplier), see section 5.
- **Skill ranks** - increase Skill % directly (not a separate bucket).

## 4. Mainstat scaling for Barbarian (Strength)

**[Certain]** Strength is a separate multiplier from the additive buckets -
it does not stack additively with `[+]Damage` affixes; it applies as its
own "Skill Damage from Main Stat" multiplier layer, per Maxroll's In-Depth
Damage Guide:

> "This functions as a separate multiplier and is not to be confused with
> Skill %."

**[Certain] Current per-point value**: the In-Depth Damage Guide's Class
Coefficient table lists Barbarian at **9.0991 Strength per 1% Skill
Damage** (coefficient 909.91), versus 8 Main Stat per 1% for every other
class. A companion Maxroll page ("Damage for Beginners") rounds this to
"1% Skill Damage per 10 Strength." Diablo 4 Wiki / patch-note tracking
(Fextralife, reflecting a Barbarian buff applied in a recent balance pass)
independently confirms the current value as **"Damage per 10 points of
Strength increased from 1% to 1.1%"** - i.e. ~1.0991% per 10 Strength,
matching the Maxroll coefficient. This is a straight-line (no visible
diminishing-returns curve) conversion: every additional 9.0991 Strength
adds a further flat +1% to the Strength-derived multiplier layer. There is
no evidence of a soft cap or curve on this conversion itself - the
"diminishing returns" people observe from stacking Main Stat is the normal
consequence of it being one multiplicative layer among several (adding a
fixed percentage to an already-large multiplier yields a smaller relative
gain), not a special penalty on Strength.

**[Certain]** Strength also gives **+2 Armor per point** (a defensive,
unrelated effect), per Maxroll's defense guide (section 7).

## 5. Attack Speed and channeled skills (Whirlwind)

**[Certain]** Attack Speed% (AS%) is capped at 200% total, split into two
separate 100% caps drawn from different source categories (the guide does
not fully enumerate which sources fall in which of the two caps in the
extracted text). Source: Maxroll In-Depth Damage Guide changelog line:
"Attack Speed% is capped at 200%, divided into two smaller caps of 100%.
The benefit of Attack Speed depends on the skill, as each skill has unique
breakpoints."

**[Likely]** Attack Speed does not change per-hit damage; it changes how
many instances of a channeled skill's damage tick land per second, so its
value is a DPS multiplier gated by skill-specific "breakpoints" - thresholds
where an extra tick/instance becomes possible only once AS% crosses a
specific value, making AS% between breakpoints partially or fully wasted.
Maxroll's own example (unspecified skill): "between 58.8% Attack Speed and
72.5% Attack Speed, you do not benefit from AS%."

**[Unverified]** The exact numeric breakpoints for Whirlwind specifically
(where its channel damage-tick rate steps up) were not found in a
primary or build-guide source during this session; multiple current
Season 14 Whirlwind Barbarian guides (e.g. iggm.com's Season 14 guide)
reference "getting an additional attack speed breakpoint on Whirlwind" as
a real, actionable Paragon/gear choice, confirming breakpoints exist and
matter for this build, but none of the extracted sources gave the specific
percentage thresholds. Treat this as an open gap - see Gaps section.

## 6. Damage Over Time (DoT): Bleed, Burning, Poison

**[Certain]** Burning, Bleeding, and Poisoning are the three DoT damage
types, per Maxroll's Damage-for-Beginners guide, which states direct damage
and DoT "behave exactly" the same with respect to the general formula (both
scale through Weapon Damage, Skill%, Main Stat, and the additive/summed
multiplier buckets); the only structural difference is that DoT damage is
spread over its duration rather than applied instantly, and Weapon Speed
(attack speed) does not affect a non-stacking DoT's total damage at all.

**[Certain] Live open question - is a DoT Multiplier affix worth it on a
2H Sword Barbarian who plays a "direct damage" skill?** Two independently
confirmed current-patch sources answer this directly:

1. Diablo 4 Wiki (Fextralife) "Two-Handed Sword Expertise" passive node
   page (`https://diablo4.wiki.fextralife.com/Two-Handed+Sword+Expertise`):
   "A percentage of direct damage you deal is inflicted as bleeding damage
   over 5 seconds," scaling from 2% (Rank 1) to 20% (Rank 10) of your
   direct damage, converted into Bleed DoT, plus a Rank-10 bonus of +30%
   increased Bleeding damage for 5s after a kill.
2. The Fextralife consolidated Patch Notes page documents a separate,
   baseline **Weapon Mastery** effect (applies simply from wielding a
   Two-Handed Slashing weapon - i.e. a sword - regardless of the above
   skill-tree node), most recently tuned in Patch 3.0.2: "Two-Handed
   Slashing initial damage increased from 20% to 40%. Two-Handed Slashing
   bleeding damage increased from 120% to 240%." That is, of the weapon's
   own bonus-damage instance, **40% is dealt as direct damage and 240%
   (six times as much) is dealt as Bleeding DoT.**

**[Likely] Verdict**: for a Barbarian using a Two-Handed Sword, a large
majority of the weapon-expertise damage bonus (240% out of a 280% total,
i.e. ~86%) is delivered as Bleed DoT, independent of whether the
character's active skill (e.g. Whirlwind, Hammer of the Ancients) is
itself a "direct damage" skill. A "Damage Over Time Multiplier" affix
therefore multiplies a large, guaranteed slice of total damage output on
any 2H-Sword Barbarian, and should generally be carried rather than
dismissed as irrelevant to a "direct damage build." The skill-tree Two-Handed
Sword Expertise node adds a further, smaller, separate conversion (up to
20% of direct damage) on top of the baseline Weapon Mastery split. This
does not mean DoT Multiplier is definitionally better than Vulnerable/Crit
Multiplier for a given build - that depends on total investment in each
bucket (section 1) - only that it is not wasted or "off-bucket" for a
2H-Sword Barbarian.

## 7. Defence: Armor, Damage Reduction, Resistances, Fortify, Barrier, Toughness

Primary source for this whole section: Maxroll "In-depth Defense Guide"
(`https://maxroll.gg/d4/getting-started/defenses-for-beginners`).

**[Certain] Armor**

- Scales linearly with an armor piece's item power; also **+2 Armor per
  point of Strength** for every class.
- Converts to Damage Reduction via
  `DR% from Armor = Armor / (Armor * 10/9 + Constant)`, with Constant =
  5678 at character level 70 (smaller at lower levels). The `10/9` term
  means DR from Armor asymptotically approaches, but never reaches, 90% -
  a built-in, ever-increasing diminishing return, not a hard cap.

**[Certain] Resistances**

- Cold/Fire/Lightning/Physical/Poison/Shadow, each tracked separately;
  reduces damage taken only from that element.
- Same-shaped formula: `DR% from Resistance = Resistance / (Resistance*10/9 + Constant)`,
  Constant = 1136 at level 70. Also asymptotically approaches 90% DR, no
  hard cap, strong diminishing returns at high values.
- Sources: Jewelry implicit Resistance, gear Resistance affixes, Gems,
  Paragon, Skills, the Mercenary Raheir; Intelligence also grants +0.4 All
  Resistance per point (for every class, not just Sorcerer/Necromancer).

**[Certain] Damage Reduction (DR)**

- All independent DR sources (Armor's DR, each Resistance's DR, and any
  flat "% Damage Reduction" affix/skill/passive) stack **multiplicatively
  with each other**, not additively: a 20% DR source and a 40% DR source
  together give `1 - (1-0.20)*(1-0.40) = 52%` total DR, not 60%. This
  means each additional flat DR source is worth less in isolation, but a
  DR source is never "wasted" the way additive-bucket stacking can be.
- Every class takes an extra flat 92% Damage Reduction in PvP specifically.

**[Certain] Fortify**

- A Life-like shield mechanic available via specific skills/passives/
  Aspects (notably strong on Barbarian, Druid, Necromancer, Paladin,
  Warlock). While you hold any Fortify you are "Fortified," which is
  itself a condition many affixes/passives key off of (e.g. "+X% damage
  while Fortified").

**[Certain] Barrier**

- A temporary Life-like pool that depletes before Life on incoming
  damage; multiple simultaneous Barriers add their amounts together but
  track duration independently per source. Most Barrier effects scale off
  your Maximum Life, so (outside of the Melted Heart of Selig build in
  section 8) stacking Life is usually the efficient way to make Barriers
  bigger, rather than stacking flat "Barrier Generation."

**[Certain] Toughness**

- Not a real stat, but a distilled single number for comparing tankiness
  across builds/characters. For each damage-type category, Toughness is
  computed as `(Maximum Life + Barrier) / (1 - total active DR against that type)`
  - i.e. the effective-health-points value against that specific damage
  type, using only DR effects that are "active" (unconditional or
  currently satisfied conditions - e.g. a "DR vs Close enemies" passive is
  excluded unless you are actually close). The single "Combined Toughness"
  figure shown on the character sheet is the mean of the per-damage-type
  Toughness values and is explicitly called out by Maxroll as "not real" -
  a rough comparison number only, not something to optimize directly
  (optimize the underlying Life/Barrier/DR/Resistance values instead).

## 8. Melted Heart of Selig and Maximum Life

**[Certain] Current verbatim unique power** (Mythic Unique Amulet, item
power 800, all classes), per purediablo.com's Diablo 4 item wiki, which
also documents the patch that introduced this exact wording:

> "Damage is dealt to your Primary Resource before Life and damage taken
> this way is drastically reduced. Your Primary Resource is doubled but
> Maximum Life is reduced by 75%."

This wording was introduced in **Patch 2.5.0** (dated 11 December 2025 by
that source), replacing the older wording ("Gain 60 Maximum Resource. When
taking damage, 75% is drained as 2 Resource for every 1% of Maximum Life
you would have lost"). Nothing in the Patch 3.1.0/3.1.1/3.1.2 (Season 14)
notes extracted this session changed this specific power's text again,
though a Season-14 tuning pass did reduce the effective damage-reduction
strength of builds using it (see below) without altering the printed
tooltip. The Fextralife item page (last touched for Patch 3.1.0, Season 14)
independently corroborates the "damage redirected to Resource, scaled off
Maximum Life lost" mechanic still being live this season.

**[Certain] The item itself imposes a flat -75% Maximum Life penalty** as
part of its own unique power (not a build choice - it is baked into
wearing the amulet). Your Primary Resource (Fury, for Barbarian) pool is
doubled, and incoming damage is drained from that (now-inflated) Resource
pool before it touches Life, "drastically reduced" in the process.

**[Likely] How Maximum Life on the REST of your gear interacts with this**:
current Season 14 build guides (e.g. nexttier.pro's Selig guide, cross-
checked against the mechanic description above) state the damage-to-
Resource conversion scales with your Maximum Life - a smaller Maximum Life
means a given raw hit converts to a smaller absolute Resource cost, and
also means each hit represents a bigger fraction of a value that is itself
being pushed toward zero, which is what the build deliberately exploits.
Quoting the guide directly: "The key detail is that the damage-to-resource
conversion scales with your Maximum Life: the less Life you have, the
smaller each hit that gets converted becomes. That single line is why the
amulet is built around having zero Life, not stacking it - which is the
opposite of a normal defensive stat." The same source explicitly warns:
"The one thing to ignore is any older guide that tells you to pair it with
Maximum Life - that is backwards."

**[Likely] Direct verdict on the user's "strip all Maximum Life" approach**:
**correct, and confirmed by current (Season 14 / patch 3.1.x) build
guidance for this specific item.** Stacking Maximum Life on gear/Paragon/
charms works against Melted Heart of Selig's core mechanic; the build
wants near-zero Maximum Life outside what's structurally required, and
leans on the inflated, doubled Resource pool (plus Resource Generation) as
its effective health pool instead. Note this is item-specific advice - it
applies to gear/Paragon/Talisman investment while wearing this exact
amulet, not to Maximum Life as a stat in general on builds that do not use
Selig.

**[Likely] Season 14 nerf context**: per the same build-guide source, "Patch
3.1 tuned that down - the immortal variant lost roughly 20% damage
reduction," and a related Fextralife bugfix note lists "Fixed an issue
where Barbarians could become effectively invulnerable when using Melted
Heart of Selig and building around it in a certain way" as a Season-14-era
fix - both corroborate that the zero-Life Selig loop was intentionally
weakened this season but is still described as viable/strong, not removed.

## Sources

- Maxroll "In-Depth Damage Guide" -
  `https://maxroll.gg/d4/resources/in-depth-damage-guide` - page states its
  own update log entries through "Updated for Season 13" (Summed
  Multipliers, Attack Speed caps) as its most recent dated change; no
  Season 14 update line was present in the extracted text.
- Maxroll "Damage for Beginners" -
  `https://maxroll.gg/d4/getting-started/damage-for-beginners` - changelog
  shows most recent dated entries "Updated for Season 9" / "Updated for
  Season 8."
- Maxroll "In-depth Defense Guide" -
  `https://maxroll.gg/d4/getting-started/defenses-for-beginners` - changelog
  entry visible: "Updated Armor and Resistances to follow a rating system...
  Included section for the new Toughness stat" (no explicit season number
  captured in the extracted text).
- Maxroll "Lord of Hatred - 3.1.2 Patch Notes for Diablo IV Season 14 -
  Death Awakening" -
  `https://maxroll.gg/d4/news/lord-of-hatred-3-1-2-patch-notes` - states
  Patch 3.1.2 released 28 July 2026.
- Mobalytics "Diablo 4 Patch Notes 3.1.1 Are Here!" -
  `https://mobalytics.gg/diablo-4/guides/patch-notes-3-1-1-season-14` -
  states Patch 3.1.1 released Tuesday 14 July 2026; links official Blizzard
  notes at `https://news.blizzard.com/en-us/article/24287406/diablo-iv-patch-notes`.
- Diablo 4 Wiki (Fextralife) consolidated Patch Notes -
  `https://diablo4.wiki.fextralife.com/Patch+Notes` - source for the
  Barbarian Two-Handed Slashing Weapon Mastery numbers (Patch 3.0.2
  section) and the Strength scaling change ("1% to 1.1% per 10 points").
- Diablo 4 Wiki (Fextralife) "Two-Handed Sword Expertise" -
  `https://diablo4.wiki.fextralife.com/Two-Handed+Sword+Expertise`.
- Diablo 4 Wiki (Fextralife) "Melted Heart of Selig" -
  `https://diablo4.wiki.fextralife.com/Melted+Heart+of+Selig` - notes "2
  Fixed Affixes added in Patch 3.1.0... for Season 14."
- purediablo.com "Melted Heart of Selig Mythic Unique Amulet" -
  `https://www.purediablo.com/diablo4/Melted_Heart_of_Selig` - includes
  dated patch-history table; current wording attributed to Patch 2.5.0 (11
  December 2025).
- nexttier.pro "Diablo 4 Melted Heart of Selig (Season 14): Effect & Build" -
  `https://nexttier.pro/guide/diablo-4-melted-heart-of-selig` - build-guide
  interpretation of the Maximum-Life interaction and the Season 14 tuning
  pass; not a primary Blizzard source, treated as [Likely].
- TheGamer "What Are Damage Buckets? Diablo 4" -
  `https://www.thegamer.com/diablo-4-damage-scaling-guide` - older
  (pre-Season-13-looking) community explainer, used only for supplementary
  framing; its claim that Overpower is fully exempt from all damage
  buckets is contradicted by the newer Maxroll In-Depth Damage Guide and
  should be considered superseded.
- Reddit r/diablo4 "DAMAGE AND HOW IT WORKS - Buckets, Additives,
  Multiplicatives, etc." -
  `https://www.reddit.com/r/diablo4/comments/1tu0tez/` - community
  discussion corroborating that Patch 1.2 (the original "Damage 2.0"
  overhaul, Season 2 era) moved Critical Strike/Vulnerable/DoT/elemental
  damage into additive-within-bucket behavior; used as corroboration only,
  not primary.
- IGN "Diablo 4 2.4.0 Patch Notes" -
  `https://www.ign.com/wikis/diablo-4/Diablo_4_2.4.0_Patch_Notes` - contains
  the original Blizzard framing of the Patch 1.2.0 Critical/Vulnerable
  rework ("these stats were previously calculated as part of separate
  damage buckets that were fully multiplicative"); links official Blizzard
  post at `https://news.blizzard.com/en-gb/diablo4/23964909/diablo-iv-patch-notes`.
- iggm.com "Diablo 4 Season 14 Whirlwind Barbarian Guide" -
  `https://www.iggm.com/news/diablo-4-season-14-whirlwind-barbarian-guide-speedrun-pit-120-tank-super-bosses-without-mythic-gear` -
  confirms Whirlwind-specific attack-speed breakpoints are a live, relevant
  concept this season, without giving exact thresholds.

## Gaps

- **Exact Whirlwind attack-speed breakpoint values** (the specific AS%
  thresholds where Whirlwind's channel gains an extra damage tick) were
  not found. Multiple Season 14 guides reference chasing "an extra
  breakpoint on Whirlwind" as a real gearing decision, but none of the
  extracted pages gave the numeric thresholds. Needs a dedicated Maxroll
  "attack-speed-mechanics" page extraction (referenced but not itself
  fetched this session:
  `https://maxroll.gg/d4/resources/attack-speed-mechanics`) or an in-game
  test.
- **Which sources count toward AS%'s "first" 100% cap vs the "second"**
  100% cap was not resolved - the extracted Maxroll text asserts the split
  exists but the enumeration of which categories (gear vs paragon vs
  buffs) land in which cap was cut off in the extraction.
- **No Season-14-dated update note was found on Maxroll's core
  Damage/Defense guides themselves** confirming the Summed-Multiplier and
  bucket model is explicitly unchanged for Season 14/patch 3.1.x - the
  most recent visible changelog line on the In-Depth Damage Guide says
  "Updated for Season 13." Patch 3.1.0/3.1.1/3.1.2 notes reviewed this
  session contained no line describing a core formula change, which
  supports (but does not from a dedicated source directly confirm) that
  the Season 13 bucket model carried forward unchanged.
- **Overpower's exact current bucket placement** is contested between two
  Maxroll pages: the newer In-Depth Damage Guide places it inside the
  Additive Damage Multiplier bucket, while the still-live "Damage for
  Beginners" companion page (last dated update "Season 9") talks about it
  more like a bolt-on separate multiplier event. Did not fully reconcile
  the two pages' wording against each other line by line.
- **The Fextralife "Melted Heart of Selig" page's own quoted Unique Effect
  text contains an apparent OCR/typo** ("75% is grained as 2 Resource...")
  that does not match either the older wording ("drained") or the
  purediablo.com current wording ("Damage is dealt to your Primary
  Resource before Life..."). Treated purediablo's fuller, dated,
  patch-history-backed text as authoritative, but did not find a third
  independent source quoting the exact current tooltip character-for-
  character to fully triangulate.
- Did not verify whether the Barbarian Strength-to-Armor ratio (+2 Armor
  per point) or the Intelligence-to-Resistance ratio (+0.4 per point) have
  changed in Season 14; these numbers came from Maxroll's Defense guide
  without a Season-14-specific changelog line.
