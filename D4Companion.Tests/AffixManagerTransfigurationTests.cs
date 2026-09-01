using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Services;

namespace D4Companion.Tests
{
    /// <summary>
    /// A preset's ItemTransfigurations list was write-only: the importer filled it and the
    /// preset editor showed it, but nothing on the detection path ever read it, so the
    /// overlay never marked a transfigured affix. These pin the lookup that closes that gap.
    /// </summary>
    public class AffixManagerTransfigurationTests
    {
        private static AffixPreset PresetWith(params ItemAffix[] transfigurations)
        {
            var preset = new AffixPreset { Name = "test" };
            preset.ItemTransfigurations.AddRange(transfigurations);
            return preset;
        }

        private static ItemAffix BuildWide(string id) => new ItemAffix
        {
            Id = id,
            Type = string.Empty,
            IsAnyType = true,
            IsTransfigured = true
        };

        private static ItemAffix ScopedTo(string id, string itemType) => new ItemAffix
        {
            Id = id,
            Type = itemType,
            IsAnyType = false,
            IsTransfigured = true
        };

        [Test]
        public void BuildWideEntry_MatchesAnySlot()
        {
            // Five of the six entries the Whirlwind guide lists carry no slot qualifier at
            // all, so the build-wide path is the common case, not the exception.
            var preset = PresetWith(BuildWide("critical-strike-chance"));

            var match = AffixManager.FindTransfiguration(preset, "critical-strike-chance",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Amulet);

            Assert.That(match, Is.Not.Null);
            Assert.That(match!.IsTransfigured, Is.True);
        }

        [Test]
        public void ScopedEntry_MatchesOnlyItsOwnSlot()
        {
            // "Cooldown (on 2-Handed Weapons)" is the one scoped entry in that guide.
            var preset = PresetWith(ScopedTo("cooldown-reduction", ItemTypeConstants.WeaponBludgeoning));

            Assert.Multiple(() =>
            {
                Assert.That(AffixManager.FindTransfiguration(preset, "cooldown-reduction",
                    AffixTypeConstants.Transfigured, ItemTypeConstants.WeaponBludgeoning), Is.Not.Null);
                Assert.That(AffixManager.FindTransfiguration(preset, "cooldown-reduction",
                    AffixTypeConstants.Transfigured, ItemTypeConstants.Gloves), Is.Null);
            });
        }

        [Test]
        public void ScopedEntry_ReachesAScannedPlainWeapon()
        {
            // The scope table stores 2-handed entries as weapon_bludgeoning / weapon_slicing,
            // but a tooltip only yields the Arsenal subtype on Barbarian in English. Everyone
            // else scans as plain "weapon", so the lookup has to cross the supertype the same
            // way IsTypeMatch does everywhere else - otherwise most users lose the entry.
            var preset = PresetWith(ScopedTo("cooldown-reduction", ItemTypeConstants.WeaponBludgeoning));

            Assert.That(AffixManager.FindTransfiguration(preset, "cooldown-reduction",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Weapon), Is.Not.Null);
        }

        [Test]
        public void ScopedEntryForAnotherSlot_DoesNotMaskABuildWideEntry()
        {
            // Both entries share an id. Testing IsAnyType after picking a first match would
            // let the off-slot entry win and return nothing. GetAffix and GetAspect resolve
            // this by filtering inside the predicate; keep all three in step.
            var preset = PresetWith(
                ScopedTo("cooldown-reduction", ItemTypeConstants.WeaponBludgeoning),
                BuildWide("cooldown-reduction"));

            var match = AffixManager.FindTransfiguration(preset, "cooldown-reduction",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Gloves);

            Assert.That(match, Is.Not.Null);
            Assert.That(match!.IsAnyType, Is.True);
        }

        [TestCase(AffixTypeConstants.Normal)]
        [TestCase(AffixTypeConstants.Greater)]
        [TestCase(AffixTypeConstants.Tempered)]
        [TestCase(AffixTypeConstants.Implicit)]
        public void NonTransfiguredArea_NeverMatches(string affixType)
        {
            // The list says which affixes the build wants TRANSFIGURED. An ordinary roll of
            // the same stat is not that, and must still fall through to ItemAffixes.
            var preset = PresetWith(BuildWide("critical-strike-chance"));

            Assert.That(AffixManager.FindTransfiguration(preset, "critical-strike-chance",
                affixType, ItemTypeConstants.Amulet), Is.Null);
        }

        [Test]
        public void UnwantedAffix_DoesNotMatch()
        {
            var preset = PresetWith(BuildWide("critical-strike-chance"));

            Assert.That(AffixManager.FindTransfiguration(preset, "maximum-life",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Amulet), Is.Null);
        }

        [Test]
        public void Match_CarriesTheRankOfTheOrdinaryAffix()
        {
            // Guide prose carries no ranking, so a transfiguration entry is always rank 0.
            // Returning it bare would blank the priority digit - DrawStatPriority draws
            // nothing at 0 - on exactly the stat the build ranks first.
            var preset = PresetWith(BuildWide("critical-strike-chance"));
            preset.ItemAffixes.Add(new ItemAffix
            {
                Id = "critical-strike-chance",
                Type = ItemTypeConstants.Amulet,
                Rank = 1
            });

            var match = AffixManager.FindTransfiguration(preset, "critical-strike-chance",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Amulet);

            Assert.That(match!.Rank, Is.EqualTo(1));
        }

        [Test]
        public void Match_TakesTheBestRankWhenSeveralEntriesShareTheId()
        {
            // Rank 1 is the highest priority, so the lowest positive rank wins.
            var preset = PresetWith(BuildWide("strength"));
            preset.ItemAffixes.Add(new ItemAffix { Id = "strength", Type = ItemTypeConstants.Amulet, Rank = 6 });
            preset.ItemAffixes.Add(new ItemAffix { Id = "strength", Type = ItemTypeConstants.Amulet, Rank = 2 });

            var match = AffixManager.FindTransfiguration(preset, "strength",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Amulet);

            Assert.That(match!.Rank, Is.EqualTo(2));
        }

        [Test]
        public void Match_DoesNotWriteTheRankBackIntoThePreset()
        {
            // The rank belongs to the ordinary affix. Writing it onto the stored entry would
            // persist it into the preset file the user saves.
            var preset = PresetWith(BuildWide("strength"));
            preset.ItemAffixes.Add(new ItemAffix { Id = "strength", Type = ItemTypeConstants.Amulet, Rank = 3 });

            AffixManager.FindTransfiguration(preset, "strength",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Amulet);

            Assert.That(preset.ItemTransfigurations[0].Rank, Is.EqualTo(0));
        }

        [Test]
        public void Match_WithNoOrdinaryEntry_HasNoRank()
        {
            // Five of the six guide entries are not ranked anywhere - they are build-wide
            // prose with no matching slot entry - and must still resolve.
            var preset = PresetWith(BuildWide("all-stats"));

            var match = AffixManager.FindTransfiguration(preset, "all-stats",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Amulet);

            Assert.That(match, Is.Not.Null);
            Assert.That(match!.Rank, Is.EqualTo(0));
        }

        [Test]
        public void EmptyList_DoesNotMatch()
        {
            // Every preset saved before transfigurations existed deserialises to this.
            Assert.That(AffixManager.FindTransfiguration(PresetWith(), "critical-strike-chance",
                AffixTypeConstants.Transfigured, ItemTypeConstants.Amulet), Is.Null);
        }
    }
}
