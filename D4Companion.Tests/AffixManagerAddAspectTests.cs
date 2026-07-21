using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Interfaces;
using D4Companion.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;

namespace D4Companion.Tests
{
    /// <summary>
    /// Covers the manual-add fan-out fix in AffixViewModel.SetAspectExecute: adding an
    /// aspect through the UI must create one IsAnyType entry, not ten per-slot entries.
    /// AffixManager only needs ILogger and ISettingsManager, and both Data\Aspects.*.json
    /// (read unconditionally in the constructor) and Config\ are already copied to the test
    /// output directory, so it can be constructed directly here without heavy mocking.
    /// </summary>
    public class AffixManagerAddAspectTests
    {
        private const string PresetFilePath = "Config/AffixPresets-v2.json";

        [SetUp]
        public void DeletePersistedPreset()
        {
            // AffixManager loads Config/AffixPresets-v2.json unconditionally in its
            // constructor and AddAspect writes it back out via SaveAffixPresets. Without
            // this, a preset saved by one test would be loaded by the next AffixManager
            // instance and silently duplicate "test-preset", corrupting the assertions.
            if (File.Exists(PresetFilePath))
            {
                File.Delete(PresetFilePath);
            }
        }

        private static AffixManager CreateAffixManagerWithPreset(out AffixPreset preset)
        {
            var settingsManager = new FakeSettingsManager();
            settingsManager.Settings.SelectedAffixPreset = "test-preset";

            var affixManager = new AffixManager(NullLogger<AffixManager>.Instance, settingsManager);

            preset = new AffixPreset { Name = "test-preset" };
            affixManager.AddAffixPreset(preset);

            return affixManager;
        }

        [Test]
        public void AddAspect_Default_CreatesSingleSlotEntry()
        {
            var affixManager = CreateAffixManagerWithPreset(out var preset);
            var aspectInfo = new AspectInfo { IdName = "aspect_test" };

            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Boots);

            Assert.That(preset.ItemAspects, Has.Count.EqualTo(1));
            Assert.That(preset.ItemAspects[0].Type, Is.EqualTo(ItemTypeConstants.Boots));
            Assert.That(preset.ItemAspects[0].IsAnyType, Is.False);
        }

        [Test]
        public void AddAspect_IsAnyType_CreatesExactlyOneEntryNotTenPerSlot()
        {
            // This is the regression test for the fan-out bug: SetAspectExecute used to
            // call AddAspect once per equipment slot (ten calls). A manually added aspect
            // has no slot provenance, so it must resolve to a single IsAnyType entry.
            var affixManager = CreateAffixManagerWithPreset(out var preset);
            var aspectInfo = new AspectInfo { IdName = "aspect_test" };

            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Weapon, isAnyType: true);

            Assert.That(preset.ItemAspects, Has.Count.EqualTo(1));
            Assert.That(preset.ItemAspects[0].IsAnyType, Is.True);
        }

        [Test]
        public void AddAspect_IsAnyType_Twice_DoesNotDuplicate()
        {
            var affixManager = CreateAffixManagerWithPreset(out var preset);
            var aspectInfo = new AspectInfo { IdName = "aspect_test" };

            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Weapon, isAnyType: true);
            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Weapon, isAnyType: true);

            Assert.That(preset.ItemAspects, Has.Count.EqualTo(1));
        }

        [Test]
        public void AddAspect_IsAnyType_ResolvesOnEverySlotViaGetAspect()
        {
            // Confirms the new entry actually resolves: GetAspect's IsAnyType fallback
            // must find it regardless of the queried item type.
            var affixManager = CreateAffixManagerWithPreset(out _);
            var aspectInfo = new AspectInfo { IdName = "aspect_test" };
            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Weapon, isAnyType: true);

            Assert.That(affixManager.GetAspect("aspect_test", ItemTypeConstants.Boots).IsAnyType, Is.True);
            Assert.That(affixManager.GetAspect("aspect_test", ItemTypeConstants.Amulet).IsAnyType, Is.True);
        }

        [Test]
        public void RemoveAspect_RemovesSingleAnyTypeEntry()
        {
            var affixManager = CreateAffixManagerWithPreset(out var preset);
            var aspectInfo = new AspectInfo { IdName = "aspect_test" };
            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Weapon, isAnyType: true);

            affixManager.RemoveAspect(preset.ItemAspects[0]);

            Assert.That(preset.ItemAspects, Is.Empty);
        }

        [Test]
        public void RemoveAspect_StillRemovesAllLegacyPerSlotEntries()
        {
            // A preset created before this fix (or before the ingest rework) can hold ten
            // identical-Id entries, one per slot. RemoveAspect must still clear all of
            // them; it was never migrated and matches by Id alone already.
            var affixManager = CreateAffixManagerWithPreset(out var preset);
            var aspectInfo = new AspectInfo { IdName = "aspect_test" };
            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Helm);
            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Chest);
            affixManager.AddAspect(aspectInfo, ItemTypeConstants.Boots);
            Assert.That(preset.ItemAspects, Has.Count.EqualTo(3));

            affixManager.RemoveAspect(preset.ItemAspects[0]);

            Assert.That(preset.ItemAspects, Is.Empty);
        }

        private sealed class FakeSettingsManager : ISettingsManager
        {
            public SettingsD4 Settings { get; } = new SettingsD4();
            public void LoadSettings() { }
            public void SaveSettings() { }
        }
    }
}
