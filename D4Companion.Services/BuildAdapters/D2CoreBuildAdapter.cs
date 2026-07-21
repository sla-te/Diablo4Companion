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
                "Dagger" or "Glaive" or "Mace" or "Mace2H" or "Polearm" or "Quarterstaff"
                    or "Scythe" or "Scythe2H" or "Staff" or "Sword" or "Sword2H" or "Wand"
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
