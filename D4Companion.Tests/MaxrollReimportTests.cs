using D4Companion.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace D4Companion.Tests
{
    /// <summary>
    /// Re-imports a Maxroll build that is already cached in the app's Builds folder, writing
    /// the preset the app would write, without going through the GUI.
    ///
    /// Presets are a snapshot of the importer at the time they were made, so a preset
    /// imported before an importer fix keeps the old, wrong data until it is imported again.
    /// Verifying an importer change therefore means re-importing first, and doing that by
    /// hand between every check is slow and easy to forget.
    ///
    /// Usage - the profile is the variant name shown on the build page:
    ///   D4C_REIMPORT_BUILD=ce9zox0y D4C_REIMPORT_PROFILE=Midgame dotnet test --filter Reimport
    ///
    /// Inconclusive when no build is named, so the normal suite is unaffected.
    ///
    /// This writes Config/AffixPresets-v2.json in the app folder. Close the app first: it
    /// holds presets in memory and writes them back, so a running instance will overwrite
    /// whatever this produces.
    /// </summary>
    [NonParallelizable]
    public class MaxrollReimportTests
    {
        private const string BuildVariable = "D4C_REIMPORT_BUILD";
        private const string ProfileVariable = "D4C_REIMPORT_PROFILE";
        private const string AppDirectoryVariable = "D4C_REPLAY_APPDIR";
        private const string DefaultAppDirectory = @"..\..\..\..\D4Companion\bin\Release\net10.0-windows";

        [Test]
        public void Reimport_CachedMaxrollBuild_WritesPresetWithPriorityData()
        {
            string? buildId = Environment.GetEnvironmentVariable(BuildVariable);
            if (string.IsNullOrWhiteSpace(buildId))
            {
                Assert.Inconclusive($"Set {BuildVariable} to a build id cached under Builds/Maxroll.");
                return;
            }

            string profile = Environment.GetEnvironmentVariable(ProfileVariable) ?? "Midgame";

            string appDirectory = Path.GetFullPath(Environment.GetEnvironmentVariable(AppDirectoryVariable)
                ?? Path.Combine(TestContext.CurrentContext.TestDirectory, DefaultAppDirectory));
            Assert.That(Directory.Exists(appDirectory), Is.True,
                $"No app output at {appDirectory}. Build D4Companion, or set {AppDirectoryVariable}.");

            string originalDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(appDirectory);

            try
            {
                var settingsManager = new SettingsManager();
                settingsManager.LoadSettings();

                var affixManager = new AffixManager(NullLogger<AffixManager>.Instance, settingsManager);

                // The constructor loads every cached build from Builds/Maxroll, so there is
                // no download and no network access on this path. It does that on a
                // background task, though, so the list is empty for a moment after the
                // constructor returns and has to be waited for.
                var buildsManager = new BuildsManagerMaxroll(
                    NullLogger<BuildsManagerMaxroll>.Instance,
                    affixManager,
                    new BuildPresetProjector(settingsManager),
                    new Services.HttpClientHandler(NullLogger<Services.HttpClientHandler>.Instance),
                    settingsManager);

                var deadline = DateTime.UtcNow.AddSeconds(30);
                while (buildsManager.MaxrollBuilds.Count == 0 && DateTime.UtcNow < deadline)
                {
                    Thread.Sleep(50);
                }

                var build = buildsManager.MaxrollBuilds.SingleOrDefault(b => b.Id.Equals(buildId));
                Assert.That(build, Is.Not.Null,
                    $"Build {buildId} is not cached. Available: {string.Join(", ", buildsManager.MaxrollBuilds.Select(b => b.Id))}");

                string presetName = $"{build!.Name} - {profile}";
                buildsManager.CreatePresetFromMaxrollBuild(build, profile, presetName);

                var preset = affixManager.AffixPresets.SingleOrDefault(p => p.Name.Equals(presetName));
                Assert.That(preset, Is.Not.Null, $"Import produced no preset named {presetName}.");

                int ranked = preset!.ItemAffixes.Count(a => a.Rank > 0);
                int greater = preset.ItemAffixes.Count(a => a.IsGreater);

                TestContext.Out.WriteLine($"Preset  : {presetName}");
                TestContext.Out.WriteLine($"Affixes : {preset.ItemAffixes.Count}");
                TestContext.Out.WriteLine($"Ranked  : {ranked}");
                TestContext.Out.WriteLine($"Greater : {greater}");

                foreach (var group in preset.ItemAffixes.GroupBy(a => a.Type).OrderBy(g => g.Key))
                {
                    string affixes = string.Join(", ", group.OrderBy(a => a.Rank == 0 ? int.MaxValue : a.Rank)
                        .Select(a => $"{(a.Rank > 0 ? a.Rank.ToString() : "-")}{(a.IsGreater ? "*" : "")}:{a.Id}"));
                    TestContext.Out.WriteLine($"  {group.Key,-20} {affixes}");
                }

                Assert.Multiple(() =>
                {
                    Assert.That(ranked, Is.GreaterThan(0), "No affix carries a stat priority.");
                    Assert.That(greater, Is.GreaterThan(0), "No affix is flagged as a Greater Affix.");
                });
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }
    }
}
