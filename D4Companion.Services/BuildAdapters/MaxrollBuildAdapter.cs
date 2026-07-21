using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;
using D4Companion.Helpers;

namespace D4Companion.Services.BuildAdapters
{
    /// <summary>
    /// Maps a downloaded Maxroll planner build onto the canonical model.
    /// Acquisition stays in BuildsManagerMaxroll; this type is pure and has no
    /// dependencies, which is what makes it testable without a browser.
    /// </summary>
    public class MaxrollBuildAdapter
    {
        public CanonicalBuild ToCanonical(MaxrollBuild build)
        {
            var canonical = new CanonicalBuild { Name = build.Name };

            foreach (var profile in build.Data.Profiles)
            {
                var variant = new CanonicalVariant { Name = profile.Name };

                foreach (var slotEntry in profile.Items)
                {
                    if (!build.Data.Items.TryGetValue(slotEntry.Value, out var sourceItem)) continue;

                    string? slot = ResolveSlot(slotEntry.Key, sourceItem.Id);
                    if (slot == null) continue;

                    var item = new CanonicalItem { Slot = slot, SlotIsKnown = true };

                    foreach (var aspect in sourceItem.Aspects)
                    {
                        if (aspect.Nid != 0) item.AspectIds.Add(aspect.Nid.ToString());
                    }

                    variant.Items.Add(item);
                }

                canonical.Variants.Add(variant);
            }

            return canonical;
        }

        /// <summary>
        /// Maps a Maxroll equipment-slot id to an item type. Weapon slots defer to the
        /// item id so that Bludgeoning, Slicing and one-handed weapons separate.
        /// Returns null for slots that are deliberately not imported.
        /// </summary>
        private static string? ResolveSlot(int slotId, string itemId)
        {
            switch (slotId)
            {
                case 4: return ItemTypeConstants.Helm;
                case 5: return ItemTypeConstants.Chest;
                case 6: return ItemTypeConstants.Offhand;
                case 7:
                case 8:
                case 9:
                case 11:
                case 12: return WeaponTypeResolver.FromMaxrollItemId(itemId);
                case 10: return ItemTypeConstants.Ranged;
                case 13: return ItemTypeConstants.Gloves;
                case 14: return ItemTypeConstants.Pants;
                case 15: return ItemTypeConstants.Boots;
                case 16:
                case 17: return ItemTypeConstants.Ring;
                case 18: return ItemTypeConstants.Amulet;
                // Slot 20 (HoradricSeal) and 21-26 (Charm) carry no resolvable aspects:
                // BuildsManagerMaxroll skips these item types outright, so emitting a
                // CanonicalItem for them here would only leak raw unresolved Nid strings
                // into the projected preset. Return null so they are never emitted.
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26: return null;
                default: return null;
            }
        }
    }
}
