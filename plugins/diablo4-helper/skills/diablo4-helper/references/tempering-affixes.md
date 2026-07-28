# Temperable affixes (generated)

**Do not hand-edit.** Regenerate with
`uv run python3 plugins/diablo4-helper/scripts/gen_temper_reference.py`.

`[Certain]` Every row is an affix whose `IsTemperingAvailable` flag is set in
`D4Companion/Data/Affixes.enUS.json`, which D4Companion builds from the extracted client
data in [DiabloTools/d4data](https://github.com/DiabloTools/d4data) (MIT).
This is game data, not scraped guide text - if a guide disagrees about
whether an affix can be tempered, this file wins.

377 temperable affixes out of 893 total.

## What this file does and does not answer

- **Answers:** whether an affix can appear as a tempering option at all, and
  which classes can roll it.
- **Does not answer:** which tempering *manual* grants it, which item slots
  accept it, or its value range. The `#` in each row is the game's own
  placeholder - the source data carries no numeric ranges.
- **Does not answer:** Tuning Prism category. See the header comment in the
  generator for why the `Category` field cannot be trusted for that.

Remember the budget from `SKILL.md`: an item carries exactly **one**
tempering affix. `Tempers: X/Y` counts reroll attempts, not slots.

A `Class mask empty in the data - verify in game` section holds affixes whose class mask is all zeroes.
`[Unverified]` **Do not read that as either universal or unobtainable.**
D4Companion's own UI (`AffixViewModel.cs`) buckets an all-zero mask with
all-one and shows it to everyone, but the internal id of at least one such
affix names a specific class and tier, which contradicts that. The id is
printed alongside each row so the contradiction is visible rather than
hidden behind a verdict. Confirm in game before acting on one.

The stat key is the affix's internal `LocalisationId`. Use it to correlate
with diablo.trade attribute ids and with build-planner exports, which name
stats inconsistently in prose but agree on these keys.

## All classes (55)

| Affix | Stat key |
|---|---|
| #% Block Chance | `Block_Chance` |
| #% Cooldown Reduction | `Power_Cooldown_Reduction_Percent_All` |
| #% Dodge Chance | `Dodge_Chance_Bonus` |
| #% Evade Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Impairment Reduction | `CC_Duration_Reduction` |
| #% Mobility Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Resource Cost Reduction | `Resource_Cost_Reduction_Percent_All` |
| #% Resource Generation | `Resource_Gain_And_Regen_Bonus_Percent_All_Primary` |
| #% Ultimate Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| +# Armor | `Armor_Bonus` |
| +# Cold Resistance | `Resistance#Cold_Gem` |
| +# Fire Resistance | `Resistance#Fire_Gem` |
| +# Life On Hit | `Flat_Hitpoints_On_Hit_Unscaled_By_Player_Health` |
| +# Lightning Resistance | `Resistance#Lightning_Gem` |
| +# Maximum Life | `Flat_Hitpoints_Max_Bonus` |
| +# Physical Resistance | `Resistance#Physical_Gem` |
| +# Poison Resistance | `Resistance#Poison_Gem` |
| +# Resistance to All Elements | `Resistance_All` |
| +# Shadow Resistance | `Resistance#Shadow_Gem` |
| +# Thorns | `Thorns_Flat` |
| +#% Attack Speed | `Attack_Speed_Percent_Bonus` |
| +#% Barrier Generation | `Barrier_Bonus_Percent` |
| +#% Basic Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Core Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Critical Strike Chance | `Crit_Percent_Bonus` |
| +#% Critical Strike Damage | `Crit_Damage_Percent` |
| +#% Crowd Control Duration | `CC_Duration_Bonus_Percent` |
| +#% Damage | `Damage_Percent_All_From_Skills` |
| +#% Damage Over Time | `DOT_DPS_Bonus_Percent` |
| +#% Damage Per Overpower Stack | `Overpower_Damage_Bonus_Per_Stack` |
| +#% Damage to Close Enemies | `Damage_Bonus_To_Near` |
| +#% Damage to Crowd Controlled Enemies | `Damage_Percent_Bonus_Vs_CC_All` |
| +#% Damage to Injured Enemies | `Damage_Bonus_To_Low_Health` |
| +#% Damage to Slowed Enemies | `Damage_Percent_Bonus_Vs_CC_Target` |
| +#% Damage when Spending Resolve | `Paladin_Juggernaut_Damage_When_Spending_Resolve` |
| +#% Holy Damage | `Damage_Type_Percent_Bonus` |
| +#% Lucky Hit Chance | `Combat_Effect_Chance_Bonus` |
| +#% Movement Speed | `Movement_Bonus_Run_Speed` |
| +#% Movement Speed for # Seconds After Killing an Elite | `Movement_Speed_Bonus_On_Elite_Kill`, `Movement_Speed_Bonus_On_Elite_Kill` |
| +#% Ultimate Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Vulnerable Damage | `Vulnerable_Health_Damage_Bonus` |
| Casting Ultimate Skills Restores +# Primary Resource | `Primary_Resource_On_Cast_Per_Skill_Tag` |
| Lucky Hit: Up to a +#% Chance to Daze for 2 Seconds | `On_Hit_CC_Proc_Chance` |
| Lucky Hit: Up to a +#% Chance to Freeze for 2 Seconds | `On_Hit_CC_Proc_Chance` |
| Lucky Hit: Up to a +#% Chance to Immobilize for 2 Seconds | `On_Hit_CC_Proc_Chance` |
| Lucky Hit: Up to a +#% Chance to Make Enemies Vulnerable for # Seconds | `On_Hit_Vulnerable_Proc`, `On_Hit_Vulnerable_Proc` |
| Lucky Hit: Up to a +#% Chance to Slow for 2 Seconds | `On_Hit_CC_Proc_Chance` |
| Lucky Hit: Up to a +#% Chance to Stun for 2 Seconds | `On_Hit_CC_Proc_Chance` |
| Lucky Hit: Up to a 15% Chance to Restore +#% Primary Resource | `Proc_Resource_On_Hit_Percent_All_Primary` |
| Lucky Hit: Up to a 40% Chance to Deal +# Cold Damage | `Proc_Flat_Element_Damage_On_Hit` |
| Lucky Hit: Up to a 40% Chance to Deal +# Fire Damage | `Proc_Flat_Element_Damage_On_Hit` |
| Lucky Hit: Up to a 40% Chance to Deal +# Lightning Damage | `Proc_Flat_Element_Damage_On_Hit` |
| Lucky Hit: Up to a 40% Chance to Deal +# Physical Damage | `Proc_Flat_Element_Damage_On_Hit` |
| Lucky Hit: Up to a 40% Chance to Deal +# Poison Damage | `Proc_Flat_Element_Damage_On_Hit` |
| Lucky Hit: Up to a 40% Chance to Deal +# Shadow Damage | `Proc_Flat_Element_Damage_On_Hit` |

## Sorcerer (35)

| Affix | Stat key |
|---|---|
| #% Deep Freeze Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Frost Nova Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Hydra Resource Cost Reduction | `Power_Resource_Cost_Reduction_Percent` |
| #% Ice Blades Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Inferno Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Lightning Spear Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Teleport Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Unstable Currents Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| +#% Blizzard Damage | `Power_Damage_Percent_Bonus` |
| +#% Burning Damage | `DOT_DPS_Bonus_Percent_Per_Damage_Type#Fire` |
| +#% Chill Slow Potency | `Chill_Progressive_Bonus_Slow_Percent` |
| +#% Conjuration Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Crackling Energy Damage | `Power_Damage_Percent_Bonus` |
| +#% Enchantment Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Familiar Damage | `Power_Damage_Percent_Bonus` |
| +#% Familiar Lucky Hit Chance | `Combat_Effect_Chance_Bonus_Per_Skill` |
| +#% Fire Damage | `Damage_Type_Percent_Bonus` |
| +#% Frost Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Frost Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Hydra Damage | `Power_Damage_Percent_Bonus` |
| +#% Hydra Lucky Hit Chance | `Combat_Effect_Chance_Bonus_Per_Skill` |
| +#% Ice Blades Damage | `Power_Damage_Percent_Bonus` |
| +#% Ice Blades Lucky Hit Chance | `Combat_Effect_Chance_Bonus_Per_Skill` |
| +#% Ice Spike Damage | `Power_Damage_Percent_Bonus` |
| +#% Immobilize Duration | `CC_Duration_Bonus_Percent_Per_Type` |
| +#% Lightning Spear Damage | `Power_Damage_Percent_Bonus` |
| +#% Lightning Spear Lucky Hit Chance | `Combat_Effect_Chance_Bonus_Per_Skill` |
| +#% Mastery Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Pyromancy Attack Speed | `Attack_Speed_Percent_Bonus_Per_Skill_Tag` |
| +#% Pyromancy Critical Strike Damage | `Crit_Damage_Percent_Per_Skill_Tag` |
| +#% Pyromancy Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Shock Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Shock Critical Strike Damage | `Crit_Damage_Percent_Per_Skill_Tag` |
| +#% Shock Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Teleport Damage | `Power_Damage_Percent_Bonus` |

## Druid (43)

| Affix | Stat key |
|---|---|
| #% Blood Howl Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Boulder Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Cataclysm Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Companion Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Cyclone Armor Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Debilitating Roar Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Earthen Bulwark Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Grizzly Rage Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Hurricane Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Lacerate Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Petrify Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Poison Creeper Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Rabies Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Ravens Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Trample Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Wolves Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| +#% Boulder Damage | `Power_Damage_Percent_Bonus` |
| +#% Cataclysm Damage | `Power_Damage_Percent_Bonus` |
| +#% Companion Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Cyclone Armor Damage | `Power_Damage_Percent_Bonus` |
| +#% Earth Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Earth Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Earth Lucky Hit Chance | `Hit_Effect_Chance_Bonus_Per_Skill_Tag` |
| +#% Human Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Hurricane Damage | `Power_Damage_Percent_Bonus` |
| +#% Lacerate Damage | `Power_Damage_Percent_Bonus` |
| +#% Lightning Bolt Damage | `Power_Damage_Percent_Bonus` |
| +#% Poison Creeper Damage | `Power_Damage_Percent_Bonus` |
| +#% Rabies Damage | `Power_Damage_Percent_Bonus` |
| +#% Ravens Attack Speed | `Attack_Speed_Percent_Bonus_For_Power` |
| +#% Ravens Damage | `Power_Damage_Percent_Bonus` |
| +#% Shred Critical Strike Chance | `Power_Crit_Percent_Bonus` |
| +#% Storm Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Storm Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Trample Damage | `Power_Damage_Percent_Bonus` |
| +#% Versatile Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Werebear Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Werewolf Attack Speed | `Attack_Speed_Percent_Bonus_Per_Skill_Tag` |
| +#% Werewolf Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Werewolf Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Wolves Attack Speed | `Attack_Speed_Percent_Bonus_For_Power` |
| +#% Wolves Damage | `Power_Damage_Percent_Bonus` |
| Casting Wrath Skills Restores +# Primary Resource | `Primary_Resource_On_Cast_Per_Skill_Tag` |

## Barbarian (43)

| Affix | Stat key |
|---|---|
| #% Brawling Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Call of the Ancients Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Challenging Shout Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Charge Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Death Blow Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Ground Stomp Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Iron Maelstrom Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Iron Skin Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Kick Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Leap Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Rupture Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Steel Grasp Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% War Cry Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Weapon Mastery Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Wrath of the Berserker Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| +#% Ancient Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Berserking Duration | `Power_Duration_Bonus_Pct` |
| +#% Bleeding Damage | `DOT_DPS_Bonus_Percent_Per_Damage_Type#Physical` |
| +#% Brawling Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Charge Damage | `Power_Damage_Percent_Bonus` |
| +#% Damage to Bleeding Enemies | `Damage_Percent_Bonus_Against_Dot_Type` |
| +#% Damage while Berserking | `Damage_Percent_Bonus_While_Affected_By_Power#Barbarian_Proc_Berserk` |
| +#% Damage while Iron Maelstrom is Active | `Damage_Percent_Bonus_While_Affected_By_Power` |
| +#% Damage while War Cry is Active | `Damage_Percent_Bonus_While_Affected_By_Power` |
| +#% Damage while Wrath of the Berserker is Active | `Damage_Percent_Bonus_While_Affected_By_Power` |
| +#% Damage with Two-Handed Bludgeoning Weapons | `Damage_Percent_Bonus_Per_Weapon_Requirement` |
| +#% Damage with Two-Handed Slashing Weapons | `Damage_Percent_Bonus_Per_Weapon_Requirement` |
| +#% Death Blow Damage | `Power_Damage_Percent_Bonus` |
| +#% Dust Devil Damage | `Power_Damage_Percent_Bonus` |
| +#% Earthquake Damage | `Power_Damage_Percent_Bonus` |
| +#% Ground Stomp Damage | `Power_Damage_Percent_Bonus` |
| +#% Kick Damage | `Power_Damage_Percent_Bonus` |
| +#% Leap Damage | `Power_Damage_Percent_Bonus` |
| +#% Lunging Strike Healing | `Percent_Bonus_Projectiles_Per_Power#Barbarian_LungingStrike` |
| +#% Resource Generation with Dual-Wielded Weapons | `Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement` |
| +#% Resource Generation with Polearms | `Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement` |
| +#% Resource Generation with Two-Handed Bludgeoning Weapons | `Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement` |
| +#% Resource Generation with Two-Handed Slashing Weapons | `Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement` |
| +#% Rupture Damage | `Power_Damage_Percent_Bonus` |
| +#% Steel Grasp Damage | `Power_Damage_Percent_Bonus` |
| +#% Weapon Mastery Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| Steel Grasp Stuns for +# Seconds | `Bonus_Percent_Per_Power#Barbarian_SteelGrasp` |
| Upheaval Overpowers Stun for +# Seconds | `Bonus_Percent_Per_Power#Barbarian_Upheaval` |

## Rogue (41)

| Affix | Stat key |
|---|---|
| #% Agility Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Caltrops Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Concealment Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Dark Shroud Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Dash Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Death Trap Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Imbuement Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Imbuement Potency | `Rogue_Imbuement_Potency` |
| #% Poison Trap Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Puncture Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Rain of Arrows Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Shadow Clone Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Shadow Step Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Smoke Grenade Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Subterfuge Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Trap Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| +#% Agility Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Cutthroat Attack Speed | `Attack_Speed_Percent_Bonus_Per_Skill_Tag` |
| +#% Cutthroat Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Cutthroat Critical Strike Damage | `Crit_Damage_Percent_Per_Skill_Tag` |
| +#% Cutthroat Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Damage on Next Attack After Entering Stealth | `Rogue_Special_DamageAfterStealth` |
| +#% Damage per Combo Point Spent | `Damage_Bonus_Percent_Per_Combo_Point` |
| +#% Damage to Trapped Enemies | `Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag#Skill_Trap` |
| +#% Dash Damage | `Power_Damage_Percent_Bonus` |
| +#% Grenade Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Imbued Damage | `Imbued_Skill_Damage_Percent_Bonus` |
| +#% Inner Sight Duration | `Power_Duration_Bonus_Pct` |
| +#% Invigorating Strike Energy Regeneration | `Bonus_Percent_Per_Power#Rogue_InvigoratingStrike` |
| +#% Marksman Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Marksman Critical Strike Damage | `Crit_Damage_Percent_Per_Skill_Tag` |
| +#% Marksman Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Rain of Arrows Damage | `Power_Damage_Percent_Bonus` |
| +#% Shade Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Shadow Clone Damage | `Power_Damage_Percent_Bonus` |
| +#% Shadow Step Damage | `Power_Damage_Percent_Bonus` |
| +#% Smoke Grenade Damage | `Power_Damage_Percent_Bonus` |
| +#% Stun Grenade Damage | `Power_Damage_Percent_Bonus` |
| +#% Trap Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| Traps Arm # Seconds Faster | `Trap_Arm_Time_Reduction_Seconds` |
| Twisting Blades Returns +#% Faster | `Bonus_Percent_Per_Power#Rogue_TwistingBlades` |

## Necromancer (38)

| Affix | Stat key |
|---|---|
| # All Stats | `Plus_All_Stats` |
| #% Blood Mist Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Bone Spirit Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Damage Reduction for Your Summons | `Pet_Damage_Reduction_Percent` |
| #% Golem Active Cooldown Reduction | `Power_Cooldown_Reduction_Percent#Necromancer_Golem` |
| +#% Blight Chill Potency | `Bonus_Percent_Per_Power#Necromancer_Blight` |
| +#% Blood Attack Speed | `Attack_Speed_Percent_Bonus_Per_Skill_Tag` |
| +#% Blood Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Blood Orb Healing | `Blood_Orb_Pickup_Healing_Percent_Bonus` |
| +#% Bone Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Bone Critical Strike Damage | `Crit_Damage_Percent_Per_Skill_Tag` |
| +#% Bone Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Bone Spirit Damage | `Power_Damage_Percent_Bonus` |
| +#% Chance For Minion Attacks to Fortify You for 3% Maximum Life | `Minions_Fortify_On_Attack_Chance` |
| +#% Corpse Explosion Damage | `Power_Damage_Percent_Bonus` |
| +#% Corpse Tendrils Damage | `Power_Damage_Percent_Bonus` |
| +#% Corrupting Damage | `DOT_DPS_Bonus_Percent_Per_Damage_Type#Shadow` |
| +#% Curse Duration | `Per_Skill_Tag_Buff_Duration_Bonus_Percent` |
| +#% Damage for # Seconds After Picking Up a Blood Orb | `Blood_Orb_Pickup_Damage_Combined`, `Blood_Orb_Pickup_Damage_Combined` |
| +#% Damage to Cursed Enemies | `Damage_Percent_Bonus_To_Targets_Affected_By_Skill_Tag#Skill_Primary_Curse` |
| +#% Darkness Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Desecrated Ground Damage | `Power_Damage_Percent_Bonus` |
| +#% Golem Damage | `Power_Damage_Percent_Bonus` |
| +#% Iron Maiden Damage | `Power_Damage_Percent_Bonus` |
| +#% Macabre Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Resource Generation while Wielding a Scythe | `Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement#Scythe` |
| +#% Resource Generation while Wielding a Shield | `Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement#Shield` |
| +#% Resource Generation with Two-Handed Weapons | `Primary_Resource_Gain_Bonus_Percent_Per_Weapon_Requirement` |
| +#% Skeleton Mage Damage | `Power_Damage_Percent_Bonus` |
| +#% Summon Attack Speed | `Pet_Attack_Speed_Bonus_Percent` |
| +#% Summon Damage | `Damage_Percent_Bonus_Per_Skill_Tag#Skill_Primary_Summoning` |
| +#% Thorns while Fortified | `Thorns_Percent_Bonus_While_Fortified` |
| Blood Orbs Restore +# Essence | `Percent_Bonus_Projectiles_Per_Power#Necromancer_BloodOrb_Pickup` |
| Casting Macabre Skills Restores +# Primary Resource | `Primary_Resource_On_Cast_Per_Skill_Tag` |
| Golems Inherit +#% of Your Thorns | `NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct` |
| Minions Inherit +#% of Your Thorns  | `NecroArmy_All_Pet_Types_Inherit_Thorns_Bonus_Pct` |
| Skeletal Mages Inherit +#% of Your Thorns | `NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct` |
| Skeletal Warriors Inherit +#% of Your Thorns | `NecroArmy_Pet_Type_Inherit_Thorns_Bonus_Pct` |

## Spiritborn (50)

| Affix | Stat key |
|---|---|
| #% Defensive Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Focus Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Incarnate Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Potency Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Rock Splitter Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Soar Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% The Devourer Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% The Hunter Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% The Protector Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% The Seeker Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Thrash Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Thunderspike Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Withering Fist Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| +# Counterattack Charges | `Bonus_Max_Skill_Charges_For_Power` |
| +# Maximum Resolve Stacks | `MaxStacks#Spiritborn_Gorilla_Passive` |
| +# Razor Wings Charges | `Bonus_Max_Skill_Charges_For_Power` |
| +# Rushing Claw Charges | `Bonus_Max_Skill_Charges_For_Power` |
| +# The Seeker Charges | `Bonus_Max_Skill_Charges_For_Power` |
| +#% Centipede Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Chance for Concussive Stomp to Extra Hit | `Bonus_Percent_Per_Power#Spiritborn_Gorilla_Defensive2` |
| +#% Chance for Payback to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Gorilla_Potency` |
| +#% Chance for Rock Splitter to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Gorilla_Basic` |
| +#% Chance for Rushing Claw to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Jaguar_Potency` |
| +#% Chance for Soar to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Eagle_Focus2` |
| +#% Chance for The Devourer to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Centipede_Ultimate` |
| +#% Chance for The Hunter to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Jaguar_Ultimate` |
| +#% Chance for The Protector to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Gorilla_Ultimate` |
| +#% Chance for The Seeker to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Eagle_Ultimate` |
| +#% Chance for Thrash to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Jaguar_Basic` |
| +#% Chance for Thunderspike to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Eagle_Basic` |
| +#% Chance for Vortex to Extra Hit | `Bonus_Percent_Per_Power#Spiritborn_Eagle_Focus` |
| +#% Chance for Withering Fist to Deal Double Damage | `Bonus_Percent_Per_Power#Spiritborn_Centipede_Basic` |
| +#% Defensive Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Eagle Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Ferocity Potency | `Bonus_Percent_Per_Power#Spiritborn_Plains_Passive` |
| +#% Focus Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Gorilla Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Jaguar Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Mobility Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Mystic Circle Potency | `Bonus_Percent_Per_Power#Spiritborn_RuneArea` |
| +#% Pestilent Swarm Damage | `Power_Damage_Percent_Bonus` |
| +#% Poisoning Damage | `DOT_DPS_Bonus_Percent_Per_Damage_Type#Poison` |
| +#% Potency Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Primary Centipede Spirit Hall Damage | `Spiritborn_Spirit_Bonus#spiritborn_centipede_sun_passive` |
| +#% Primary Eagle Spirit Hall Damage | `Spiritborn_Spirit_Bonus#spiritborn_eagle_sun_passive_alternate` |
| +#% Primary Gorilla Spirit Hall Damage | `Spiritborn_Spirit_Bonus#spiritborn_gorilla_sun_passive` |
| +#% Primary Jaguar Spirit Hall Damage | `Spiritborn_Spirit_Bonus#spiritborn_jaguar_sun_passive` |
| +#% Ravager On Kill Duration Extension | `Bonus_Percent_Per_Power#Spiritborn_Jaguar_Focus` |
| +#% Scourge Poisoning Duration | `Bonus_Percent_Per_Power#Spiritborn_Centipede_Defensive` |
| +#% Storm Feather Potency | `Bonus_Percent_Per_Power#Spiritborn_Feather_Spawn` |

## Paladin (44)

| Affix | Stat key |
|---|---|
| #% Advance Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Aegis Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Arbiter of Justice Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Aura Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Brandish Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Clash Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Condemn Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Consecration Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Falling Star Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Fortress Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Heaven's Fury Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Holy Bolt Resource Generation | `Resource_Gain_Bonus_Percent_Per_Power` |
| #% Justice Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Purify Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Shield Charge Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| #% Valor Cooldown Reduction | `Skill_Tag_Cooldown_Reduction_Percent` |
| #% Zenith Cooldown Reduction | `Power_Cooldown_Reduction_Percent` |
| +# Resolve Generated | `Bonus_Percent_Per_Power#Spiritborn_Gorilla_Passive` |
| +#% Arbiter Duration | `Power_Duration_Bonus_Pct` |
| +#% Armor in Arbiter Form | `Paladin_Arbiter_AdditionalArmor` |
| +#% Aura Enhancement Potency | `Paladin_Aura_Enhancement_Potency` |
| +#% Aura Potency | `Paladin_Aura_Potency` |
| +#% Chance for Arbiter to Deal Double Damage | `Chance_For_Double_Damage_Per_Power` |
| +#% Chance for Clash to Deal Double Damage | `Chance_For_Double_Damage_Per_Power` |
| +#% Chance for Judgement to Deal Double Damage | `Chance_For_Double_Damage_Per_Power` |
| +#% Chance for Retribution to Deal Double Damage | `Chance_For_Double_Damage_Per_Power` |
| +#% Chance for Shield Bash to Deal Double Damage | `Chance_For_Double_Damage_Per_Power` |
| +#% Chance for Shield Charge to Deal Double Damage | `Chance_For_Double_Damage_Per_Power` |
| +#% Damage to Judged Enemies | `Paladin_Judgement_DamageTakenWhileJudged` |
| +#% Damage to Weakened Enemies | `Damage_Bonus_Percent_To_Weakened` |
| +#% Damage while in Arbiter Form | `Damage_Percent_Bonus_While_Affected_By_Power#Paladin_Sub_Angel` |
| +#% Defiance Aura Potency | `Paladin_Aura_Potency_Per_Skill` |
| +#% Disciple Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Fanaticism Aura Potency | `Paladin_Aura_Potency_Per_Skill` |
| +#% Holy Light Aura Potency | `Paladin_Aura_Potency_Per_Skill` |
| +#% Judicator Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Juggernaut Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Justice Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Wing Strike Damage | `Paladin_Arbiter_WingStrike_Damage` |
| +#% Zealot Critical Strike Chance | `Crit_Percent_Bonus_Per_Skill_Tag` |
| +#% Zealot Critical Strike Damage | `Crit_Damage_Percent_Per_Skill_Tag` |
| +#% Zealot Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| Casting Justice Skills Restores +# Primary Resource | `Primary_Resource_On_Cast_Per_Skill_Tag` |
| Casting Valor Skills Restores +# Primary Resource | `Primary_Resource_On_Cast_Per_Skill_Tag` |

## Warlock (10)

| Affix | Stat key |
|---|---|
| #% Core Resource Cost Reduction | `Skill_Tag_Resource_Cost_Reduction_Percent` |
| +#% Abyss Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Archfiend Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Damage while Shadowform is Active | `Damage_Percent_Bonus_While_Affected_By_Power` |
| +#% Demonform Damage Bonus | `Warlock_Demonform_Damage_Bonus` |
| +#% Demonology Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Hellfire Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Occult Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Sigil Damage | `Damage_Percent_Bonus_Per_Skill_Tag` |
| +#% Sigil Duration | `Custom_Duration_Bonus_Per_Skill_Tag` |

## Class mask empty in the data - verify in game (1)

| Affix | Internal id | Stat key |
|---|---|---|
| +#% Damage to Knocked-Down Enemies | `Damage_to_Knockdown;Tempered_Damage_Spiritborn_Type_KnockDown_Tier1;Tempered_Damage_Spiritborn_Type_KnockDown_Tier2;Tempered_Damage_Spiritborn_Type_KnockDown_Tier3` | `Damage_Percent_Bonus_Vs_CC_Target` |

## Two or more classes (not all) (17)

| Affix | Classes | Stat key |
|---|---|---|
| #% Basic Resource Generation | Barbarian / Necromancer | `Resource_Gain_Bonus_Percent_Per_Skill_Tag` |
| +#% Cold Damage | Sorcerer / Rogue / Necromancer | `Damage_Type_Percent_Bonus` |
| +#% Damage to Distant Enemies | Sorcerer / Druid / Rogue / Necromancer | `Damage_Bonus_To_Far` |
| +#% Damage to Frozen Enemies | Sorcerer / Rogue | `Damage_Percent_Bonus_Vs_CC_Target` |
| +#% Damage to Poisoned Enemies | Druid / Rogue | `Damage_Percent_Bonus_Against_Dot_Type` |
| +#% Damage when Swapping Weapons | Barbarian / Rogue | `Damage_Percent_Bonus_When_Weapon_Swapping` |
| +#% Damage while Fortified | Druid / Necromancer | `Damage_Percent_Bonus_When_Fortified` |
| +#% Damage with Dual-Wielded Weapons | Barbarian / Rogue | `Damage_Percent_Bonus_Per_Weapon_Requirement` |
| +#% Fortify Generation | Druid / Barbarian / Necromancer | `Fortified_Health_Application_Bonus` |
| +#% Freeze Duration | Sorcerer / Rogue | `CC_Duration_Bonus_Percent_Per_Type` |
| +#% Lightning Damage | Sorcerer / Druid | `Damage_Type_Percent_Bonus` |
| +#% Physical Damage | Druid / Barbarian / Rogue / Necromancer | `Damage_Type_Percent_Bonus` |
| +#% Poison Damage | Druid / Rogue | `Damage_Type_Percent_Bonus` |
| +#% Shadow Damage | Rogue / Necromancer | `Damage_Type_Percent_Bonus` |
| +#% Stun Duration | Sorcerer / Barbarian | `CC_Duration_Bonus_Percent_Per_Type` |
| Lucky Hit: Up to a +#% Chance to Weaken Enemies for # Seconds | Sorcerer / Druid / Necromancer / Spiritborn / Paladin / Warlock | `On_Hit_Weakened_Proc`, `On_Hit_Weakened_Proc` |
| Lucky Hit: Up to a +#% Chance to Weaken for 2 Seconds | Barbarian / Rogue | `On_Hit_CC_Proc_Chance` |
