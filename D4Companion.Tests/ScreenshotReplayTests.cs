using CommunityToolkit.Mvvm.Messaging;
using D4Companion.Entities;
using D4Companion.Messages;
using D4Companion.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Drawing;
using System.Text;

namespace D4Companion.Tests
{
    /// <summary>
    /// Replays a screenshot the app itself captured back through the real detection
    /// pipeline, and reports what the overlay would draw for every affix it matches.
    ///
    /// This exists because the overlay can only be observed in game. Reasoning about it
    /// from the code is guesswork; a screenshot taken with the app's own Take Screenshot
    /// command (Ctrl+F10, or the camera button on the Debug tab) is captured by exactly the
    /// code path that feeds detection, so replaying it here reproduces what the overlay saw.
    ///
    /// How to use it:
    ///   1. In game, hover the item and press Ctrl+F10.
    ///      The PNG lands in the app's Screenshots folder.
    ///   2. Point this at it:
    ///      D4C_REPLAY_SCREENSHOT=/path/to/shot.png dotnet test --filter Replay
    ///   3. Read the report written beside the screenshot as <name>.replay.txt.
    ///
    /// The test is inconclusive rather than failing when no screenshot is supplied, so the
    /// normal suite is unaffected.
    ///
    /// It runs against the app's own output folder rather than the test output folder.
    /// Detection needs the system-preset template images, which are only deployed with the
    /// app, and using the app's folder also means the replay reads the same Config and the
    /// same selected affix preset the overlay was using - the point is to reproduce that
    /// run, not a synthetic one.
    ///
    /// What it does NOT cover: the drawing itself. It resolves the marker shape through the
    /// same OverlayMarkResolver the overlay uses, so the shape reported here is the shape
    /// drawn - but nothing here proves the pixels reach the screen.
    /// </summary>
    [NonParallelizable]
    public class ScreenshotReplayTests
    {
        private const string ScreenshotVariable = "D4C_REPLAY_SCREENSHOT";
        private const string AppDirectoryVariable = "D4C_REPLAY_APPDIR";
        private const string DefaultAppDirectory = @"..\..\..\..\D4Companion\bin\Release\net10.0-windows";

        [Test]
        public void Replay_CapturedScreenshot_ReportsWhatTheOverlayWouldDraw()
        {
            string? screenshot = Environment.GetEnvironmentVariable(ScreenshotVariable);
            if (string.IsNullOrWhiteSpace(screenshot))
            {
                Assert.Inconclusive($"Set {ScreenshotVariable} to a screenshot captured with the app's Take Screenshot command.");
                return;
            }

            screenshot = Path.GetFullPath(screenshot);
            Assert.That(File.Exists(screenshot), Is.True, $"No screenshot at {screenshot}");

            string appDirectory = Path.GetFullPath(Environment.GetEnvironmentVariable(AppDirectoryVariable)
                ?? Path.Combine(TestContext.CurrentContext.TestDirectory, DefaultAppDirectory));
            Assert.That(Directory.Exists(appDirectory), Is.True,
                $"No app output at {appDirectory}. Build D4Companion, or set {AppDirectoryVariable}.");

            // Every manager resolves its files relative to the working directory, so this
            // has to be switched before any of them are constructed - and restored, because
            // the whole suite shares this process.
            string originalDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(appDirectory);

            try
            {
                var settingsManager = new SettingsManager();
                settingsManager.LoadSettings();

                var tooltip = Replay(screenshot, settingsManager);
                string report = Describe(tooltip, screenshot, appDirectory, settingsManager);

                string reportPath = Path.ChangeExtension(screenshot, ".replay.txt");
                File.WriteAllText(reportPath, report);
                TestContext.Out.Write(report);

                Assert.That(tooltip.ItemAffixes, Is.Not.Empty,
                    $"Detection matched no affixes. Report written to {reportPath}.");
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDirectory);
            }
        }

        private static readonly List<string> _log = new();

        private static ItemTooltipDescriptor Replay(string screenshot, SettingsManager settingsManager)
        {
            var affixManager = new AffixManager(new CollectingLogger<AffixManager>(_log), settingsManager);

            var screenProcessHandler = new ScreenProcessHandler(
                new CollectingLogger<ScreenProcessHandler>(_log),
                affixManager,
                new OcrHandler(new CollectingLogger<OcrHandler>(_log), settingsManager),
                settingsManager,
                new SystemPresetManager(new CollectingLogger<SystemPresetManager>(_log), new Services.HttpClientHandler(NullLogger<Services.HttpClientHandler>.Instance), settingsManager),
                new TradeItemManager(new CollectingLogger<TradeItemManager>(_log), settingsManager))
            {
                // Set directly rather than through ToggleOverlayMessage: the messenger is
                // process-wide and a stray toggle would leak into other test classes.
                IsEnabled = true
            };

            var completed = new ManualResetEventSlim(false);
            ItemTooltipDescriptor? result = null;

            // A plain object as the recipient token, unregistered in the finally block, so
            // the process-wide messenger is left exactly as it was found.
            object recipient = new();
            WeakReferenceMessenger.Default.Register<TooltipDataReadyMessage>(recipient, (_, message) =>
            {
                result = message.Value.Tooltip;
                completed.Set();
            });

            try
            {
                // The pipeline disposes the bitmap it is handed, so this must be a copy it
                // may own rather than a handle the test still holds.
                using var source = new Bitmap(screenshot);
                var screen = new Bitmap(source);

                WeakReferenceMessenger.Default.Send(new ScreenCaptureReadyMessage(new ScreenCaptureReadyMessageParams
                {
                    CurrentScreen = screen
                }));

                // Detection runs on a background task. The bound is generous because
                // Tesseract on a full-resolution capture is slow on a cold run.
                Assert.That(completed.Wait(TimeSpan.FromSeconds(120)), Is.True, "Detection did not finish within 120s.");
            }
            finally
            {
                WeakReferenceMessenger.Default.UnregisterAll(recipient);
                GC.KeepAlive(screenProcessHandler);
            }

            return result!;
        }

        private static string Describe(ItemTooltipDescriptor tooltip, string screenshot, string appDirectory, SettingsManager settingsManager)
        {
            var report = new StringBuilder();
            report.AppendLine($"Screenshot : {screenshot}");
            report.AppendLine($"App folder : {appDirectory}");
            report.AppendLine($"Preset     : {settingsManager.Settings.SelectedAffixPreset}");
            report.AppendLine($"System     : {settingsManager.Settings.SelectedSystemPreset}");
            report.AppendLine($"Greater col: {settingsManager.Settings.DefaultColorGreater}");
            report.AppendLine($"Normal col : {settingsManager.Settings.DefaultColorNormal}");
            report.AppendLine();
            report.AppendLine($"Item type  : {tooltip.ItemType}");
            report.AppendLine($"Rarity     : {tooltip.ItemRarity}");
            report.AppendLine($"Item power : {tooltip.ItemPower}");
            report.AppendLine($"Affix rows : {tooltip.ItemAffixLocations.Count} located, {tooltip.ItemAffixes.Count} matched");
            report.AppendLine();
            report.AppendLine("row  mark        colour                rank  greater  affix");

            foreach (var (row, affix) in tooltip.ItemAffixes.OrderBy(a => a.Item1).Select(a => (a.Item1, a.Item2)))
            {
                bool isBelowMinimalValue = row < tooltip.ItemAffixAreas.Count &&
                    tooltip.ItemAffixAreas[row].AffixValue < tooltip.ItemAffixAreas[row].AffixThresholdValue;

                var mark = OverlayMarkResolver.Resolve(
                    affix,
                    isDungeonSigil: false,
                    settingsManager.Settings.IsMinimalAffixValueFilterEnabled,
                    isBelowMinimalValue);

                string rank = affix.Rank > 0 ? affix.Rank.ToString() : "-";
                report.AppendLine($"{row,3}  {mark,-11} {affix.Color,-21} {rank,4}  {affix.IsGreater,-7}  {affix.Id}");
            }

            if (_log.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("Pipeline log:");
                foreach (string line in _log) report.AppendLine($"  {line}");
            }

            return report.ToString();
        }
    }

    /// <summary>
    /// The pipeline reports why it gave up only through its logger, so a replay that finds
    /// nothing is undiagnosable with NullLogger - which is how the first replay run wasted a
    /// round trip. Collects into a list the report then prints.
    /// </summary>
    internal class CollectingLogger<T> : ILogger<T>
    {
        private readonly List<string> _sink;

        public CollectingLogger(List<string> sink) => _sink = sink;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            string message = formatter(state, exception);
            if (exception != null) message = $"{message} :: {exception.GetType().Name}: {exception.Message}";

            _sink.Add($"[{logLevel}] {typeof(T).Name}: {message}");
        }
    }
}
