# Diablo 4 helper - durable traps and correct reasoning

Each entry is a mistake that has actually happened, framed as a durable rule.
The rules are stable; any concrete example is time-stamped and must itself be
re-verified, because it will also go stale.

## 1. Never state the current season from memory

The training cutoff is always behind the live game. Stating a season number or
theme from memory has been wrong. **Always resolve the current season with a
live lookup** before answering anything season-scoped, then hang the rest of the
answer off that. If the lookup fails, say the season is unconfirmed rather than
guessing.

## 2. Overlay red-marks are a stat-list artifact, not a "don't use"

D4Companion marks item affixes against an imported build preset. A **red mark
means "this line is not in the preset's tracked stat-priority list," not "the
build does not want this item."** Charms in particular carry fixed unique effects
that the preset does not grade as affixes, so a recommended charm can show its
effect line in red. **Confirm charm/item recommendations against the actual build
guide, never from the overlay colour alone.** (Confirmed the hard way: a charm
the Maxroll build explicitly includes was twice called "bad for the build"
because its effect line was red in the overlay.)

Related overlay constraint (from memory `project-d4companion-overlay-capture-feedback`):
the app captures its own overlay via `CAPTUREBLT`, so any element drawn near the
tooltip corrupts detection - overlay elements must be anchored to a window corner.

## 3. Random-damage uniques are strong on high-hit-rate skills

A unique whose effect is "attacks randomly deal X% to Y% of normal damage" (e.g.
Fists of Fate) looks like a gamble, but on a **high-hit-frequency skill the
variance averages out** to a large, effectively reliable multiplier. Whirlwind is
the highest-hit-rate skill in the game, so such uniques are consistent there, not
risky. Do not dismiss a random-effect unique as bad without accounting for the
skill's hit rate - and defer to the build guide, which already prices this in.

## 4. Grade gear against the guide, not by feel

"Is this an upgrade?" must be answered by pulling the specific build guide's
per-slot stat priority live and scoring the item's stat match against it, then
ranking by gap. Note the guide *variant* (e.g. Midgame vs Endgame - they have
different targets). State the limit: stat-match is not a DPS simulation; a
higher-item-power piece with worse stats can still be a downgrade, and vice
versa.

## 5. Damage buckets are separate multipliers unless a patch says otherwise

Different damage categories (e.g. All Damage vs Vulnerable vs Damage Over Time)
are generally **separate multiplicative buckets**, not one additive pool - so
"All Damage" does not subsume a conditional multiplier. This is a common user
misconception ("isn't All Damage the same as Vulnerable, all in one?"). The
bucket layout is patch-dependent: state the general principle, but verify the
current-patch specifics before giving exact stacking behaviour or roll ranges.

## 6. Crafting has hidden prerequisites - verify the full recipe

Cube/crafting recipes frequently require conditions beyond the obvious input:
the source item must often be **Ancestral**; conversions can **re-roll** the
power and **consume** the item; material costs change between patches. The
common real-world failure is "I can find the item but the Cube won't let me
craft it" because a prerequisite (Ancestral, material count) is unmet. Always
surface prerequisites and the "it consumes/re-rolls" caveats, and tell the user
the Cube shows the live material cost when the item is slotted.

## 7. Do not repeat gold-farming-site names or NPC/place names from memory

Third-party gold/boost sites and hastily-read guides introduce misspelled NPC
and location names. Verify proper nouns (boss names, zone names, NPC names)
against the wiki before repeating them.
