using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;

namespace D4Companion.Services.BuildAdapters
{
    /// <summary>
    /// Maps a Mobalytics build variant onto the canonical model.
    ///
    /// Mobalytics does not tag aspects with a gear slot, so they land on a single
    /// provenance-free item with SlotIsKnown = false, exactly as for D4Builds.
    /// </summary>
    public class MobalyticsBuildAdapter
    {
        public CanonicalBuild ToCanonical(MobalyticsBuildVariant sourceVariant, string buildName)
        {
            var canonical = new CanonicalBuild { Name = buildName };
            var variant = new CanonicalVariant
            {
                Name = sourceVariant.Name,
                ParagonBoards = sourceVariant.ParagonBoards
            };

            AddSlot(variant, ItemTypeConstants.Helm, sourceVariant.Helm);
            AddSlot(variant, ItemTypeConstants.Chest, sourceVariant.Chest);
            AddSlot(variant, ItemTypeConstants.Gloves, sourceVariant.Gloves);
            AddSlot(variant, ItemTypeConstants.Pants, sourceVariant.Pants);
            AddSlot(variant, ItemTypeConstants.Boots, sourceVariant.Boots);
            AddSlot(variant, ItemTypeConstants.Amulet, sourceVariant.Amulet);
            AddSlot(variant, ItemTypeConstants.Ring, sourceVariant.Ring);
            AddSlot(variant, ItemTypeConstants.Weapon, sourceVariant.Weapon);
            AddSlot(variant, ItemTypeConstants.Ranged, sourceVariant.Ranged);
            AddSlot(variant, ItemTypeConstants.Offhand, sourceVariant.Offhand);

            var unslotted = new CanonicalItem
            {
                Slot = ItemTypeConstants.Weapon,
                SlotIsKnown = false
            };
            unslotted.AspectIds.AddRange(sourceVariant.Aspect);
            unslotted.UniqueIds.AddRange(sourceVariant.Uniques);
            unslotted.RuneIds.AddRange(sourceVariant.Runes);
            variant.Items.Add(unslotted);

            canonical.Variants.Add(variant);
            return canonical;
        }

        private static void AddSlot(CanonicalVariant variant, string slot, List<MobalyticsAffix> affixes)
        {
            if (affixes.Count == 0) return;

            var item = new CanonicalItem { Slot = slot, SlotIsKnown = true };
            foreach (var affix in affixes)
            {
                item.Affixes.Add(new CanonicalAffix
                {
                    Id = affix.AffixText,
                    IsGreater = affix.IsGreater,
                    IsImplicit = affix.IsImplicit,
                    IsTempered = affix.IsTempered
                });
            }
            variant.Items.Add(item);
        }
    }
}
