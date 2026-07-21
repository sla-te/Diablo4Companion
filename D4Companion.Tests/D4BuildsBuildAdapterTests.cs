using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Services.BuildAdapters;

namespace D4Companion.Tests
{
    public class D4BuildsBuildAdapterTests
    {
        private D4BuildsBuildAdapter _adapter = null!;

        [SetUp]
        public void Setup()
        {
            _adapter = new D4BuildsBuildAdapter();
        }

        [Test]
        public void ToCanonical_AspectsHaveNoSlotProvenance_LandOnSingleUnknownSlotItem()
        {
            // This is the honest-ignorance case: D4Builds scrapes aspects into a flat
            // list with no slot association. The adapter must not fabricate slots for
            // them - regression guard for the "every aspect on all ten slots" defect.
            var sourceVariant = new D4BuildsBuildVariant
            {
                Name = "Variant A",
                Aspect = { "legendary_generic_063", "legendary_generic_027" }
            };

            var canonical = _adapter.ToCanonical(sourceVariant, "Test Build");
            var variant = canonical.Variants.Single();

            var unslotted = variant.Items.Where(i => !i.SlotIsKnown).ToList();
            Assert.That(unslotted, Has.Count.EqualTo(1));
            Assert.That(unslotted[0].AspectIds, Is.EquivalentTo(new[] { "legendary_generic_063", "legendary_generic_027" }));
        }

        [Test]
        public void ToCanonical_UniquesAndRunes_LandOnTheSameUnknownSlotItemAsAspects()
        {
            var sourceVariant = new D4BuildsBuildVariant
            {
                Name = "Variant A",
                Aspect = { "legendary_generic_063" },
                Uniques = { "Helm_Unique_Barb_100" },
                Runes = { "Item_Rune_Condition_OnSpendResource" }
            };

            var canonical = _adapter.ToCanonical(sourceVariant, "Test Build");
            var unslotted = canonical.Variants.Single().Items.Single(i => !i.SlotIsKnown);

            Assert.Multiple(() =>
            {
                Assert.That(unslotted.AspectIds, Is.EquivalentTo(new[] { "legendary_generic_063" }));
                Assert.That(unslotted.UniqueIds, Is.EquivalentTo(new[] { "Helm_Unique_Barb_100" }));
                Assert.That(unslotted.RuneIds, Is.EquivalentTo(new[] { "Item_Rune_Condition_OnSpendResource" }));
            });
        }

        [Test]
        public void ToCanonical_AffixesCarryTheirRealSlot_UnlikeAspects()
        {
            // Affixes DO carry slot provenance from D4Builds and must map normally -
            // this is the asymmetry the task is about, not a bug to "fix".
            var sourceVariant = new D4BuildsBuildVariant
            {
                Name = "Variant A",
                Boots = { new D4buildsAffix { AffixText = "CoreStat_Strength" } }
            };

            var canonical = _adapter.ToCanonical(sourceVariant, "Test Build");
            var bootsItem = canonical.Variants.Single().Items.Single(i => i.Slot == ItemTypeConstants.Boots);

            Assert.Multiple(() =>
            {
                Assert.That(bootsItem.SlotIsKnown, Is.True);
                Assert.That(bootsItem.Affixes.Single().Id, Is.EqualTo("CoreStat_Strength"));
            });
        }

        [Test]
        public void ToCanonical_EmptySlotLists_ProduceNoItemForThatSlot()
        {
            var sourceVariant = new D4BuildsBuildVariant { Name = "Variant A" };

            var canonical = _adapter.ToCanonical(sourceVariant, "Test Build");
            var variant = canonical.Variants.Single();

            // Only the always-present unslotted item should exist when no slot list has entries.
            Assert.That(variant.Items, Has.Count.EqualTo(1));
            Assert.That(variant.Items[0].SlotIsKnown, Is.False);
        }

        [Test]
        public void ToCanonical_SetsBuildNameAndVariantName()
        {
            var sourceVariant = new D4BuildsBuildVariant { Name = "Variant A" };

            var canonical = _adapter.ToCanonical(sourceVariant, "Test Build");

            Assert.Multiple(() =>
            {
                Assert.That(canonical.Name, Is.EqualTo("Test Build"));
                Assert.That(canonical.Variants.Single().Name, Is.EqualTo("Variant A"));
            });
        }

        [Test]
        public void ToCanonical_PassesParagonBoardsThrough()
        {
            var board = new ParagonBoard { Name = "Board 1" };
            var sourceVariant = new D4BuildsBuildVariant
            {
                Name = "Variant A",
                ParagonBoards = { board }
            };

            var canonical = _adapter.ToCanonical(sourceVariant, "Test Build");

            Assert.That(canonical.Variants.Single().ParagonBoards, Is.EqualTo(new[] { board }));
        }
    }
}
