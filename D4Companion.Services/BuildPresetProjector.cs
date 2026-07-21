using System.Windows.Media;
using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;
using D4Companion.Interfaces;

namespace D4Companion.Services
{
    public class BuildPresetProjector : IBuildPresetProjector
    {
        private readonly ISettingsManager _settingsManager;

        public BuildPresetProjector(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public AffixPreset Project(CanonicalVariant variant, string presetName)
        {
            var preset = new AffixPreset { Name = presetName };

            foreach (var item in variant.Items)
            {
                AddAffixes(preset, item);
                AddAspects(preset, item);
                AddUniques(preset, item);
                AddRunes(preset, item);
            }

            SortAffixes(preset);

            if (variant.ParagonBoards.Count > 0)
            {
                preset.ParagonBoardsList.Add(variant.ParagonBoards);
            }

            return preset;
        }

        private void AddAffixes(AffixPreset preset, CanonicalItem item)
        {
            foreach (var affix in item.Affixes)
            {
                bool exists = preset.ItemAffixes.Any(a =>
                    a.Id.Equals(affix.Id) &&
                    a.Type.Equals(item.Slot) &&
                    a.IsImplicit == affix.IsImplicit &&
                    a.IsTempered == affix.IsTempered);
                if (exists) continue;

                preset.ItemAffixes.Add(new ItemAffix
                {
                    Id = affix.Id,
                    Type = item.Slot,
                    Color = ColorFor(affix),
                    IsGreater = affix.IsGreater,
                    IsImplicit = affix.IsImplicit,
                    IsTempered = affix.IsTempered
                });
            }
        }

        private void AddAspects(AffixPreset preset, CanonicalItem item)
        {
            foreach (var aspectId in item.AspectIds)
            {
                // One entry per aspect per slot. Sources that cannot supply provenance
                // set SlotIsKnown false and get a single IsAnyType entry instead. This is
                // the fix for the original defect: the old importer registered every
                // aspect under all ten slots.
                bool exists = preset.ItemAspects.Any(a =>
                    a.Id.Equals(aspectId) && a.Type.Equals(item.Slot));
                if (exists) continue;

                preset.ItemAspects.Add(new ItemAffix
                {
                    Id = aspectId,
                    Type = item.Slot,
                    Color = _settingsManager.Settings.DefaultColorAspects,
                    IsAnyType = !item.SlotIsKnown
                });
            }
        }

        private void AddUniques(AffixPreset preset, CanonicalItem item)
        {
            foreach (var uniqueId in item.UniqueIds)
            {
                if (preset.ItemUniques.Any(u => u.Id.Equals(uniqueId))) continue;

                preset.ItemUniques.Add(new ItemAffix
                {
                    Id = uniqueId,
                    Type = string.Empty,
                    Color = _settingsManager.Settings.DefaultColorUniques
                });
            }
        }

        private void AddRunes(AffixPreset preset, CanonicalItem item)
        {
            foreach (var runeId in item.RuneIds)
            {
                if (preset.ItemRunes.Any(r => r.Id.Equals(runeId))) continue;

                preset.ItemRunes.Add(new ItemAffix
                {
                    Id = runeId,
                    Type = ItemTypeConstants.Rune,
                    Color = _settingsManager.Settings.DefaultColorRunes
                });
            }
        }

        private Color ColorFor(CanonicalAffix affix)
        {
            var settings = _settingsManager.Settings;
            if (affix.IsImplicit) return settings.DefaultColorImplicit;
            // Greater deliberately outranks Tempered: D2Core and Mobalytics, the original
            // importers that supported both flags, both treated Greater as winning over
            // Tempered, and D4Builds had no Greater arm at all. Unifying on Tempered-first
            // would silently change every greater+tempered affix from D2Core and Mobalytics
            // builds away from the user's Greater colour, so Greater is checked first here.
            if (affix.IsGreater) return settings.DefaultColorGreater;
            if (affix.IsTempered) return settings.DefaultColorTempered;
            return settings.DefaultColorNormal;
        }

        private static void SortAffixes(AffixPreset preset)
        {
            // Preserves the ordering the previous importers produced:
            // implicit first, tempered last.
            preset.ItemAffixes.Sort((x, y) =>
            {
                if (x.IsTempered != y.IsTempered) return x.IsTempered ? 1 : -1;
                if (x.IsImplicit != y.IsImplicit) return x.IsImplicit ? -1 : 1;
                return string.Compare(x.Type, y.Type, StringComparison.Ordinal);
            });
        }
    }
}
