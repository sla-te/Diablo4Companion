# Barbarian Whirlwind - Season 14 "Death Awakening" (Lord of Hatred, patch 3.1.x)

Snapshot date: 2026-07-28. Describes the live patch as of that date. D4 balance
patches roughly every three months - re-verify all figures below before relying
on them for a new season or patch.

## 1. Weapon / gear stat priority per slot

[Certain] Source: icy-veins.com/d4/guides/whirlwind-barbarian-build, "Stat
Priority and Tempering Affixes" section (accessed 2026-07-28, page marked
Season 14). Quoted verbatim, order = priority, bold = best Masterworking
target per icy-veins' own key:

| Slot | Gear Affixes (priority order) | Tempering Affix |
| --- | --- | --- |
| Helm | 1. Cooldown Reduction (bold/best-MW) 2. Maximum Life 3. Strength 4. Armor | + Resistance to All Elements (Defensive) |
| Chest | 1. Strength 2. Maximum Life (bold/best-MW) 3. Fury Generation 4. Armor | + Resistance to All Elements (Defensive) |
| Gloves | 1. Attack Speed (bold/best-MW) 2. Strength 3. Critical Strike Chance 4. Physical Damage Multiplier | + Critical Strike Damage (Offensive) |
| Pants | 1. Strength 2. Maximum Life (bold/best-MW) 3. Fury Generation 4. Armor | + Resistance to All Elements (Defensive) |
| Boots | 1. Movement Speed (bold/best-MW) 2. Strength 3. Maximum Life 4. Fury Generation | + Movement Speed (Mobility) |
| Amulet | 1. Critical Strike Chance (bold/best-MW) 2. Critical Strike Damage Multiplier 3. Attack Speed 4. Physical Damage Multiplier | + Cooldown Reduction (Resource) |
| Rings | 1. Critical Strike Chance (bold/best-MW) 2. Critical Strike Damage Multiplier 3. Physical Damage Multiplier 4. Attack Speed | + Cooldown Reduction (Resource) |
| Two-Handed Bludgeoning Weapon (mace/polearm) | 1. Weapon Damage (bold/best-MW) 2. Strength 3. Maximum Life 4. Critical Strike Damage Multiplier | + Critical Strike Chance (Weapon) |
| Two-Handed Slashing Weapon (sword/axe) | 1. Strength 2. Critical Strike Damage Multiplier 3. Maximum Life 4. Weapon Damage (bold/best-MW) | + Critical Strike Chance (Weapon) |

[Unverified] The icy-veins page also had a "Dual-Wield Weapon" row in the same
table, but the extracted snippet was truncated before that row's four affixes
came through cleanly - only the row header was captured. Re-check the live
page for the exact Dual-Wield priority order.

[Likely] Mobalytics' Whirlwind planner (mobalytics.gg/diablo-4/builds/barbarian-whirl-wind-barb)
gives a materially different loadout for its "safe"/non-Mythic variant: it
targets **Ranks to Core/Martial Skills on 2 Charms** and **Maximum Resource on
3 Charms**, with the Seal affix as "any +Damage(x)". It also explicitly bans
**Maximum Life on any Charm or Seal** when running the Melted Heart of Selig
variant (see section 5) - this directly conflicts with icy-veins' generic
"Maximum Life" priority on Chest/Pants/Boots, so which one applies depends on
which Whirlwind variant (Grandfather vs. Melted Heart) you are building.

## 2. Arsenal system - do all equipped weapons apply at all times?

[Certain] Quoted directly from maxroll.gg/d4/resources/barbarian-arsenal-system
(accessed 2026-07-28):

> "If you mainly use 1 damaging skill, all other weapon slots become stat
> sticks to boost your primary weapon."

Verdict: **No**, weapon-type Expertise/Technique bonuses are NOT all active
simultaneously. Mechanism, per the same page:

- You assign a specific weapon to your main damage-dealing skill via the
  Skill Assignment menu ("Cycle Weapons" on the skill bar icon). This is the
  Arsenal Selection for that skill.
- Separately, a "Weapon Expertise" (called the Technique slot in some guides)
  is chosen, and its passive-style bonus (see table below) applies globally
  based on the weapon TYPE assigned to Technique - independent of which
  weapon is actively swinging for damage.
- Weapons not assigned to your active damage skill's Arsenal slot, and not
  providing the Technique bonus, exist purely as "stat sticks" - their raw
  affixes (Strength, Weapon Damage roll, sockets, etc.) still count toward
  your character sheet, but their weapon-type Expertise bonus does not
  additionally stack on top of the assigned Technique.

## 3. Weapon Expertise - current per-type bonuses and how the active one is chosen

[Certain] Full table quoted from maxroll.gg/d4/resources/barbarian-arsenal-system
via game8.co/games/Diablo-4/archives/609686 (both echoed the same table,
2026-07-28):

| Weapon Type | Passive Bonus 1 | Passive Bonus 2 |
| --- | --- | --- |
| 1H Mace | x10% increased damage to Stunned enemies. Double this amount when using two Maces. | Lucky Hit: Up to 10% chance to gain Berserking for 1.50 seconds when you hit a Stunned enemy. Double this chance when using two Maces. |
| 2H Axe | x10.0% increased damage to Vulnerable enemies. | +10% increased Critical Strike Chance against Vulnerable enemies. |
| Polearm | +15% increased Lucky Hit Chance. | You deal x15% increased damage while Healthy. |
| 2H Sword | x20% of the Base direct damage dealt is inflicted as Bleeding damage over 5 seconds. | You deal x30% increased Bleeding damage for 5 seconds after killing an enemy. |
| 2H Mace | Lucky Hit: Up to 10% chance to gain 5 Fury when hitting an enemy. Double the amount of Fury gained while Berserking. | You deal x15% increased Critical Strike Damage while Berserking. |

[Likely] How the active Expertise is determined: per Mobalytics' Barbarian
Passive Guide (mobalytics.gg/diablo-4/guides/barbarian-passive-guide, 2026-07-28):

> "Each weapon has its own guaranteed implicit affix... with the two-handed
> axe selected as your weapon technique, you can get the 10% increased damage
> to vulnerable enemies even if you're using a mace, but you will not get the
> increased crit chance against vulnerable enemies. ... if you had a
> two-handed axe assigned to your main damaging skill AND had a two-handed
> axe assigned as your weapon technique, you would receive both bonuses."

This confirms there are effectively TWO independent selections: (a) the
weapon TYPE assigned as your "Technique" (grants the passive-only half of the
Expertise bonus regardless of what you're swinging), and (b) the weapon
actually assigned to your damage skill via Arsenal Selection (grants the
full bonus only if its type matches the Technique). It is not "all weapon
types at once," and it is not strictly "only the weapon currently swinging" -
it is a Technique-type match against whichever weapon is doing the swinging.

[Unverified] The exact current UI name ("Technique" vs "Expertise slot") and
whether this mechanic changed name/mechanics for Season 14 specifically could
not be independently confirmed beyond the Mobalytics passive guide; the
Maxroll Arsenal resource page's own text uses "Expertise" throughout without
the word "Technique."

## 4. 2H Sword Expertise bleed conversion - does a DoT multiplier affect it?

[Certain] The conversion is confirmed verbatim (see table above): "x20% of
the Base direct damage dealt is inflicted as Bleeding damage over 5 seconds."
This applies "When using any weapon" per the game8-mirrored text, i.e. once
2H Sword is your active Technique, every hit (regardless of which physical
weapon connects) converts 20% of its base direct damage into a 5-second
Bleed DoT tick.

[Likely] Direct answer to the open question: **Yes**, a Whirlwind build using
this 2H Sword Expertise (e.g. wielding The Grandfather) benefits from a
"Damage over Time Multiplier" affix, because the 20% conversion explicitly
creates Bleeding **damage over time** - a separate damage instance from the
initial hit, subject to Bleed-scaling multipliers (Overpower-to-Bleed
interactions aside). game8's build guide states this outright: "The
Two-Handed Sword Expertise is the best specialization for the Whirlwind
Barbarian build to enable our Whirlwind with Bleeding effects and maximize
our damage output" - i.e. the build is explicitly built around stacking
Bleeding/DoT scaling once this Expertise is active.

[Unverified] I could not find a primary source that explicitly states
"Damage over Time Multiplier temper/affix multiplies this specific Bleed
tick" in so many words (e.g. a Maxroll sentence naming the affix by its exact
in-game string). The conclusion above is inferred from the mechanic (any
Bleeding-over-time source scales with DoT multipliers in D4's general damage
formula) plus game8's build framing, not from a sentence that names the
affix directly next to the 2H Sword Expertise. Treat the mechanism as solid,
the exact affix-name interaction as inferred.

## 5. Mythic variants: Grandfather vs. Melted Heart of Selig

[Likely] Per iggm.com's Season 14 Whirlwind guide (accessed 2026-07-28):
"the biggest highlight of Season 14 Whirlwind Barbarian is achieving a
balance of damage, survivability, and mobility without Mythic gear, and its
two variants offer great flexibility" - confirming there are exactly two
named Mythic-driven variants this season, matching Grandfather and Melted
Heart of Selig.

[Likely] Per Mobalytics' Whirlwind planner (2026-07-28): "The Barbarians this
season will all be running some variation of Melted Heart of Selig + [a
second Mythic]." Mobalytics frames Melted Heart of Selig as the higher-power
but fragile/setup-dependent option:

> "WARNING: Running Melted Heart of Selig requires very specific set ups.
> What you will need to do: No Maximum Life on any Charms or Seals. Endurant
> Faith charm is required... Tibault's Will - This comes on top of your other
> Fury Regeneration. If you don't meet these three requirements this variant
> will not work and you will die. Please use the Mythic Variant [i.e. the
> non-Melted-Heart / Grandfather-style setup] until you have everything set
> up."

Gear/skill differences observed between the two variants:

- **Melted Heart of Selig variant** (Mobalytics): requires zero Maximum Life
  rolls on Charms/Seals, mandates the Endurant Faith Mythic charm and
  Tibault's Will as enabling pieces, uses **Polearm** as the chosen Expertise
  (15% Lucky Hit Chance, 15% increased damage while Healthy) with **Mace**
  as the Arsenal Selection for Whirlwind (Lucky Hit: chance to gain 5 Fury,
  doubled while Berserking; +15% Crit Damage while Berserking). This is a
  non-Bleed, crit/Berserking-stacking build.
- **Grandfather variant** (icy-veins): icy-veins' base Whirlwind build
  actually defaults to **Polearm Expertise** too (for the Healthy-state 15%
  damage bonus) with a **Two-Handed Bludgeoning** (mace/polearm) weapon
  assigned to Whirlwind, and treats The Grandfather explicitly as a
  stat-stick rather than the active Whirlwind weapon: "While this might lack
  the typical weapon damage affix meaning we'd be using our Bludgeoning
  weapon for whirlwind, the 150%x critical strike damage multiplier is no
  less potent... this sword retains its power and can be equipped if you're
  fortunate enough to snag one." This CONTRADICTS game8's framing (section 4
  above), which recommends **2H Sword Expertise** specifically to enable
  Bleed scaling with Whirlwind. **[Unverified / open gap]**: whether the
  current S14-optimal "Grandfather variant" actually assigns Whirlwind to a
  2H Sword (bleed-focused, per game8) or keeps Polearm/mace as Technique and
  uses Grandfather as a stat-stick (per icy-veins) - the two build sites
  disagree, and I could not find a tie-breaking primary source (e.g. a
  Maxroll Grandfather-specific build page) in this session.

## 6. Berserker's Crucible charm set bonuses

[Certain] Quoted from diablo4.wiki.fextralife.com/Berserker's+Crucible+Set
(accessed 2026-07-28):

- (2) Set: "Berserking's damage bonus is increased by 50% and it also grants
  20% Cast Speed."
- (3) Set: "Gain 30% Damage Reduction while Berserking."
- (5) Set: "You gain stacking bonuses the longer you stay Berserking: 5
  seconds - 200% Damage and 30% Fury Cost Reduction. 10 seconds - 10 Fury Per
  Second and 10% Cooldown Reduction. 20 seconds - Every 5 seconds, trigger a
  random Shout Skill. 60 seconds - 30% increased damage against Elites."

[Certain] BUT the final figure in that (5) Set text is stale. icy-veins'
Season Updates section for the Whirlwind build (accessed the same day)
states explicitly: "The Talisman Set 'Berserker's Crucible' had its 5-piece
Elite damage bonus reduced from 30% to 10%." So the CURRENT live value for
the 60-second Elite-damage tier of the 5-piece bonus is **10%**, not the 30%
still printed on the Fextralife wiki page. All other figures (2-piece,
3-piece, and the first three 5-piece thresholds) were not called out as
changed and are treated as current.

## 7. Barbarian / Whirlwind tier standing and Season 14 balance changes

[Certain] Maxroll's "Barbarian Endgame Builds Tier List for Diablo IV Season
14 - Death Awakening" (maxroll.gg/d4/tierlists/barbarian-endgame-tier-list,
accessed 2026-07-28) lists **Whirlwind Barb in its S-Tier** builds list,
alongside Minion Barb and Mighty Throw Barb.

[Likely] iggm.com's Season 14 Whirlwind guide frames it as capable of
"speedrun[ning] Pit 120 and tank[ing] all the super bosses without Mythic
gear," calling out damage/survivability/mobility balance as its defining
strength this season.

[Certain] Season 14 balance notes affecting this build, quoted from
icy-veins' Season Updates section for the Whirlwind build:

- "Challenging Shout the damage bonus upgrade can no longer affect bosses
  since they can't be taunted by this skill."
- "The Talisman Set 'Berserker's Crucible' had its 5-piece Elite damage bonus
  reduced from 30% to 10%." (see section 6)
- "Cremator's Aspect damage reduced from 70-100% to 45-65%."
- "Tidal Aspect maximum overpower stacks reduced from 2-4 to 1-3 and it is
  now a utility aspect rather than offensive aspect."
- "Aspect of Glynn's Anvil damage reduction is now capped at 40%."
- "Fury gained from taking damage is now capped up to 0.5 Fury per 1% of your
  Maximum Life and no longer benefits from increased resource generation."

[Unverified] Whether these are Season-14-launch changes or a mid-season
hotfix, and the exact patch number (3.1.0 vs. a later 3.1.x point patch) they
landed in, was not confirmed - the icy-veins page did not date-stamp
individual bullets.

## Sources

- <https://maxroll.gg/d4/resources/barbarian-arsenal-system> - Maxroll,
  Barbarian Arsenal System resource page. Update date not visible on the
  extracted content.
- <https://www.icy-veins.com/d4/guides/whirlwind-barbarian-build> - Icy Veins,
  "Whirlwind - Barbarian Build for Diablo 4 (Season 14)". Page self-tagged
  "EndgameGuideLord of HatredFrenzy Season 14"; no explicit last-updated date
  visible in extracted content.
- <https://game8.co/games/Diablo-4/archives/609686> - Game8, "Whirlwind
  Barbarian Endgame Build (Season 14)". Page metadata showed archive_revision
  20260504023447 (i.e. last revised 2026-05-04 per the tracking pixel URL).
- <https://mobalytics.gg/diablo-4/builds/barbarian-whirl-wind-barb> - Mobalytics,
  "Whirlwind - Diablo 4 Barbarian Build Guide". No explicit date visible in
  extracted content; page references "Season 14" content and active promos.
- <https://mobalytics.gg/diablo-4/guides/barbarian-passive-guide> - Mobalytics,
  "Dalkora's Diablo 4 Barbarian Passive Guide" (Weapon Expertise mechanics).
  No explicit date visible in extracted content.
- <https://diablo4.wiki.fextralife.com/Berserker's+Crucible+Set> - Fextralife
  D4 Wiki, Berserker's Crucible Set page. No last-updated date visible in
  extracted content; contains at least one stale figure (see section 6).
- <https://maxroll.gg/d4/tierlists/barbarian-endgame-tier-list> - Maxroll,
  "Barbarian Endgame Builds Tier List for Diablo IV Season 14 - Death
  Awakening". No explicit last-updated date visible in extracted content.
- <https://www.iggm.com/news/diablo-4-season-14-whirlwind-barbarian-guide-speedrun-pit-120-tank-super-bosses-without-mythic-gear> -
  IGGM, "Diablo 4 Season 14 Whirlwind Barbarian Guide". No explicit date
  visible in extracted content.
- <https://maxroll.gg/d4/build-guides/whirlwind-barbarian-leveling-guide> -
  Maxroll, Whirlwind Barbarian Leveling Guide (linked from the S-Tier list;
  used to confirm the endgame guide's existence and URL pattern, not
  quoted directly for stat priority).

## Gaps

- Could not confirm the "Dual-Wield Weapon" stat-priority row from icy-veins'
  table in full (row header captured, four affix lines were cut off by
  extraction).
- Could not find a primary source explicitly naming the "Damage over Time
  Multiplier" affix string side-by-side with the 2H Sword Expertise Bleed
  conversion; the DoT-multiplier interaction (section 4) is a mechanical
  inference, not a directly quoted confirmation.
- Direct conflict between icy-veins (Polearm Expertise + Grandfather as
  stat-stick) and game8 (2H Sword Expertise for Bleed synergy) on what the
  "Grandfather variant" actually looks like in Season 14 - could not
  resolve which is the current community-preferred configuration, or
  whether both are viable sub-variants.
- Could not verify the exact patch number (e.g. 3.1.0 vs. a specific 3.1.x
  hotfix) for the Season 14 balance changes listed in section 7, nor a
  precise last-updated date for any of the sourced pages (none of the
  fetched pages exposed a clear "last updated" timestamp in the extracted
  markdown, aside from Game8's revision-tracking pixel).
- Did not independently verify Maxroll's own dedicated Whirlwind Barbarian
  Endgame Guide page content (only its Leveling Guide and Tier List pages
  were fetched/quoted); the endgame guide likely has its own stat-priority
  and Grandfather/Melted-Heart breakdown that could resolve the gap above,
  but was not fetched this session.
