using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Services.BuildAdapters;

namespace D4Companion.Tests
{
    public class D2CoreBuildAdapterTests
    {
        private D2CoreBuildAdapter _adapter = null!;

        [SetUp]
        public void Setup()
        {
            _adapter = new D2CoreBuildAdapter();
        }

        private static D2CoreBuildVariantGearJson Gear(string itemType, string type, string key)
        {
            return new D2CoreBuildVariantGearJson { ItemType = itemType, Type = type, Key = key };
        }

        private static D2CoreBuild BuildWith(string variantName, Dictionary<string, D2CoreBuildVariantGearJson> gear)
        {
            return new D2CoreBuild
            {
                Name = "Test Build",
                Data = new D2CoreBuildDataJson
                {
                    Variants =
                    [
                        new D2CoreBuildDataVariantJson { Name = variantName, Gear = gear }
                    ]
                }
            };
        }

        [Test]
        public void ToCanonical_LegendaryAspect_BindsOnlyToItsOwnSlot()
        {
            // Regression guard for the fan-out defect: an aspect on a boots item must
            // not appear on any other slot.
            var build = BuildWith("Midgame", new Dictionary<string, D2CoreBuildVariantGearJson>
            {
                ["boots1"] = Gear("Boots", "legendary", "Affix_S05_BSK_Barbarian_001_x2"),
                ["chest1"] = Gear("ChestArmor", "rare", "irrelevant"),
            });

            var canonical = _adapter.ToCanonical(build);

            var items = canonical.Variants[0].Items;
            var bootsItem = items.Single(i => i.Slot == ItemTypeConstants.Boots);
            var chestItem = items.Single(i => i.Slot == ItemTypeConstants.Chest);

            Assert.Multiple(() =>
            {
                Assert.That(bootsItem.AspectIds, Is.EqualTo(new[] { "S05_BSK_Barbarian_001_x2" }));
                Assert.That(chestItem.AspectIds, Is.Empty);
            });
        }

        [Test]
        public void ToCanonical_StripsAffixPrefixCaseInsensitively()
        {
            var build = BuildWith("Midgame", new Dictionary<string, D2CoreBuildVariantGearJson>
            {
                ["ring1"] = Gear("Ring", "legendary", "affix_legendary_generic_027"),
            });

            var canonical = _adapter.ToCanonical(build);

            Assert.That(canonical.Variants[0].Items[0].AspectIds, Is.EqualTo(new[] { "legendary_generic_027" }));
        }

        [Test]
        public void ToCanonical_NonLegendaryItem_HasNoAspects()
        {
            var build = BuildWith("Midgame", new Dictionary<string, D2CoreBuildVariantGearJson>
            {
                ["helm1"] = Gear("Helm", "rare", "irrelevant"),
            });

            var canonical = _adapter.ToCanonical(build);

            Assert.That(canonical.Variants[0].Items[0].AspectIds, Is.Empty);
        }

        [Test]
        public void ToCanonical_AllKnownItemTypes_ResolveToExpectedSlot()
        {
            var pairs = new (string ItemType, string ExpectedSlot)[]
            {
                ("Helm", ItemTypeConstants.Helm),
                ("ChestArmor", ItemTypeConstants.Chest),
                ("Gloves", ItemTypeConstants.Gloves),
                ("Legs", ItemTypeConstants.Pants),
                ("Boots", ItemTypeConstants.Boots),
                ("Ring", ItemTypeConstants.Ring),
                ("Amulet", ItemTypeConstants.Amulet),
                ("DruidOffhand", ItemTypeConstants.Offhand),
                ("Focus", ItemTypeConstants.Offhand),
                ("Shield", ItemTypeConstants.Offhand),
                // Handedness/damage type confirmed via the paired class-name evidence
                // (Mace/Mace2H, Sword/Sword2H) and, for Polearm, an explicit exception.
                ("Mace", ItemTypeConstants.WeaponOneHand),
                ("Mace2H", ItemTypeConstants.WeaponBludgeoning),
                ("Sword", ItemTypeConstants.WeaponOneHand),
                ("Sword2H", ItemTypeConstants.WeaponSlicing),
                ("Polearm", ItemTypeConstants.WeaponSlicing),
                // Unverified handedness/damage type - no paired class evidence, kept as
                // plain Weapon rather than guessed.
                ("Dagger", ItemTypeConstants.Weapon),
                ("Glaive", ItemTypeConstants.Weapon),
                ("Quarterstaff", ItemTypeConstants.Weapon),
                ("Scythe", ItemTypeConstants.Weapon),
                ("Scythe2H", ItemTypeConstants.Weapon),
                ("Staff", ItemTypeConstants.Weapon),
                ("Wand", ItemTypeConstants.Weapon),
                ("Bow", ItemTypeConstants.Ranged),
                ("Crossbow2H", ItemTypeConstants.Ranged),
            };

            var gear = pairs.ToDictionary(
                p => p.ItemType,
                p => Gear(p.ItemType, "rare", "irrelevant"));

            var build = BuildWith("Midgame", gear);

            var canonical = _adapter.ToCanonical(build);

            var expectedSlotCounts = pairs
                .GroupBy(p => p.ExpectedSlot)
                .ToDictionary(g => g.Key, g => g.Count());
            var actualSlotCounts = canonical.Variants[0].Items
                .GroupBy(i => i.Slot)
                .ToDictionary(g => g.Key, g => g.Count());

            Assert.Multiple(() =>
            {
                Assert.That(canonical.Variants[0].Items, Has.Count.EqualTo(pairs.Length));
                Assert.That(actualSlotCounts, Is.EqualTo(expectedSlotCounts));
            });
        }

        [Test]
        public void ToCanonical_UnknownItemType_IsSkipped()
        {
            var build = BuildWith("Midgame", new Dictionary<string, D2CoreBuildVariantGearJson>
            {
                ["mystery1"] = Gear("SomeUnknownType", "rare", "irrelevant"),
            });

            var canonical = _adapter.ToCanonical(build);

            Assert.That(canonical.Variants[0].Items, Is.Empty);
        }

        [Test]
        public void ToCanonical_AllItems_HaveSlotIsKnownTrue()
        {
            var build = BuildWith("Midgame", new Dictionary<string, D2CoreBuildVariantGearJson>
            {
                ["helm1"] = Gear("Helm", "rare", "irrelevant"),
            });

            var canonical = _adapter.ToCanonical(build);

            Assert.That(canonical.Variants[0].Items[0].SlotIsKnown, Is.True);
        }

        [Test]
        public void ToCanonical_CopiesBuildAndVariantNames()
        {
            var build = BuildWith("Endgame", []);

            var canonical = _adapter.ToCanonical(build);

            Assert.Multiple(() =>
            {
                Assert.That(canonical.Name, Is.EqualTo("Test Build"));
                Assert.That(canonical.Variants[0].Name, Is.EqualTo("Endgame"));
            });
        }
    }
}
