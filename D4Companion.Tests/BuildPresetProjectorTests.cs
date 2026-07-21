using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;
using D4Companion.Services;

namespace D4Companion.Tests
{
    public class BuildPresetProjectorTests
    {
        private BuildPresetProjector _projector = null!;

        [SetUp]
        public void Setup()
        {
            _projector = new BuildPresetProjector(new SettingsManagerStub());
        }

        private static CanonicalVariant VariantWith(params CanonicalItem[] items)
        {
            return new CanonicalVariant { Name = "Midgame", Items = items.ToList() };
        }

        [Test]
        public void Project_KnownSlotAspect_EmitsSingleEntryAtThatSlot()
        {
            var variant = VariantWith(new CanonicalItem
            {
                Slot = ItemTypeConstants.Boots,
                SlotIsKnown = true,
                AspectIds = { "S05_BSK_Barbarian_001_x2" }
            });

            var preset = _projector.Project(variant, "test");

            Assert.That(preset.ItemAspects, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(preset.ItemAspects[0].Type, Is.EqualTo(ItemTypeConstants.Boots));
                Assert.That(preset.ItemAspects[0].IsAnyType, Is.False);
            });
        }

        [Test]
        public void Project_KnownSlotAspect_DoesNotEmitOtherSlots()
        {
            // Regression guard for the original defect: a boots aspect must never
            // produce a chest entry.
            var variant = VariantWith(new CanonicalItem
            {
                Slot = ItemTypeConstants.Boots,
                AspectIds = { "S05_BSK_Barbarian_001_x2" }
            });

            var preset = _projector.Project(variant, "test");

            Assert.That(preset.ItemAspects.Any(a => a.Type.Equals(ItemTypeConstants.Chest)), Is.False);
        }

        [Test]
        public void Project_UnknownSlotAspect_EmitsSingleAnyTypeEntry()
        {
            var variant = VariantWith(new CanonicalItem
            {
                Slot = ItemTypeConstants.Weapon,
                SlotIsKnown = false,
                AspectIds = { "legendary_generic_063" }
            });

            var preset = _projector.Project(variant, "test");

            Assert.That(preset.ItemAspects, Has.Count.EqualTo(1));
            Assert.That(preset.ItemAspects[0].IsAnyType, Is.True);
        }

        [Test]
        public void Project_SameAspectOnTwoItems_DeduplicatesPerSlot()
        {
            var variant = VariantWith(
                new CanonicalItem { Slot = ItemTypeConstants.Ring, AspectIds = { "legendary_generic_027" } },
                new CanonicalItem { Slot = ItemTypeConstants.Ring, AspectIds = { "legendary_generic_027" } });

            var preset = _projector.Project(variant, "test");

            Assert.That(preset.ItemAspects, Has.Count.EqualTo(1));
        }

        [Test]
        public void Project_AffixesCarryTheirItemSlot()
        {
            var variant = VariantWith(new CanonicalItem
            {
                Slot = ItemTypeConstants.Helm,
                Affixes = { new CanonicalAffix { Id = "CoreStat_Strength" } }
            });

            var preset = _projector.Project(variant, "test");

            Assert.That(preset.ItemAffixes[0].Type, Is.EqualTo(ItemTypeConstants.Helm));
        }

        [Test]
        public void Project_UniquesAreNotSlotTyped()
        {
            var variant = VariantWith(new CanonicalItem
            {
                Slot = ItemTypeConstants.Helm,
                UniqueIds = { "Helm_Unique_Barb_100" }
            });

            var preset = _projector.Project(variant, "test");

            Assert.That(preset.ItemUniques[0].Type, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Project_RunesUseRuneType()
        {
            var variant = VariantWith(new CanonicalItem
            {
                Slot = ItemTypeConstants.Chest,
                RuneIds = { "Item_Rune_Condition_OnSpendResource" }
            });

            var preset = _projector.Project(variant, "test");

            Assert.That(preset.ItemRunes[0].Type, Is.EqualTo(ItemTypeConstants.Rune));
        }

        [Test]
        public void Project_UsesGivenPresetName()
        {
            var preset = _projector.Project(VariantWith(), "My Preset");

            Assert.That(preset.Name, Is.EqualTo("My Preset"));
        }
    }

    /// <summary>
    /// Minimal ISettingsManager for projector tests. Only the colour properties are
    /// exercised. Implement the remaining interface members by throwing, so an
    /// unexpected dependency surfaces loudly rather than silently returning a default.
    /// </summary>
    internal class SettingsManagerStub : D4Companion.Interfaces.ISettingsManager
    {
        public SettingsD4 Settings { get; set; } = new SettingsD4();

        public void LoadSettings() => throw new NotImplementedException();
        public void SaveSettings() => throw new NotImplementedException();
    }
}
