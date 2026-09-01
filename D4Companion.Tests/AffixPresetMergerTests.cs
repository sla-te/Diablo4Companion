using D4Companion.Constants;
using D4Companion.Entities;
using System.Windows.Media;

namespace D4Companion.Tests
{
    /// <summary>
    /// The two-build merge silently dropped most of a preset for as long as it lived inside
    /// the merge dialog, where nothing could reach it. These pin what survives a merge.
    /// </summary>
    public class AffixPresetMergerTests
    {
        private static AffixPreset Merge(AffixPreset build1, AffixPreset build2)
        {
            return AffixPresetMerger.Merge(build1, build2, "merged", AffixPresetMergeColors.None);
        }

        [Test]
        public void EveryListSurvivesTheMerge()
        {
            // Uniques, runes and transfigurations were dropped outright. A user merging two
            // imported builds lost them with no warning and no way to notice but in game.
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemAffixes.Add(new ItemAffix { Id = "affix", Type = ItemTypeConstants.Amulet });
            build1.ItemAspects.Add(new ItemAffix { Id = "aspect", Type = ItemTypeConstants.Amulet });
            build1.ItemSigils.Add(new ItemAffix { Id = "sigil", Type = ItemTypeConstants.Sigil });
            build1.ItemUniques.Add(new ItemAffix { Id = "unique", Type = ItemTypeConstants.Chest });
            build1.ItemRunes.Add(new ItemAffix { Id = "rune", Type = ItemTypeConstants.Rune });
            build1.ItemTransfigurations.Add(new ItemAffix { Id = "transfiguration", IsAnyType = true, IsTransfigured = true });

            var merged = Merge(build1, new AffixPreset { Name = "two" });

            Assert.Multiple(() =>
            {
                Assert.That(merged.ItemAffixes, Has.Count.EqualTo(1), "affixes");
                Assert.That(merged.ItemAspects, Has.Count.EqualTo(1), "aspects");
                Assert.That(merged.ItemSigils, Has.Count.EqualTo(1), "sigils");
                Assert.That(merged.ItemUniques, Has.Count.EqualTo(1), "uniques");
                Assert.That(merged.ItemRunes, Has.Count.EqualTo(1), "runes");
                Assert.That(merged.ItemTransfigurations, Has.Count.EqualTo(1), "transfigurations");
            });
        }

        [Test]
        public void AffixFlags_SurviveTheMerge()
        {
            // IsAnyType and Rank were dropped from the affixes the merge did copy, so an
            // any-slot entry narrowed to one slot and the priority digit went blank.
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemAffixes.Add(new ItemAffix
            {
                Id = "affix",
                Type = ItemTypeConstants.Amulet,
                IsAnyType = true,
                IsGreater = true,
                Rank = 3
            });

            var affix = Merge(build1, new AffixPreset { Name = "two" }).ItemAffixes[0];

            Assert.Multiple(() =>
            {
                Assert.That(affix.IsAnyType, Is.True);
                Assert.That(affix.IsGreater, Is.True);
                Assert.That(affix.Rank, Is.EqualTo(3));
            });
        }

        [Test]
        public void SharedAffix_KeepsTheBetterRank()
        {
            // Rank 1 is the highest priority, so the lower positive number wins.
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemAffixes.Add(new ItemAffix { Id = "affix", Type = ItemTypeConstants.Amulet, Rank = 6 });
            var build2 = new AffixPreset { Name = "two" };
            build2.ItemAffixes.Add(new ItemAffix { Id = "affix", Type = ItemTypeConstants.Amulet, Rank = 2 });

            Assert.That(Merge(build1, build2).ItemAffixes[0].Rank, Is.EqualTo(2));
        }

        [Test]
        public void SharedAffix_PrefersARankedEntryOverAnUnrankedOne()
        {
            // 0 means unranked, not best. Taking a plain minimum would let it win.
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemAffixes.Add(new ItemAffix { Id = "affix", Type = ItemTypeConstants.Amulet, Rank = 0 });
            var build2 = new AffixPreset { Name = "two" };
            build2.ItemAffixes.Add(new ItemAffix { Id = "affix", Type = ItemTypeConstants.Amulet, Rank = 4 });

            Assert.That(Merge(build1, build2).ItemAffixes[0].Rank, Is.EqualTo(4));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void SharedEntry_StaysAsPermissiveAsEitherInput(bool anyTypeInBuild1, bool anyTypeInBuild2)
        {
            // Sources that cannot report an item's slot mark entries IsAnyType, so the two
            // builds routinely disagree. Narrowing loses matches; widening never does.
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemAspects.Add(new ItemAffix { Id = "aspect", Type = ItemTypeConstants.Amulet, IsAnyType = anyTypeInBuild1 });
            var build2 = new AffixPreset { Name = "two" };
            build2.ItemAspects.Add(new ItemAffix { Id = "aspect", Type = ItemTypeConstants.Amulet, IsAnyType = anyTypeInBuild2 });

            Assert.That(Merge(build1, build2).ItemAspects[0].IsAnyType, Is.True);
        }

        [Test]
        public void SharedRune_IsMatchedOnIdAlone()
        {
            // A rune sits in a socket, not on a slot, and GetRune resolves it by id only.
            // Matching on Type as well would duplicate the entry whenever the two builds
            // recorded a different item type for it.
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemRunes.Add(new ItemAffix { Id = "rune", Type = ItemTypeConstants.Rune });
            var build2 = new AffixPreset { Name = "two" };
            build2.ItemRunes.Add(new ItemAffix { Id = "rune", Type = ItemTypeConstants.Helm });

            Assert.That(Merge(build1, build2).ItemRunes, Has.Count.EqualTo(1));
        }

        [Test]
        public void EntryFromOnlyOneBuild_IsCarriedOver()
        {
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemTransfigurations.Add(new ItemAffix { Id = "from-one", IsAnyType = true, IsTransfigured = true });
            var build2 = new AffixPreset { Name = "two" };
            build2.ItemTransfigurations.Add(new ItemAffix { Id = "from-two", IsAnyType = true, IsTransfigured = true });

            var merged = Merge(build1, build2);

            Assert.That(merged.ItemTransfigurations.Select(t => t.Id),
                Is.EquivalentTo(new[] { "from-one", "from-two" }));
            Assert.That(merged.ItemTransfigurations.All(t => t.IsTransfigured), Is.True);
        }

        [Test]
        public void MergeDoesNotMutateEitherInput()
        {
            // The dialog leaves both source presets selected after a merge, and they are the
            // user's saved presets - writing a merged rank or IsAnyType back into one would
            // corrupt a preset they never asked to change.
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemAffixes.Add(new ItemAffix { Id = "affix", Type = ItemTypeConstants.Amulet, Rank = 6, IsAnyType = false });
            var build2 = new AffixPreset { Name = "two" };
            build2.ItemAffixes.Add(new ItemAffix { Id = "affix", Type = ItemTypeConstants.Amulet, Rank = 2, IsAnyType = true });

            Merge(build1, build2);

            Assert.Multiple(() =>
            {
                Assert.That(build1.ItemAffixes[0].Rank, Is.EqualTo(6));
                Assert.That(build1.ItemAffixes[0].IsAnyType, Is.False);
                Assert.That(build2.ItemAffixes[0].Rank, Is.EqualTo(2));
            });
        }

        [Test]
        public void RecolourOptions_ApplyPerSource()
        {
            var build1 = new AffixPreset { Name = "one" };
            build1.ItemAffixes.Add(new ItemAffix { Id = "only-one", Type = ItemTypeConstants.Amulet });
            build1.ItemAffixes.Add(new ItemAffix { Id = "shared", Type = ItemTypeConstants.Amulet });
            var build2 = new AffixPreset { Name = "two" };
            build2.ItemAffixes.Add(new ItemAffix { Id = "only-two", Type = ItemTypeConstants.Amulet });
            build2.ItemAffixes.Add(new ItemAffix { Id = "shared", Type = ItemTypeConstants.Amulet });

            var merged = AffixPresetMerger.Merge(build1, build2, "merged", new AffixPresetMergeColors(
                true, Colors.Blue, true, Colors.Yellow, true, Colors.Lime));

            Assert.Multiple(() =>
            {
                Assert.That(merged.ItemAffixes.Single(a => a.Id.Equals("only-one")).Color, Is.EqualTo(Colors.Blue));
                Assert.That(merged.ItemAffixes.Single(a => a.Id.Equals("only-two")).Color, Is.EqualTo(Colors.Yellow));
                Assert.That(merged.ItemAffixes.Single(a => a.Id.Equals("shared")).Color, Is.EqualTo(Colors.Lime));
            });
        }
    }
}
