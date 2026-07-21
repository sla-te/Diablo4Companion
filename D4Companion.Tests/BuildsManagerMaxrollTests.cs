using System.Text.Json;
using D4Companion.Entities;
using D4Companion.Interfaces;
using D4Companion.Services;
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
        [Test]
        public void CreatePresetFromMaxrollBuild_MidgameVariant_ProducesEightAspectsNotEighty()
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

            buildsManager.CreatePresetFromMaxrollBuild(maxrollBuild, "Midgame", "Midgame");

            Assert.That(affixManager.AddedPreset, Is.Not.Null);
            Assert.That(affixManager.AddedPreset!.ItemAspects, Has.Count.EqualTo(8));
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
