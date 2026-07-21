using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Services;
using System.Windows.Media;

namespace D4Companion.Tests
{
    /// <summary>
    /// The overlay's only statement about a stat the item does not have. It draws under the
    /// tooltip and can normally only be checked by looking at the game, so the selection rule
    /// is pinned here instead.
    /// </summary>
    public class MissingAffixResolverTests
    {
        private static AffixPreset PresetOf(params ItemAffix[] affixes)
        {
            var preset = new AffixPreset { Name = "test" };
            preset.ItemAffixes.AddRange(affixes);

            return preset;
        }

        private static ItemAffix Wanted(string id, string type, int rank = 0)
        {
            return new ItemAffix { Id = id, Type = type, Rank = rank, Color = Colors.Green };
        }

        [Test]
        public void AWantedStatTheItemDoesNotHave_IsMissing()
        {
            var preset = PresetOf(Wanted("crit", ItemTypeConstants.Amulet), Wanted("life", ItemTypeConstants.Amulet));

            var missing = MissingAffixResolver.Resolve(preset, ItemTypeConstants.Amulet, new[] { "life" });

            Assert.That(missing.Select(affix => affix.Id), Is.EqualTo(new[] { "crit" }));
        }

        [Test]
        public void AnItemWithEverything_ReportsNothing()
        {
            // The panel is drawn only when this is non-empty, so this is what makes a
            // complete item show no panel at all.
            var preset = PresetOf(Wanted("crit", ItemTypeConstants.Amulet));

            Assert.That(MissingAffixResolver.Resolve(preset, ItemTypeConstants.Amulet, new[] { "crit" }), Is.Empty);
        }

        [Test]
        public void StatsForOtherSlots_AreNotMissing()
        {
            var preset = PresetOf(Wanted("crit", ItemTypeConstants.Amulet), Wanted("armor", ItemTypeConstants.Helm));

            var missing = MissingAffixResolver.Resolve(preset, ItemTypeConstants.Amulet, Array.Empty<string>());

            Assert.That(missing.Select(affix => affix.Id), Is.EqualTo(new[] { "crit" }));
        }

        [Test]
        public void AGenericWeaponStat_IsMissingFromAWeaponSubtype()
        {
            // The regression this rule exists for: a preset stores a two-handed mace's stats
            // under "weapon", the tooltip reports "weapon_bludgeoning". Comparing the strings
            // directly would call every weapon stat missing from every weapon.
            var preset = PresetOf(Wanted("strength", ItemTypeConstants.Weapon), Wanted("life", ItemTypeConstants.Weapon));

            var missing = MissingAffixResolver.Resolve(preset, ItemTypeConstants.WeaponBludgeoning, new[] { "strength" });

            Assert.That(missing.Select(affix => affix.Id), Is.EqualTo(new[] { "life" }));
        }

        [Test]
        public void AnAnyTypeStat_IsMissingFromEverySlot()
        {
            var preset = PresetOf(new ItemAffix { Id = "anything", Type = ItemTypeConstants.Helm, IsAnyType = true, Color = Colors.Green });

            Assert.That(MissingAffixResolver.Resolve(preset, ItemTypeConstants.Boots, Array.Empty<string>()).Select(affix => affix.Id),
                Is.EqualTo(new[] { "anything" }));
        }

        [Test]
        public void ImplicitsAndUnwantedStats_AreNeverMissing()
        {
            var preset = PresetOf(
                new ItemAffix { Id = "implicit", Type = ItemTypeConstants.Boots, IsImplicit = true, Color = Colors.Green },
                new ItemAffix { Id = "unwanted", Type = ItemTypeConstants.Boots, Color = Colors.Red });

            // An implicit is not rolled, so the item cannot be short of one; red is how the
            // preset marks a stat it does not want, which is the opposite of missing.
            Assert.That(MissingAffixResolver.Resolve(preset, ItemTypeConstants.Boots, Array.Empty<string>()), Is.Empty);
        }

        [Test]
        public void MissingStats_AreOrderedByTheBuildsPriority()
        {
            // Unranked last: a plain numeric sort would put rank 0 ahead of the build's
            // top-priority stat, and the panel is truncated from the bottom.
            var preset = PresetOf(
                Wanted("unranked", ItemTypeConstants.Chest),
                Wanted("third", ItemTypeConstants.Chest, rank: 3),
                Wanted("first", ItemTypeConstants.Chest, rank: 1));

            var missing = MissingAffixResolver.Resolve(preset, ItemTypeConstants.Chest, Array.Empty<string>());

            Assert.That(missing.Select(affix => affix.Id), Is.EqualTo(new[] { "first", "third", "unranked" }));
        }

        [Test]
        public void SigilsAndRunes_ReportNothing()
        {
            // Picked from a catalogue rather than rolled: a preset lists many and an item
            // carries one, so every other entry would read as missing.
            var preset = PresetOf(Wanted("a", ItemTypeConstants.Sigil), Wanted("b", ItemTypeConstants.Rune));

            Assert.Multiple(() =>
            {
                Assert.That(MissingAffixResolver.Resolve(preset, ItemTypeConstants.Sigil, Array.Empty<string>()), Is.Empty);
                Assert.That(MissingAffixResolver.Resolve(preset, ItemTypeConstants.Rune, Array.Empty<string>()), Is.Empty);
            });
        }

        [Test]
        public void NoSelectedPreset_ReportsNothing()
        {
            Assert.That(MissingAffixResolver.Resolve(null, ItemTypeConstants.Helm, Array.Empty<string>()), Is.Empty);
        }
    }
}
