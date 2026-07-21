using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;

namespace D4Companion.Services.BuildAdapters
{
    /// <summary>
    /// Maps a scraped D4Builds variant onto the canonical model.
    ///
    /// D4Builds does not associate scraped aspects with a gear slot, so aspects land on
    /// a single provenance-free item with SlotIsKnown = false. The projector turns that
    /// into one IsAnyType entry instead of ten fabricated per-slot entries.
    /// Capturing real slots would require changing the scraper and the on-disk cache
    /// schema in Builds/D4Builds/, invalidating previously downloaded builds.
    /// </summary>
    public class D4BuildsBuildAdapter
    {
        public CanonicalBuild ToCanonical(D4BuildsBuildVariant sourceVariant, string buildName)
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

        private static void AddSlot(CanonicalVariant variant, string slot, List<D4buildsAffix> affixes)
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
