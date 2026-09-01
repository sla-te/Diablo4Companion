using System.Text.Json;
using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Helpers;
using D4Companion.Interfaces;
using D4Companion.Services;
using FuzzierSharp;
using FuzzierSharp.SimilarityRatio;
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
        private static AffixPreset CreatePreset(string profileName)
        {
            string json = File.ReadAllText(@".\Fixtures\ce9zox0y.json");
            var outer = JsonSerializer.Deserialize<MaxrollBuildJson>(json)!;
            var data = JsonSerializer.Deserialize<MaxrollBuildDataJson>(outer.Data)!;
            var maxrollBuild = new MaxrollBuild { Id = outer.Id, Name = outer.Name, Data = data };

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

            buildsManager.CreatePresetFromMaxrollBuild(maxrollBuild, profileName, profileName);

            Assert.That(affixManager.AddedPreset, Is.Not.Null);
            return affixManager.AddedPreset!;
        }

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
        public void CooldownTransfiguration_IsScopedToTwoHandedWeapons()
        {
            var scoped = CreatePreset("Endgame").ItemTransfigurations
                .Where(t => !t.IsAnyType)
                .Select(t => t.Type)
                .ToList();

            Assert.That(scoped, Is.EquivalentTo(new[]
            {
                ItemTypeConstants.WeaponBludgeoning,
                ItemTypeConstants.WeaponSlicing
            }));
        }

        [Test]
        public void StarterPreset_HasNoTransfigurations()
        {
            Assert.That(CreatePreset("Starter").ItemTransfigurations, Is.Empty);
        }
    }

    /// <summary>
    /// Pins where BuildsManagerMaxroll.TransfigurationMatchFloor has to sit. The floor is the
    /// one deliberate divergence from the D4Builds importer, which takes ExtractOne's best
    /// match unconditionally, so it needs a guard of its own: this reruns the same corpus and
    /// scorer the resolver uses and pins both ends of the gap the floor lives in.
    ///
    /// The gap is narrow because the guide names the stat, not the affix. DefaultRatioScorer
    /// is a plain length-sensitive ratio, so "Cooldown" against "Cooldown Reduction" - the
    /// correct answer - scores 62, against 94 to 100 for the stats that hit an affix
    /// description verbatim. Any floor tight enough to look comfortable drops it.
    ///
    /// The floor is a filter, not a proof. A short phrase can tie 62 by accident ("Any of the
    /// below" does), and no threshold separates a tie. What the floor buys is that sentences,
    /// which is what non-stat guide prose actually looks like, score 56 and below.
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

        // Prose a guide's transfiguration section can plausibly carry that names no affix.
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

        [Test]
        public void EveryFixtureStat_ScoresAtOrAboveTheFloor()
        {
            foreach (string stat in FixtureStats)
            {
                Assert.That(BestScore(stat), Is.GreaterThanOrEqualTo(Floor), $"stat: {stat}");
            }
        }

        [Test]
        public void NonAffixProse_ScoresBelowTheFloor()
        {
            foreach (string prose in NonAffixProse)
            {
                Assert.That(BestScore(prose), Is.LessThan(Floor), $"prose: {prose}");
            }
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
}
