using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Messages;
using D4Companion.Helpers;
using D4Companion.Interfaces;
using D4Companion.Services;
using FuzzierSharp;
using FuzzierSharp.SimilarityRatio;
using FuzzierSharp.SimilarityRatio.Scorer;
using FuzzierSharp.SimilarityRatio.Scorer.Composite;
using FuzzierSharp.SimilarityRatio.Scorer.StrategySensitive;
using Microsoft.Extensions.Logging.Abstractions;

namespace D4Companion.Tests
{
    /// <summary>
    /// Wires the real Maxroll adapter, the real projector and the real fixture through
    /// BuildsManagerMaxroll.CreatePresetFromMaxrollBuild - the entry point Task 13 rewired.
    /// This is the regression guard for the original defect: importing the Midgame variant
    /// of ce9zox0y once produced 80 ItemAspects entries (every aspect fanned out across all
    /// ten slots). It must produce 8 - one per distinct aspect actually on the build.
    /// </summary>
    public class BuildsManagerMaxrollTests
    {
        internal static MaxrollBuild LoadFixture()
        {
            string json = File.ReadAllText(@".\Fixtures\ce9zox0y.json");
            var outer = JsonSerializer.Deserialize<MaxrollBuildJson>(json)!;
            var data = JsonSerializer.Deserialize<MaxrollBuildDataJson>(outer.Data)!;
            return new MaxrollBuild { Id = outer.Id, Name = outer.Name, Data = data };
        }

        /// <summary>
        /// Runs the real import and returns both what it produced and every warning it raised,
        /// so a test can assert on an entry the resolver DROPPED as well as on the ones it kept.
        /// </summary>
        internal static (AffixPreset Preset, List<string> Warnings) Import(MaxrollBuild maxrollBuild, string profileName)
        {
            var affixManager = new AffixManagerStub();
            var settingsManager = new SettingsManagerStub();
            settingsManager.Settings.IsImportParagonMaxrollEnabled = false;
            var projector = new BuildPresetProjector(settingsManager);

            var buildsManager = new BuildsManagerMaxroll(
                NullLogger<BuildsManagerMaxroll>.Instance,
                affixManager,
                projector,
                new HttpClientHandlerStub(),
                settingsManager);

            var warnings = new List<string>();
            var recipient = new object();
            WeakReferenceMessenger.Default.Register<WarningOccurredMessage>(
                recipient, (_, message) => warnings.Add(message.Value.Message));
            try
            {
                buildsManager.CreatePresetFromMaxrollBuild(maxrollBuild, profileName, profileName);
            }
            finally
            {
                WeakReferenceMessenger.Default.Unregister<WarningOccurredMessage>(recipient);
                GC.KeepAlive(recipient);
            }

            Assert.That(affixManager.AddedPreset, Is.Not.Null);
            return (affixManager.AddedPreset!, warnings);
        }

        private static AffixPreset CreatePreset(string profileName)
            => Import(LoadFixture(), profileName).Preset;

        /// <summary>
        /// The first sno of a resolved IdName. Affixes.enUS.json merges every matching sno into
        /// one semicolon-joined string, so the head is the stable, readable identity.
        /// </summary>
        private static string FirstSno(string idName) => idName.Split(';')[0];

        [Test]
        public void CreatePresetFromMaxrollBuild_MidgameVariant_ProducesEightAspectsNotEighty()
        {
            Assert.That(CreatePreset("Midgame").ItemAspects, Has.Count.EqualTo(8));
        }

        [Test]
        public void EndgamePreset_ContainsResolvedTransfigurations()
        {
            var preset = CreatePreset("Endgame");

            Assert.That(preset.ItemTransfigurations, Is.Not.Empty);
            Assert.That(preset.ItemTransfigurations.Select(t => t.Id),
                Has.No.Member("Critical Strike Chance"),
                "raw prose must be replaced by an affix IdName");
        }

        [Test]
        public void EndgamePreset_ResolvesEveryStatToTheGenericAffix()
        {
            // Pins the SCORER through the production wiring, which asserting Type alone does
            // not. Swap DefaultRatioScorer for any substring-tolerant one and "Cooldown"
            // resolves to CDR_Imbues ("Imbuement Cooldown Reduction") while "% Physical Damage"
            // resolves to Damage_Type_Bonus_NonPhysical or bare Damage - all of which clear
            // both the floor and the containment gate, so nothing else in this suite notices.
            var resolved = CreatePreset("Endgame").ItemTransfigurations
                .Select(t => FirstSno(t.Id))
                .Distinct()
                .ToList();

            Assert.That(resolved, Is.EquivalentTo(new[]
            {
                "Damage_Type_Bonus_Physical",
                "CooldownReductionCDR",
                "CritChance",
                "AttackSpeed",
                "CoreStat_Strength",
                "CoreStats_All"
            }));
        }

        [Test]
        public void CooldownTransfiguration_IsScopedToTwoHandedWeapons()
        {
            var scoped = CreatePreset("Endgame").ItemTransfigurations
                .Where(t => !t.IsAnyType)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(scoped.Select(t => t.Type), Is.EquivalentTo(new[]
                {
                    ItemTypeConstants.WeaponBludgeoning,
                    ItemTypeConstants.WeaponSlicing
                }));

                // The generic cooldown affix, not a skill-specific one.
                Assert.That(scoped.Select(t => FirstSno(t.Id)).Distinct(),
                    Is.EqualTo(new[] { "CooldownReductionCDR" }));
            });
        }

        [Test]
        public void StarterPreset_HasNoTransfigurations()
        {
            Assert.That(CreatePreset("Starter").ItemTransfigurations, Is.Empty);
        }

        /// <summary>
        /// Builds a widget-notes document of the shape MaxrollTransfigurationParser reads: a
        /// transfiguration heading followed by one list item per entry.
        /// </summary>
        private static MaxrollWidgetNotesJson TransfigurationNotes(params string[] entries)
        {
            static MaxrollLexicalNodeJson Block(string type, string text) => new()
            {
                Type = type,
                Children = [new MaxrollLexicalNodeJson { Type = "text", Text = text }]
            };

            var root = new MaxrollLexicalNodeJson { Type = "root" };
            root.Children.Add(Block("heading", "Optimal Tranfigurations"));
            foreach (string entry in entries)
            {
                root.Children.Add(Block("listitem", entry));
            }

            return new MaxrollWidgetNotesJson { Equipment = new MaxrollLexicalNodeJson { Root = root } };
        }

        [Test]
        public void JunkTransfiguration_IsDroppedAndWarnedThroughTheProductionPath()
        {
            // The end-to-end pin for the containment gate. The six real fixture entries all
            // pass both gates, so without this test the reject branch lives only in the mirror
            // in TransfigurationContainmentGateTests - delete MatchContainsEveryWordOf from
            // ResolveTransfigurations and the whole suite would stay green.
            //
            // "Two-Handed" scores 67 against "to Shred", clearing the floor, so only the gate
            // can stop it. "Critical Strike Chance" alongside it proves the import still ran.
            var build = LoadFixture();
            build.Data.Profiles.Single(p => p.Name.Equals("Endgame")).WidgetNotes =
                TransfigurationNotes("Two-Handed", "Critical Strike Chance");

            var (preset, warnings) = Import(build, "Endgame");

            Assert.Multiple(() =>
            {
                Assert.That(preset.ItemTransfigurations.Select(t => FirstSno(t.Id)),
                    Is.EqualTo(new[] { "CritChance" }), "the junk entry must not be imported");

                Assert.That(warnings.Where(w => w.Contains("Two-Handed")), Is.Not.Empty,
                    "the dropped entry must be reported");

                // The containment warning names what it matched, so a maintainer can tell a
                // near-miss from a nothing-like-an-affix miss.
                Assert.That(warnings.Single(w => w.Contains("Two-Handed")), Does.Contain("to Shred"));
            });
        }

        [Test]
        public void ProseWithNoAffixLikeMatch_IsDroppedWithTheBelowFloorWarning()
        {
            // The other rejection branch: a sentence scores 48, below the floor, so it never
            // reaches the containment gate and gets the generic wording instead.
            var build = LoadFixture();
            build.Data.Profiles.Single(p => p.Name.Equals("Endgame")).WidgetNotes =
                TransfigurationNotes("See the video guide linked above");

            var (preset, warnings) = Import(build, "Endgame");

            Assert.Multiple(() =>
            {
                Assert.That(preset.ItemTransfigurations, Is.Empty);
                Assert.That(warnings.Single(w => w.Contains("See the video guide linked above")),
                    Does.Contain("matched no affix"));
            });
        }

        [Test]
        public void DigitBearingProse_StillResolves()
        {
            // Purely numeric words are discarded before the containment check, so a guide that
            // writes a rolled value does not lose the entry: no DescriptionClean carries "15".
            var build = LoadFixture();
            build.Data.Profiles.Single(p => p.Name.Equals("Endgame")).WidgetNotes =
                TransfigurationNotes("15% Cooldown Reduction");

            var (preset, _) = Import(build, "Endgame");

            Assert.That(preset.ItemTransfigurations.Select(t => FirstSno(t.Id)),
                Is.EqualTo(new[] { "CooldownReductionCDR" }));
        }
    }

    /// <summary>
    /// Pins where BuildsManagerMaxroll.TransfigurationMatchFloor has to sit, and why the
    /// resolver keeps DefaultRatioScorer. The floor is the one deliberate divergence from the
    /// D4Builds importer, which takes ExtractOne's best match unconditionally, so it gets a
    /// guard of its own rather than being covered only indirectly by the fixture.
    ///
    /// Every number in the TransfigurationMatchFloor comment is asserted here. Change either
    /// place and this class fails.
    /// </summary>
    public class TransfigurationMatchFloorTests
    {
        // Keep in sync with BuildsManagerMaxroll.TransfigurationMatchFloor.
        private const int Floor = 60;

        private static readonly string[] FixtureStats =
        [
            "% Physical Damage",
            "Cooldown",
            "Critical Strike Chance",
            "Attack Speed",
            "Strength",
            "All Stats"
        ];

        // Sentence-shaped prose - what a guide note that names no affix actually looks like.
        private static readonly string[] NonAffixProse =
        [
            "See the video guide linked above",
            "Use whatever you have available",
            "Optimal Tranfigurations",
            "Watch the video for more details",
            "Note: prioritise Greater Affixes",
            "Check the Maxroll planner",
            "Aspect of the Umbral"
        ];

        // Short phrases that name no affix and that the floor does NOT reject. Kept as a
        // test, not a bug: the point is that this limit is measured and known, so the next
        // reader does not mistake the floor for a proof of affix-hood.
        private static readonly string[] ShortPhrasesTheFloorCannotReject =
        [
            "Two-Handed",
            "Skills",
            "Any of the below",
            "Endgame"
        ];

        private List<string> _affixDescriptions = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new BoolConverter());
            options.Converters.Add(new IntConverter());

            using FileStream stream = File.OpenRead(@".\Data\Affixes.enUS.json");
            var affixes = JsonSerializer.Deserialize<List<AffixInfo>>(stream, options)!;

            _affixDescriptions = affixes.Select(affix => affix.DescriptionClean.Contains(")")
                ? affix.DescriptionClean.Split(['(', ')'], StringSplitOptions.RemoveEmptyEntries)[0]
                : affix.DescriptionClean).ToList();
        }

        private int BestScore(string prose)
            => Process.ExtractOne(prose, _affixDescriptions, scorer: ScorerCache.Get<DefaultRatioScorer>()).Score;

        private string BestMatch<TScorer>(string prose) where TScorer : IRatioScorer, new()
            => Process.ExtractOne(prose, _affixDescriptions, scorer: ScorerCache.Get<TScorer>()).Value;

        [Test]
        public void EveryFixtureStat_ScoresAtOrAboveTheFloor()
        {
            foreach (string stat in FixtureStats)
            {
                Assert.That(BestScore(stat), Is.GreaterThanOrEqualTo(Floor), $"stat: {stat}");
            }
        }

        [Test]
        public void SentenceShapedProse_ScoresBelowTheFloor()
        {
            foreach (string prose in NonAffixProse)
            {
                Assert.That(BestScore(prose), Is.LessThan(Floor), $"prose: {prose}");
            }
        }

        [Test]
        public void TheGapIsSixPointsWide_MeasuredNotAssumed()
        {
            // Worst real stat and best sentence-shaped junk. The floor sits between them, and
            // both ends are only 6 points apart, which is the whole reason the floor is 60
            // rather than anything that looks safer.
            Assert.Multiple(() =>
            {
                Assert.That(FixtureStats.Min(BestScore), Is.EqualTo(62), "worst real stat");
                Assert.That(NonAffixProse.Max(BestScore), Is.EqualTo(56), "best sentence-shaped junk");
            });
        }

        [Test]
        public void ShortNonStatPhrases_ClearTheFloor_WhichIsWhyTheGateExists()
        {
            // These score at or above the correct "Cooldown" match, so no threshold separates
            // them. Rejecting them needs a different mechanism than a score floor - which is
            // what MatchContainsEveryWordOf is, pinned in TransfigurationContainmentGateTests.
            foreach (string prose in ShortPhrasesTheFloorCannotReject)
            {
                Assert.That(BestScore(prose), Is.GreaterThanOrEqualTo(Floor), $"prose: {prose}");
            }
        }

        [Test]
        public void SubstringTolerantScorers_ResolveCooldownToTheWrongAffix()
        {
            // The reason the resolver keeps DefaultRatioScorer despite the narrow gap. Every
            // substring-tolerant scorer scores a skill-prefixed variant perfectly, because it
            // contains the whole search string - a confident wrong match, which is exactly
            // what the floor exists to prevent. Higher scores, worse answers.
            Assert.Multiple(() =>
            {
                Assert.That(BestMatch<DefaultRatioScorer>("Cooldown"), Is.EqualTo("Cooldown Reduction"));
                Assert.That(BestMatch<WeightedRatioScorer>("Cooldown"), Is.Not.EqualTo("Cooldown Reduction"));
                Assert.That(BestMatch<TokenSetScorer>("Cooldown"), Is.Not.EqualTo("Cooldown Reduction"));
                Assert.That(BestMatch<PartialRatioScorer>("Cooldown"), Is.Not.EqualTo("Cooldown Reduction"));

                // And they degrade the other abbreviated entry too.
                Assert.That(BestMatch<DefaultRatioScorer>("% Physical Damage"), Is.EqualTo("Physical Damage"));
                Assert.That(BestMatch<WeightedRatioScorer>("% Physical Damage"), Is.EqualTo("Non-Physical Damage"));
                Assert.That(BestMatch<TokenSetScorer>("% Physical Damage"), Is.EqualTo("Damage"));
            });
        }
    }

    /// <summary>
    /// Minimal IAffixManager for this integration test. Only the members
    /// CreatePresetFromMaxrollBuild actually calls are given real behaviour; every aspect
    /// sno resolves to a distinct id so the assertion isolates the fan-out bug rather than
    /// depending on the production Data/Aspects.enUS.json lookup table. The rest throw, so an
    /// unexpected dependency surfaces loudly rather than silently returning a default.
    /// </summary>
    internal class AffixManagerStub : IAffixManager
    {
        public AffixPreset? AddedPreset { get; private set; }

        public List<AffixInfo> Affixes { get; } = new();
        public List<AffixPreset> AffixPresets { get; } = new();
        public List<AspectInfo> Aspects { get; } = new();
        public List<SigilInfo> Sigils { get; } = new();
        public List<UniqueInfo> Uniques { get; } = new();
        public List<RuneInfo> Runes { get; } = new();
        public List<ParagonBoardInfo> ParagonBoards { get; } = new();
        public List<ParagonGlyphInfo> ParagonGlyphs { get; } = new();

        public void AddAffix(AffixInfo affixInfo, string itemType) => throw new NotImplementedException();
        public void AddAffixPreset(AffixPreset affixPreset) => AddedPreset = affixPreset;
        public void AddAspect(AspectInfo aspectInfo, string itemType, bool isAnyType = false) => throw new NotImplementedException();
        public void AddSigil(SigilInfo sigilInfo, string itemType) => throw new NotImplementedException();
        public void AddUnique(UniqueInfo uniqueInfo) => throw new NotImplementedException();
        public void AddRune(RuneInfo runeInfo) => throw new NotImplementedException();
        public ItemAffix GetAffix(string affixId, string affixType, string itemType) => throw new NotImplementedException();
        public string GetAffixDescription(string affixId) => throw new NotImplementedException();
        public string GetAffixId(string affixSno) => throw new NotImplementedException();
        public AffixInfo? GetAffixInfoMaxrollByIdSno(string affixIdSno) => null;
        public AffixInfo? GetAffixInfoByIdName(string affixIdName) => throw new NotImplementedException();
        public double GetAffixMinimalValue(string idName) => throw new NotImplementedException();
        public ItemAffix GetAspect(string aspectId, string itemType) => throw new NotImplementedException();
        public string GetAspectDescription(string aspectId) => throw new NotImplementedException();
        public string GetAspectName(string aspectId) => throw new NotImplementedException();
        public AspectInfo? GetAspectInfoMaxrollByIdSno(string aspectIdSno) => new AspectInfo { IdName = $"resolved_{aspectIdSno}" };
        public AspectInfo? GetAspectInfoMaxrollByIdName(string aspectIdName) => throw new NotImplementedException();
        public string GetParagonBoardLocalisation(string id) => throw new NotImplementedException();
        public string GetParagonGlyphLocalisation(string id) => throw new NotImplementedException();
        public string GetParagonGlyphLocalisationByNumber(string id) => throw new NotImplementedException();
        public ItemAffix GetSigil(string affixId, string itemType) => throw new NotImplementedException();
        public string GetSigilDescription(string sigilId) => throw new NotImplementedException();
        public string GetSigilDungeonTier(string sigilId) => throw new NotImplementedException();
        public string GetSigilType(string sigilId) => throw new NotImplementedException();
        public string GetSigilName(string sigilId) => throw new NotImplementedException();
        public ItemAffix GetUnique(string uniqueId, string itemType) => throw new NotImplementedException();
        public string GetUniqueDescription(string uniqueId) => throw new NotImplementedException();
        public UniqueInfo? GetUniqueInfoMaxrollByIdSno(string idSno) => null;
        public string GetUniqueName(string uniqueId) => throw new NotImplementedException();
        public ItemAffix GetRune(string runeId, string itemType) => throw new NotImplementedException();
        public string GetRuneDescription(string runeId) => throw new NotImplementedException();
        public string GetRuneName(string runeId) => throw new NotImplementedException();
        public string GetGearOrSigilAffixDescription(string value) => throw new NotImplementedException();
        public bool IsDuplicate(ItemAffix itemAffix) => throw new NotImplementedException();
        public void RemoveAffix(ItemAffix itemAffix) => throw new NotImplementedException();
        public void RemoveAspect(ItemAffix itemAffix) => throw new NotImplementedException();
        public void RemoveSigil(ItemAffix itemAffix) => throw new NotImplementedException();
        public void RemoveUnique(ItemAffix itemAffix) => throw new NotImplementedException();
        public void RemoveRune(ItemAffix itemAffix) => throw new NotImplementedException();
        public void RemoveAffixPreset(AffixPreset affixPreset) => throw new NotImplementedException();
        public void RenamePreset(string oldName, string newName) => throw new NotImplementedException();
        public void ResetMinimalAffixValues() => throw new NotImplementedException();
        public void SaveAffixColor(ItemAffix itemAffix) => throw new NotImplementedException();
        public void SaveAffixPresets() => throw new NotImplementedException();
        public void SetAffixMinimalValue(string idName, double minimalValue) => throw new NotImplementedException();
        public void SetSigilDungeonTier(SigilInfo sigilInfo, string tier) => throw new NotImplementedException();
        public void SetIsAnyType(ItemAffix itemAffix, bool isAnyType) => throw new NotImplementedException();
    }

    /// <summary>Minimal IHttpClientHandler. Never called by CreatePresetFromMaxrollBuild.</summary>
    internal class HttpClientHandlerStub : IHttpClientHandler
    {
        public Task<string> GetRequest(string uri) => throw new NotImplementedException();
        public Task DownloadZip(string uri) => throw new NotImplementedException();
        public Task DownloadZipSystemPreset(string uri) => throw new NotImplementedException();
    }

    /// <summary>
    /// Pins the second gate: every word of the guide entry must appear in the description
    /// ExtractOne matched it to. It exists because the score floor provably cannot deliver
    /// "a wrong match is worse than no match" on its own - see
    /// TransfigurationMatchFloorTests for the measurement that shows best-junk beats
    /// worst-real-stat under every scorer.
    ///
    /// The accept direction is also covered end to end: if the real gate rejected any of the
    /// six fixture entries, EndgamePreset_ContainsResolvedTransfigurations and
    /// CooldownTransfiguration_IsScopedToTwoHandedWeapons would fail. What this class adds is
    /// the reject direction, on strings the fixture cannot supply.
    /// </summary>
    public class TransfigurationContainmentGateTests
    {
        // Mirrors BuildsManagerMaxroll.MatchContainsEveryWordOf, which is private and cannot
        // be reached from here without an InternalsVisibleTo this project does not have.
        // Keep in sync. Drift is caught end to end by
        // BuildsManagerMaxrollTests.JunkTransfiguration_IsDroppedAndWarnedThroughTheProductionPath
        // (reject) and the fixture tests (accept).
        private static string[] Words(string text)
            => new string(text.Select(c => char.IsLetterOrDigit(c) ? c : ' ').ToArray())
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !word.All(char.IsDigit))
                .ToArray();

        private static bool Gate(string prose, string description)
        {
            string[] proseWords = Words(prose);
            if (proseWords.Length == 0) return false;

            var descriptionWords = Words(description).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return proseWords.All(word => descriptionWords.Contains(word));
        }

        // prose, the description DefaultRatioScorer actually matches it to.
        [TestCase("% Physical Damage", "Physical Damage")]
        [TestCase("Cooldown", "Cooldown Reduction")]
        [TestCase("Critical Strike Chance", "Critical Strike Chance")]
        [TestCase("Attack Speed", "Attack Speed")]
        [TestCase("Strength", "Strength")]
        [TestCase("All Stats", "All Stats")]
        public void EveryFixtureEntry_PassesTheGate(string prose, string description)
        {
            // The leading "%" costs nothing because words are letters and digits only.
            Assert.That(Gate(prose, description), Is.True);
        }

        [TestCase("Two-Handed", "to Shred")]
        [TestCase("Any of the below", "to Death Blow")]
        [TestCase("Endgame", "Damage")]
        public void ShortPhrasesTheFloorLetsThrough_AreRejectedByTheGate(string prose, string description)
        {
            // All three score at or above the floor. The gate is what stops them.
            Assert.That(Gate(prose, description), Is.False);
        }

        [Test]
        public void Skills_SurvivesBothGates_AndIsAKnownLimit()
        {
            // Documented, not overlooked. "Skills" scores 63 against the affix "to All
            // Skills" and is a genuine subset of it, so neither the floor nor the gate has
            // grounds to reject it. Separating a section header from a stat that shares its
            // vocabulary needs knowledge this importer does not have.
            Assert.That(Gate("Skills", "to All Skills"), Is.True);
        }

        [Test]
        public void WordComparison_IsCaseInsensitive()
        {
            Assert.That(Gate("cooldown", "Cooldown Reduction"), Is.True);
        }

        [TestCase("15% Cooldown Reduction", "Cooldown Reduction")]
        [TestCase("+3 Ranks to Core Skills", "Ranks to Core Skills")]
        public void PurelyNumericWords_AreIgnored(string prose, string description)
        {
            // A guide is free to write a rolled value. No DescriptionClean carries one, so
            // keeping "15" or "3" as a word would reject a perfect score-100 match.
            Assert.That(Gate(prose, description), Is.True);
        }

        [Test]
        public void AlphanumericWords_AreNotIgnored()
        {
            // Only PURELY numeric words drop. "2H" is still a word and still has to appear.
            Assert.That(Gate("2H Damage", "Physical Damage"), Is.False);
        }

        [Test]
        public void ProseWithNoWordCharacters_IsRejected()
        {
            // Otherwise "every word is contained" would be vacuously true. Digits alone count
            // as no words, so a bare "15" is rejected too.
            Assert.Multiple(() =>
            {
                Assert.That(Gate("%%%", "Physical Damage"), Is.False);
                Assert.That(Gate("15", "Cooldown Reduction"), Is.False);
            });
        }
    }
}
