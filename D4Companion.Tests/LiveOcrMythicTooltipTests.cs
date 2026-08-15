using D4Companion.Constants;
using D4Companion.Services;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging.Abstractions;
using System.Drawing;

namespace D4Companion.Tests
{
    /// <summary>
    /// Runs real mythic tooltip captures through the actual image pipeline, the same way
    /// LiveOcrTooltipTests does for an ordinary legendary: the inverted-binary threshold
    /// ScreenProcessHandler applies, its crop geometry, and real Tesseract.
    ///
    /// Mythics are the tallest-headed items in the game. Above the first splitter they carry
    /// an item name wrapped onto two or three lines, "Ancestral Mythic Unique &lt;slot&gt;"
    /// wrapped onto two more, and Armory Loadout and Transfigured rows that ordinary items do
    /// not have. That pushed the item-type line outside the TooltipMaxHeight search window,
    /// so nothing classified and ScreenProcessHandler discarded the whole tooltip - the
    /// overlay drew no markers at all on any mythic.
    ///
    /// These pin both halves of the fix: that the default window really does fail, and that
    /// the taller retry window really does recover it.
    /// </summary>
    public class LiveOcrMythicTooltipTests
    {
        // Mirrors ScreenProcessHandler.TallHeaderRetryFactor.
        private const int TallHeaderRetryFactor = 2;

        private OcrHandler _ocrHandler = null!;
        private SettingsManager _settingsManager = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _settingsManager = new SettingsManager();
            _ocrHandler = new OcrHandler(NullLogger<OcrHandler>.Instance, _settingsManager);
        }

        private Image<Gray, byte> Load(string fixture)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", fixture);
            using var source = new Image<Bgr, byte>(path);

            return source.Convert<Gray, byte>()
                .ThresholdBinaryInv(new Gray(_settingsManager.Settings.ThresholdMin), new Gray(_settingsManager.Settings.ThresholdMax));
        }

        /// <summary>
        /// Mirrors ScreenProcessHandler.GetItemTypeArea. The splitter position comes from
        /// template matching in the real pipeline, so it is supplied here.
        /// </summary>
        private string ReadUpperSection(Image<Gray, byte> filtered, int splitterY, int maxHeight)
        {
            var settings = _settingsManager.Settings;
            int offsetLeft = settings.TypeAreaOffsetLeft;
            int startY = Math.Max(0, splitterY - maxHeight);
            int height = Math.Min(splitterY, maxHeight);
            int width = filtered.Width - offsetLeft - settings.TypeAreaOffsetRight;

            using var area = filtered.GetSubRect(new Rectangle(offsetLeft, startY, width, height));
            return _ocrHandler.ConvertToTextUpperTooltipSection(area.ToBitmap());
        }

        // splitterY is the y of the divider under the Transfigured row on each capture.
        [TestCase("tooltip-mythic-ring.png", 392)]
        [TestCase("tooltip-mythic-amulet.png", 380)]
        [TestCase("tooltip-mythic-helm.png", 446)]
        public void DefaultWindow_LosesTheItemTypeLine(string fixture, int splitterY)
        {
            using var filtered = Load(fixture);
            string rawText = ReadUpperSection(filtered, splitterY, _settingsManager.Settings.TooltipMaxHeight);
            var result = _ocrHandler.ConvertToItemType(rawText);

            Assert.Multiple(() =>
            {
                // The window starts below the item-type line, so only its wrapped tail - the
                // bare slot word - is inside it. Too little to match on.
                Assert.That(rawText, Does.Not.Contain("Ancestral"));
                Assert.That(rawText, Does.Contain("900 Item Power"));

                // Empty TypeId is what made ScreenProcessHandler throw the tooltip away.
                Assert.That(result.TypeId, Is.Empty);
            });
        }

        [TestCase("tooltip-mythic-ring.png", 392, ItemTypeConstants.Ring)]
        [TestCase("tooltip-mythic-amulet.png", 380, ItemTypeConstants.Amulet)]
        [TestCase("tooltip-mythic-helm.png", 446, ItemTypeConstants.Helm)]
        public void RetryWindow_RecoversTheItemType(string fixture, int splitterY, string expectedType)
        {
            using var filtered = Load(fixture);
            int maxHeight = _settingsManager.Settings.TooltipMaxHeight * TallHeaderRetryFactor;
            string rawText = ReadUpperSection(filtered, splitterY, maxHeight);
            var result = _ocrHandler.ConvertToItemType(rawText);

            Assert.Multiple(() =>
            {
                Assert.That(rawText, Does.Contain("Ancestral Mythic Unique"));
                Assert.That(result.TypeId, Is.EqualTo(expectedType));
            });
        }

        // The rarity comes out as Unique, not Mythic: the game labels these "Ancestral Mythic
        // Unique <slot>" while ItemTypes.*.json only carries "Ancestral Mythic <slot>", so the
        // fuzzy match lands on the Unique entry instead. Pinned rather than fixed - ItemRarity
        // has no functional consumer today, and correcting it means touching 14 locale files.
        [TestCase("tooltip-mythic-ring.png", 392)]
        [TestCase("tooltip-mythic-amulet.png", 380)]
        [TestCase("tooltip-mythic-helm.png", 446)]
        public void RetryWindow_ReportsMythicsAsUniqueRarity(string fixture, int splitterY)
        {
            using var filtered = Load(fixture);
            int maxHeight = _settingsManager.Settings.TooltipMaxHeight * TallHeaderRetryFactor;
            var result = _ocrHandler.ConvertToItemType(ReadUpperSection(filtered, splitterY, maxHeight));

            Assert.That(result.Rarity, Is.EqualTo(ItemRarityConstants.Unique));
        }
    }
}
