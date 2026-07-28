# Horadric Cube - complete recipe reference

Source: <https://maxroll.gg/d4/resources/horadric-cube> (author Icytroll, reviewed by
Avarilyn & Wudijo). Extracted and verified **2026-07-26**, page state "Updated for
Season 14", changelog top entry "Added Mythic recipe."

**Staleness rule:** this file is a snapshot. Re-verify against the live page before
quoting costs or probabilities in a new season, and always re-check the Seasonal
recipes - "Upgrade to Mythic" is explicitly flagged `(Seasonal)` and will change or
vanish. Everything below is quoted or condensed from the source, not from memory.

## Why this file exists

Every "is X an upgrade?" answer must consider the Cube, not just the drop table. The
Cube can create, convert, reroll, and quality-boost items, so an item the player does
not own may still be one recipe away, and a junk item in the stash may be a crafting
input rather than vendor trash. Read this before grading gear.

## Basics

- Unlocked by playing through the **Lord of Hatred** campaign; the Cube is located in
  **Temis** afterwards.
- Core materials are the **Primordial Dust** family and **Tuning Prisms**. Seasonal
  recipes add their own currency (S14: **Pandemonium Fragment**).
- Dust tiers seen across recipes, cheapest to most restricted: Raw, Coarse, Refined,
  Enhanced, Pure, Attuned, Volatile.

## Tuning Prisms

Prisms steer random recipes toward a deterministic result. Two families.

**Affix-category prisms** (narrow which affix category a recipe touches):

| Prism | Affixes it can target |
|---|---|
| Aggressive | Mainstat, Weapon Damage, Attack Speed, Critical Strike Chance, Critical Strike Damage [x], Vulnerable Damage [x], DoT Damage [x], All Damage [x], Elemental Damage [x], Thorns |
| Pragmatic | Barrier Generation, Cooldown Reduction, Fortify Generation, Healing Received, Impairment Reduction, Life Regeneration, Lucky Hit Chance, Movement Speed, Potion Capacity, Thorns, Maximum Evade Charges, Attacks reduce Evade Cooldown, Evade grants Movement Speed |
| Protector's | Armor, Damage Reduction, Dodge Chance, Fortify Generation, Life on Hit, Life on Kill, Life Regeneration, Maximum Life, All Resistance, Specific Resistances |
| Resourceful | Lucky Hit Chance restores Resource, Maximum Resource, Resource Cost Reduction, Resource on Kill, Resource Regeneration |
| Adept's | Mainstat, Skill Ranks |
| Chromatic | All Resistance, Specific Resistances |

Normal affix rules still apply: you cannot add a second Maximum Life to an item that
already has it, and some affixes are mutually exclusive.

### WARNING - which side the Prism names on Chaotic Reroll is UNRESOLVED

Two readings exist and they give opposite advice. Do not spend materials on a good item
until this is settled in-game.

- **(a) Prism names the VICTIM.** It selects which existing affix gets destroyed; the
  replacement is a random affix from some other Category. Maxroll's wording supports this:
  "Use a Tuning Prism to determine the Category of the random chosen Affix."
- **(b) Prism names the REPLACEMENT.** It selects the Category you receive; the victim is
  drawn from affixes outside that Category. rpgstash's wording supports this: "Rerolls a
  random affix to a different category. You can influence what that category is."

Field observations 2026-07-28 conflict. A Protector's roll produced Life on Hit, which fits
(b). A later Aggressive roll on a ring destroyed Strength and Critical Strike Chance (both
Aggressive) and returned Fury On Kill and Lucky Hit Chance (neither Aggressive), which fits
(a) and refutes (b).

**Resolve it with a controlled test, not by re-reading guide prose.** Take a junk
Legendary, write down every affix and its Category, run ONE Chaotic Reroll with ONE prism,
and record which side of the swap matched the prism. One test costs a single Refined Dust,
15 Raw Dust and one Prism, which is far cheaper than being wrong on a geared item.

**Transfiguration prisms:** Entropic, Kullean (see Transfigure Item below).

## Recipes

### Affix Modification

| Recipe | Effect | Cost |
|---|---|---|
| **Add Affix** | Adds a random Affix to an item. Prism narrows the category. | 1x Common/Magic/Rare/Legendary + 1x Coarse Primordial Dust + 5x Raw + optional Prism |
| **Chaotic Reroll** | Changes a random affix to one of **another** Category. **Which side the Prism names is DISPUTED - see the warning below.** | 1x Magic/Rare/Legendary + 1x Refined + 15x Raw + optional Prism |
| **Focused Reroll** | Changes an affix to one of the **same** Category. **Prism required.** | 1x Magic/Rare/Legendary + 1x Refined + 15x Raw + Prism |
| **Remove Affix** | Removes a random Affix. Prism narrows the category. **Magic or Rare only.** | 1x Magic/Rare + 1x Refined + 15x Raw + optional Prism |

### Transfigure Item

1x Legendary, Unique, or Mythic Unique + 1x **Volatile** Primordial Dust + optional Prism.

Behaves like Season 11 Sanctification: the item gains an extra random affix or
modification and becomes **unmodifiable** - no further alteration except swapping gems
or runes. There is a small chance (community estimate **~1/16**) it stays modifiable,
letting you stack another Transfiguration.

> **Make sure to add sockets, masterwork, and apply the correct aspect BEFORE using
> Transfiguration.** This is the single most common way to brick a finished item.

Outcome distribution without a Prism (Maxroll's sample: 540 Barbarian Transfigurations):

| Outcome | Effect | ~Chance |
|---|---|---|
| Bonus Transfiguration Affix | Extra affix from the Transfiguration pool. Gear-slot rules apply. | ~35% |
| Indestructible | Item no longer loses durability. | ~20% |
| Bonus Item Quality | +1-15 extra Quality, stacking with Masterworking Quality. **2H weapons gain 2-30.** | ~20% |
| Upgrade to Greater Affix | Upgrades a non-Greater affix to a Greater Affix. | ~15% |
| Replace affix with Transfiguration Affix | As Bonus, but replaces an affix. Cannot hit Enchanted affixes, Tempers, or Mythic Uniques. | ~10% |

Transfiguration affix pool (values are for **non-2H** weapons; **2H weapons roll double**
these ranges):

| Affix | Range |
|---|---|
| All Stats [+] | 75-100 |
| Attack Speed [+] | 8-10% |
| Cooldown Reduction | 10-12% |
| Critical Strike Chance [+] | 3.5-5% |
| Elemental specific Damage [x] | 8-10% |
| Gem Strength for an item [x] | 75-100% |
| Life on Hit | 263-316 |
| Lucky Hit Chance | 6-8% |
| Max Life% [+] | 6-8% |
| Max Resource [+] | 15-20 |
| Movement Speed [+] | 20-30 |
| Primary Stat [+] | 150-180 |
| Primary Stat% [+] | 3.5-5% |
| Resource Cost Reduction | 6-8% |
| Total Armor% [+] | 8-10% |
| Total Resistance% [+] | 8-10% |

Affix categories are slot-gated: Movement Speed only on Boots and Amulets, Life on Hit
only on Offensive and Jewelry slots, and so on.

Transfiguration skill ranks default to **+2-3 ranks** to a skill tag, except **Ultimate
ranks on 2H weapons, which are +4-6**. Barbarian slot-to-tag mapping:

| Slot | Barbarian tags |
|---|---|
| Head | Defensive, Wpn Mastery |
| Chest | Brawling, Defensive |
| Gloves | Core, Ultimate |
| Pants | Basic, Defensive |
| Boots | Brawling, Defensive |
| Amulet | All Skills, Core |
| Rings | All Skills, Wpn Mastery |
| 2H Weapon | Ultimate |
| 1H Weapon | Basic |

**Prism behaviour on Transfiguration:**

- **Kullean** imprints a random **Utility Aspect on a non-unmodifiable Amulet**,
  rerollable as often as you like. Always do this *before* the regular Transfiguration.
- **Entropic** guarantees a useful outcome but removes the high-value ones. It strips
  Indestructible, Replace Affix, and Greater Affix from the table and always makes the
  item unmodifiable. Result: ~60% Bonus Transfiguration Affix, ~40% Bonus Item Quality,
  and the affix pool shrinks to All Stats, Life on Hit, Max Life%, Movement Speed,
  Primary Stat, Total Armor%, Total Resistance%.

### Unique Power Reroll

Randomizes the value of the Unique Power on an Ancestral Unique.
Cost: 1x Ancestral Unique + 1x **Attuned** Primordial Dust + 100x Raw.

### 3 to 1 Transmutation

Transmutes **3 of the same equipment, Talisman, or Rune** into a random new item of that
type. Equipment and Charms may mix ancestral and non-ancestral, but using ancestral
items **guarantees an ancestral outcome**.
Cost: 3x items of the same type.

### Recycle Uniques

Transmutes **3 of the same Unique equipment item or Charm** into a new item of that type.
Cost: 3x same Unique / Mythic Unique / Unique Charm.

### Upgrade to Unique

Transmutes a **Common** item into a random Unique of the same type. An Ancestral Common
produces an Ancestral Unique.
Cost: 1x Common + 1x **Enhanced** Primordial Dust + 10x Raw.

### Upgrade to Legendary

Transmutes a **Rare** into a Legendary with a random Legendary power. A Tuning Prism can
modify the Category of the Legendary power added.
Cost: 1x Rare + 1x **Pure** Primordial Dust + 10x Raw + optional Prism.

### Upgrade to Mythic *(Seasonal - S14)*

> "Transmutes a Unique equipment item with at least 850 Item Power into a random Mythic
> of the same item slot."
>
> "**NOTE**: This completely randomizes the item gained, meaning it does not retain
> Greater Affixes, Affixes, nor is it guaranteed to be the same item."

Cost: **1x Unique item (850+ IP) + 5x Pandemonium Fragment.**

Practical consequences, and these are the ones that get misremembered:

- **Only the slot matters.** The identity, affixes, and quality of the input are
  discarded. Feed the worst 850+ Unique you own for the target slot.
- Feeding a specific Unique **does not** raise the chance of getting that Unique's
  Mythic form.
- ~~You may only equip ONE Mythic Unique that you crafted.~~ **OBSOLETE.** The
  crafted-Mythic equip limit was removed in patch 3.1.1a ("removing equip limits for
  Mythic Uniques", Blizzard blue post - verbatim quote in `itemization.md`). Verified
  in the field 2026-07-28 with three `Crafted` Mythics equipped at once. Many guides
  still repeat the old limit; they are stale. Craft and equip freely.

### Reroll Set Charm

Transmutes a Set Charm into a different Charm from the same set, with a **~4% chance per
affix** to produce a Greater Affix. This is the intended way to farm GAs on charms.
Cost: 1x Set Charm + 25x Raw Primordial Dust + 50x Infused Horadric Resin.

### Craft Unique Charm

Transmutes an Ancestral Unique **equipment** item into a Unique **Charm**. Some Uniques
are excluded.
Cost: 1x Ancestral Unique, 3x any Unique Charms, 1x Enhanced Primordial Dust,
50x Raw Primordial Dust, 100x Infused Horadric Resin.

Eligible **shared** Uniques: Azurewrath, Banished Lord's Talisman, Blood-Mad Idol, Crown
of Lucion, Endurant Faith, Fists of Fate, Flickerstep, Frostburn, Godslayer Crown,
Locran's Talisman, Mother's Embrace, Paingorger's Gauntlets, Penitent Greaves,
Rakanoth's Wake, Razorplate, Rustbitten Dirk, Soulbrand, Tassets of the Dawning Sky,
Temerity, Tibault's Will, The Butcher's Cleaver, Thousand-Eye Reaver, Wendigo Brand,
Wyrdskin, X'Fal's Corroded Signet, Yen's Blessing.

Eligible **Barbarian** Uniques: Arreat's Bearing, Chainscourged Mail, Dark Stalker's
Medallion, Emblem of Staalbreak, Hooves of the Mountain God, Nomad's Longing Heart,
Rage of Harrogath, Ramaladni's Magnum Opus, Ring of Red Furor, Sabre of Tsasgal,
The Open Eye of Gorgorra, X'Fal's Corroded Signet, Yen's Blessing.

(Other class lists exist on the source page; pull them live if a non-Barbarian question
comes up.)

### Amalgamation (5 to 1)

Transmutes 5 of a certain item into a single better one. Also the **only** way to
upgrade Grand gems into Horadric and Flawless Horadric gems.

| Input | Output |
|---|---|
| 5x any Nightmare Sigils | 1x Escalation Sigil |
| 5x any Magic Runes | 1x random Rare Rune |
| 5x any Rare Runes | 1x random Legendary Rune |
| 5x same Magic Tributes | 1x Rare Tribute of the same type |
| 5x same Rare Tributes | 1x Legendary Tribute of the same type |
| 5x Greater Tribute of Armaments | 1x Mythic Tribute of Armaments |
| 5x same Boss Trophy | 1x random Unique from that Boss |
| 5x same Grand Gem | 1x Horadric Gem of the same type |
| 5x same Horadric Gem | 1x Flawless Horadric Gem of the same type |

### Rune Crafting

Every Legendary Rune has a recipe, all of the form
**1x specific Rare Rune + 5x any Rare Runes + 5x any Legendary Runes**:

| Craft | Seed rune |
|---|---|
| Bac | Prid |
| Igni | Teb |
| Tam | Ner |
| Yul | Ur |
| Eom | Tzic |
| Jah | Cem |
| Ohm | Qua |
| Vex | Gar |
| Yom | Kry |

## Tips and tricks (from the source)

- Use **3-to-1** to collect items that cannot be target-farmed, e.g. Tibault's Will.
- Use 3-to-1 on items bricked while chasing big Transfiguration outcomes, so the bricks
  are not wasted.
- Farming All Resistance: add resistance **twice**, then Remove Affix - a 50/50 chance
  of keeping the All Resistance roll.
- **Reroll Set Charm** is how you get Greater Affixes on Charms.
- Spread utility stats across multiple pieces. Rolling one stat at a time per item is
  far more material-efficient than trying to force several onto one piece.
- **Enchanting can lock an affix** against removal by the Cube, including preserving a
  Greater Affix while you reroll another stat in the same prism category.

## Checklist to run on every "is this an upgrade?" question

1. Is the item a **crafting input** rather than an upgrade? (850+ Unique = Mythic fodder
   for its slot; Common = Upgrade to Unique; Rare = Upgrade to Legendary; 3 duplicates =
   3-to-1 / Recycle Uniques; 5 of a thing = Amalgamation.)
2. Does the player already own the target slot's Mythic **as a drop**? If yes, the
   crafted-Mythic slot is still free.
3. Is a wanted item on the **Craft Unique Charm** list? That converts a dead equipment
   Unique into a charm.
4. Is the item finished (sockets, masterwork, aspect) and therefore ready to
   **Transfigure**? Or is Transfiguring now going to brick it?
5. Is the gap a missing **affix** rather than a missing item? Add Affix / Focused Reroll
   with the right prism may close it more cheaply than a new drop.
6. State plainly that stat-matching is not a DPS simulation (see `traps.md` #4).
