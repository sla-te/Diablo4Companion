using D4Companion.Entities;
using D4Companion.Helpers;
using D4Companion.Services;
using FuzzierSharp;
using FuzzierSharp.SimilarityRatio;
using FuzzierSharp.SimilarityRatio.Scorer.StrategySensitive;
using System.Text.Json;

namespace D4Companion.Tests
{
    /// <summary>
    /// Pins the two things that decide which affix a Maxroll transfiguration entry resolves to:
    /// the corpus it is matched against, and the rewrite applied to the prose first.
    ///
    /// Both exist because of one build's list, every line of which was wrong or dropped in a
    /// different way:
    ///
    ///   Resource     imported as "Resource On Hit" - a stat the guide never named, and one
    ///                Transfiguration cannot even roll
    ///   Core ranks   dropped, after scoring 63 against "to Ravens"
    ///
    /// Neither is a scoring problem, so neither is fixed by moving the floor. The first is a
    /// corpus problem: with 893 candidates, DefaultRatioScorer's length penalty hands a bare
    /// stat word to the shortest description containing it. The second is a vocabulary problem:
    /// "ranks" appears in no affix description.
    ///
    /// These run against the real Affixes.enUS.json the app ships, through the real gates.
    /// </summary>
    public class TransfigurationCorpusTests
    {
        private List<AffixInfo> _affixes = null!;
        private List<string> _transfigurable = null!;

        private static string MatchKey(AffixInfo affix) => affix.DescriptionClean.Contains(")")
            ? affix.DescriptionClean.Split(['(', ')'], StringSplitOptions.RemoveEmptyEntries)[0]
            : affix.DescriptionClean;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new BoolConverter());
            options.Converters.Add(new IntConverter());

            using FileStream stream = File.OpenRead(@".\Data\Affixes.enUS.json");
            _affixes = JsonSerializer.Deserialize<List<AffixInfo>>(stream, options)!;

            _transfigurable = _affixes.Where(BuildsManagerMaxroll.IsTransfigurable).Select(MatchKey).ToList();
        }

        /// <summary>
        /// The resolver's two gates, run over the restricted corpus exactly as
        /// ResolveTransfigurations runs them.
        /// </summary>
        private string? Resolve(string guideEntry)
        {
            string prose = BuildsManagerMaxroll.NormaliseTransfigurationProse(guideEntry);
            var match = Process.ExtractOne(prose, _transfigurable, scorer: ScorerCache.Get<DefaultRatioScorer>());

            if (match is null || match.Score < 60) return null;

            return BuildsManagerMaxroll.MatchContainsEveryWordOf(prose, match.Value) ? match.Value : null;
        }

        // The Whirlwind guide's list, verbatim and in order. Every line has to land on the affix
        // a player would point at, or the overlay marks the wrong stat - which is worse than
        // marking none, because it looks like it worked.
        [TestCase("% Physical Damage", "Physical Damage")]
        [TestCase("Cooldown", "Cooldown Reduction")]
        [TestCase("Resource", "Maximum Resource")]
        [TestCase("Core ranks", "to Core Skills")]
        [TestCase("Critical Strike Chance", "Critical Strike Chance")]
        [TestCase("Attack Speed", "Attack Speed")]
        [TestCase("Strength", "Strength")]
        [TestCase("All Stats", "All Stats")]
        public void EveryEntryInTheGuideList_ResolvesToItsAffix(string guideEntry, string expected)
        {
            Assert.That(Resolve(guideEntry), Is.EqualTo(expected));
        }

        [Test]
        public void BareResource_DoesNotReachResourceOnHit()
        {
            // What shipped. "Resource On Hit" is 15 characters against "Maximum Resource" at 16,
            // and DefaultRatioScorer charges by length, so over the full corpus the wrong one wins
            // by a single character. Containment cannot catch it either - "Resource" is a genuine
            // subset of both. Only the corpus restriction separates them.
            var overTheWholeCorpus = Process.ExtractOne("Resource", _affixes.Select(MatchKey).ToList(),
                scorer: ScorerCache.Get<DefaultRatioScorer>());

            Assert.Multiple(() =>
            {
                Assert.That(overTheWholeCorpus.Value, Is.EqualTo("Resource On Hit"), "the failure this guards against");
                Assert.That(_transfigurable, Does.Not.Contain("Resource On Hit"));
                Assert.That(Resolve("Resource"), Is.EqualTo("Maximum Resource"));
            });
        }

        [Test]
        public void ResourceGeneration_IsNotTransfigurable()
        {
            // The stat the guide's "Resource" was assumed to mean, going by the amulet that
            // prompted this. Transfiguration cannot roll it, so the assumption was wrong and no
            // amount of matching would have made it right.
            Assert.Multiple(() =>
            {
                Assert.That(_affixes.Select(MatchKey), Does.Contain("Resource Generation"), "it is a real affix");
                Assert.That(_transfigurable, Does.Not.Contain("Resource Generation"), "just not one Transfiguration rolls");
            });
        }

        [TestCase("Core ranks", "to Core Skills")]
        [TestCase("Core Skill ranks", "to Core Skills")]
        [TestCase("Basic ranks", "to Basic Skills")]
        [TestCase("Ultimate Skills ranks", "to Ultimate Skills")]
        [TestCase("+3 Ranks to Core Skills", "+3 to Core Skills")]
        public void SkillRankShorthand_IsRewrittenToTheAffixWording(string guideEntry, string expected)
        {
            Assert.That(BuildsManagerMaxroll.NormaliseTransfigurationProse(guideEntry), Is.EqualTo(expected));
        }

        [TestCase("Critical Strike Chance")]
        [TestCase("All Stats")]
        [TestCase("% Physical Damage")]
        [TestCase("Cooldown")]
        public void ProseWithoutRanks_IsLeftAlone(string guideEntry)
        {
            Assert.That(BuildsManagerMaxroll.NormaliseTransfigurationProse(guideEntry), Is.EqualTo(guideEntry));
        }

        [Test]
        public void CoreRanks_IsUnreachableWithoutTheRewrite()
        {
            // Why the rewrite is not optional: unrewritten, the entry is lost either way, and the
            // two corpora lose it by different routes. Neither is a scoring problem, because
            // "ranks" appears in no affix description at all.
            var wide = Process.ExtractOne("Core ranks", _affixes.Select(MatchKey).ToList(), scorer: ScorerCache.Get<DefaultRatioScorer>());
            var pool = Process.ExtractOne("Core ranks", _transfigurable, scorer: ScorerCache.Get<DefaultRatioScorer>());

            Assert.Multiple(() =>
            {
                // Over the full corpus it clears the floor on a Druid raven affix, so the floor
                // alone would have imported it. Containment is what stopped that.
                Assert.That(wide.Value, Is.EqualTo("to Ravens"));
                Assert.That(wide.Score, Is.GreaterThanOrEqualTo(60));
                Assert.That(BuildsManagerMaxroll.MatchContainsEveryWordOf("Core ranks", wide.Value), Is.False);

                // Over the pool there is no raven affix to land on, so it fails at the floor
                // instead. Different gate, same dropped entry.
                Assert.That(pool.Score, Is.LessThan(60));

                Assert.That(Resolve("Core ranks"), Is.EqualTo("to Core Skills"), "the rewrite is what recovers it");
            });
        }

        [Test]
        public void TheTransfigurableSet_IsASmallCuratedPool()
        {
            // Sanity guard on the marker itself: if a data update renames the prefix this
            // collapses to zero and every transfiguration silently starts matching the wide
            // corpus again. The resolver falls back and warns in that case; this fails loudly.
            Assert.Multiple(() =>
            {
                Assert.That(_transfigurable, Has.Count.GreaterThan(20));
                Assert.That(_transfigurable.Count, Is.LessThan(_affixes.Count / 4));
                // Not ordinary affixes - they exist only as Transfiguration outcomes, which is
                // what identifies this set as the Transfiguration pool rather than a naming quirk.
                Assert.That(_transfigurable, Does.Contain("Gem Strength in this Item"));
                Assert.That(_transfigurable, Does.Contain("Item Quality"));
            });
        }
    }
}
