using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Services.BuildAdapters;

namespace D4Companion.Tests
{
    public class MobalyticsBuildAdapterTests
    {
        private MobalyticsBuildAdapter _adapter = null!;

        [SetUp]
        public void Setup()
        {
            _adapter = new MobalyticsBuildAdapter();
        }

        private static MobalyticsBuildVariant EmptyVariant()
        {
            return new MobalyticsBuildVariant { Name = "Midgame" };
        }

        [Test]
        public void ToCanonical_CopiesBuildAndVariantName()
        {
            var source = EmptyVariant();

            var canonical = _adapter.ToCanonical(source, "Whirlwind Barbarian");

            Assert.Multiple(() =>
            {
                Assert.That(canonical.Name, Is.EqualTo("Whirlwind Barbarian"));
                Assert.That(canonical.Variants, Has.Count.EqualTo(1));
                Assert.That(canonical.Variants[0].Name, Is.EqualTo("Midgame"));
            });
        }

        [Test]
        public void ToCanonical_AspectsLandOnSingleUnslottedItem_NotFannedOutToTenSlots()
        {
            // Regression guard for the original defect this whole rework fixes:
            // aspects must not be registered under all ten equipment slots.
            var source = EmptyVariant();
            source.Aspect.Add("legendary_generic_063");
            source.Aspect.Add("legendary_generic_027");

            var canonical = _adapter.ToCanonical(source, "test");
            var variant = canonical.Variants[0];

            var unslottedItems = variant.Items.Where(i => !i.SlotIsKnown).ToList();
            Assert.That(unslottedItems, Has.Count.EqualTo(1));
            Assert.That(unslottedItems[0].AspectIds, Is.EquivalentTo(new[] { "legendary_generic_063", "legendary_generic_027" }));
        }

        [Test]
        public void ToCanonical_UniquesAndRunes_LandOnTheSameUnslottedItemAsAspects()
        {
            var source = EmptyVariant();
            source.Aspect.Add("legendary_generic_063");
            source.Uniques.Add("unique_001");
            source.Runes.Add("rune_of_x");

            var canonical = _adapter.ToCanonical(source, "test");
            var unslotted = canonical.Variants[0].Items.Single(i => !i.SlotIsKnown);

            Assert.Multiple(() =>
            {
                Assert.That(unslotted.UniqueIds, Is.EquivalentTo(new[] { "unique_001" }));
                Assert.That(unslotted.RuneIds, Is.EquivalentTo(new[] { "rune_of_x" }));
            });
        }

        [Test]
        public void ToCanonical_SlottedAffix_KeepsItsSlotAndFlags()
        {
            var source = EmptyVariant();
            source.Boots.Add(new MobalyticsAffix
            {
                AffixText = "CoreStat_Strength",
                IsGreater = true,
                IsImplicit = false,
                IsTempered = true
            });

            var canonical = _adapter.ToCanonical(source, "test");
            var bootsItem = canonical.Variants[0].Items.Single(i => i.Slot == ItemTypeConstants.Boots);

            Assert.Multiple(() =>
            {
                Assert.That(bootsItem.SlotIsKnown, Is.True);
                Assert.That(bootsItem.Affixes, Has.Count.EqualTo(1));
                Assert.That(bootsItem.Affixes[0].Id, Is.EqualTo("CoreStat_Strength"));
                Assert.That(bootsItem.Affixes[0].IsGreater, Is.True);
                Assert.That(bootsItem.Affixes[0].IsTempered, Is.True);
            });
        }

        [Test]
        public void ToCanonical_EmptySlotLists_ProduceNoItemForThatSlot()
        {
            var source = EmptyVariant();
            source.Boots.Add(new MobalyticsAffix { AffixText = "CoreStat_Strength" });

            var canonical = _adapter.ToCanonical(source, "test");
            var slots = canonical.Variants[0].Items.Where(i => i.SlotIsKnown).Select(i => i.Slot).ToList();

            Assert.That(slots, Is.EqualTo(new[] { ItemTypeConstants.Boots }));
        }

        [Test]
        public void ToCanonical_ParagonBoards_ArePassedThrough()
        {
            var source = EmptyVariant();
            source.ParagonBoards.Add(new ParagonBoard { Name = "Exploit" });

            var canonical = _adapter.ToCanonical(source, "test");

            Assert.That(canonical.Variants[0].ParagonBoards, Is.EqualTo(source.ParagonBoards));
        }
    }
}
