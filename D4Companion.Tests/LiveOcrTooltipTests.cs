using D4Companion.Constants;
using D4Companion.Services;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Extensions.Logging.Abstractions;
using System.Drawing;

namespace D4Companion.Tests
{
    /// <summary>
    /// Runs a real in-game tooltip screenshot through the actual image pipeline: the same
    /// inverted-binary threshold ScreenProcessHandler applies, the same crop geometry, and
    /// real Tesseract - not text handed to the classifier.
    ///
    /// What is still not covered: screen capture and the template matching that locates the
    /// splitter line. The crop origin is supplied here instead of being matched, which is
    /// why the geometry is swept rather than fixed.
    /// </summary>
    public class LiveOcrTooltipTests
    {
        private OcrHandler _ocrHandler = null!;
        private SettingsManager _settingsManager = null!;
        private Image<Gray, byte> _filtered = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _settingsManager = new SettingsManager();
            _ocrHandler = new OcrHandler(NullLogger<OcrHandler>.Instance, _settingsManager);

            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "tooltip-1h-sword-slashing.png");
            using var source = new Image<Bgr, byte>(path);

            // ScreenProcessHandler feeds Tesseract an inverted binary image, never the raw
            // capture. Skipping this changes what Tesseract reads.
            _filtered = source.Convert<Gray, byte>()
                .ThresholdBinaryInv(new Gray(_settingsManager.Settings.ThresholdMin), new Gray(_settingsManager.Settings.ThresholdMax));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _filtered?.Dispose();
        }

        private string ReadUpperSection(int splitterY)
        {
            var settings = _settingsManager.Settings;
            int offsetLeft = settings.TypeAreaOffsetLeft;
            int startY = Math.Max(0, splitterY - settings.TooltipMaxHeight);
            int height = Math.Min(splitterY, settings.TooltipMaxHeight);
            int width = _filtered.Width - offsetLeft - settings.TypeAreaOffsetRight;

            using var area = _filtered.GetSubRect(new Rectangle(offsetLeft, startY, width, height));
            return _ocrHandler.ConvertToTextUpperTooltipSection(area.ToBitmap());
        }

        // The splitter position comes from template matching in the real pipeline. Sweeping
        // it proves the classification does not depend on hitting the crop exactly.
        [TestCase(300)]
        [TestCase(340)]
        [TestCase(360)]
        [TestCase(380)]
        [TestCase(400)]
        public void RealScreenshot_ClassifiesAsOneHandedRegardlessOfCropOrigin(int splitterY)
        {
            var result = _ocrHandler.ConvertToItemType(ReadUpperSection(splitterY));

            Assert.Multiple(() =>
            {
                Assert.That(result.TypeId, Is.EqualTo(ItemTypeConstants.WeaponOneHand));
                Assert.That(result.Type, Is.EqualTo("Ancestral Legendary Sword (Slashing)"));
                Assert.That(result.Rarity, Is.EqualTo("Legendary"));
            });
        }

        [Test]
        public void RealScreenshot_TesseractSplitsTheDamageTypeOntoItsOwnLine()
        {
            // This is why the wrapped-line cases in OcrHandlerTooltipClassificationTests
            // exist: on a tooltip this narrow the suffix genuinely arrives as a separate
            // line, so any rule reading only the item-type line would lose it.
            string rawText = ReadUpperSection(400);

            Assert.Multiple(() =>
            {
                Assert.That(rawText, Does.Contain("Ancestral Legendary Sword"));
                Assert.That(rawText, Does.Contain("(Slashing)"));
                Assert.That(rawText, Does.Not.Contain("Ancestral Legendary Sword (Slashing)"));
            });
        }

        [Test]
        public void RealScreenshot_ReadsItemPower()
        {
            var power = _ocrHandler.ConvertToPower(ReadUpperSection(400));

            Assert.That(power.Text, Does.Contain("900"));
        }

        [Test]
        public void RealScreenshot_ReadsAffixLines()
        {
            // Not the real affix path - that locates each affix by template-matching its
            // marker icon and OCRs one strip per affix. This only establishes that Tesseract
            // resolves the affix text on this capture at all.
            using var area = _filtered.GetSubRect(new Rectangle(40, 490, _filtered.Width - 60, 390));
            string text = _ocrHandler.ConvertToText(area.ToBitmap());

            Assert.Multiple(() =>
            {
                Assert.That(text, Does.Contain("Weapon Damage"));
                Assert.That(text, Does.Contain("Maximum Life"));
                Assert.That(text, Does.Contain("Physical Damage Multiplier"));
            });
        }
    }
}
