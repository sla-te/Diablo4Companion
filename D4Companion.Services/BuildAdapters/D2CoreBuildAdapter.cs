using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;

namespace D4Companion.Services.BuildAdapters
{
    /// <summary>
    /// Maps a D2Core planner build onto the canonical model. D2Core supplies per-item
    /// slots, so SlotIsKnown is always true.
    /// </summary>
    public class D2CoreBuildAdapter
    {
        public CanonicalBuild ToCanonical(D2CoreBuild build)
        {
            var canonical = new CanonicalBuild { Name = build.Name };

            foreach (var sourceVariant in build.Data.Variants)
            {
                var variant = new CanonicalVariant { Name = sourceVariant.Name };

                foreach (var gearEntry in sourceVariant.Gear)
                {
                    var gear = gearEntry.Value;
                    string? slot = ResolveSlot(gear.ItemType);
                    if (slot == null) continue;

                    var item = new CanonicalItem { Slot = slot, SlotIsKnown = true };

                    // Aspects are keyed off the item that carries them, not accumulated
                    // build-wide. This is the fix for the fan-out defect.
                    if (gear.Type.Equals("legendary"))
                    {
                        string aspectId = gear.Key.Replace("Affix_", string.Empty,
                            StringComparison.OrdinalIgnoreCase);
                        if (!string.IsNullOrEmpty(aspectId)) item.AspectIds.Add(aspectId);
                    }

                    variant.Items.Add(item);
                }

                canonical.Variants.Add(variant);
            }

            return canonical;
        }

        private static string? ResolveSlot(string itemType)
        {
            return itemType switch
            {
                "Helm" => ItemTypeConstants.Helm,
                "ChestArmor" => ItemTypeConstants.Chest,
                "DruidOffhand" or "Focus" or "Shield" => ItemTypeConstants.Offhand,

                // Handedness is confirmed for these classes because D2Core pairs a plain
                // class name with an explicit "2H" variant (Mace/Mace2H, Sword/Sword2H);
                // Polearm is the sole exception (two-handed despite carrying no "2H"
                // marker - confirmed separately, mirrors WeaponTypeResolver). Damage type
                // mirrors WeaponTypeResolver.MaxrollPrefixMap, already verified for these
                // exact classes.
                "Mace2H" => ItemTypeConstants.WeaponBludgeoning,
                "Sword2H" or "Polearm" => ItemTypeConstants.WeaponSlicing,
                "Mace" or "Sword" => ItemTypeConstants.WeaponOneHand,

                // Dagger, Glaive, Quarterstaff, Scythe, Scythe2H, Staff and Wand have no
                // paired "class / class2H" evidence in the D2Core data and no confirming
                // reference elsewhere in this codebase, so their handedness is not verified
                // here. Left as plain Weapon rather than guessed - see
                // followup-c-report.md.
                "Dagger" or "Glaive" or "Quarterstaff" or "Scythe" or "Scythe2H" or "Staff" or "Wand"
                    => ItemTypeConstants.Weapon,
                "Bow" or "Crossbow2H" => ItemTypeConstants.Ranged,
                "Gloves" => ItemTypeConstants.Gloves,
                "Legs" => ItemTypeConstants.Pants,
                "Boots" => ItemTypeConstants.Boots,
                "Ring" => ItemTypeConstants.Ring,
                "Amulet" => ItemTypeConstants.Amulet,
                _ => null
            };
        }
    }
}
