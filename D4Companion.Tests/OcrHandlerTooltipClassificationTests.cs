using D4Companion.Constants;
using D4Companion.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace D4Companion.Tests
{
    /// <summary>
    /// Drives the real OcrHandler classification path with tooltip text captured from the
    /// game, rather than re-implementing the lookup as OcrHandlerItemTypeTests does.
    ///
    /// ConvertToItemType takes text rather than an image, so these exercise the shipped
    /// fuzzy match and weapon-subtype resolution without Tesseract. They are integration
    /// tests, not unit tests: constructing OcrHandler parses every locale data file from
    /// .\Data, registers a process-wide messenger handler, and reads Config/Settings.json
    /// through SettingsManager - and ConvertToItemType then gates on the
    /// MinimalOcrMatchType threshold from those settings.
    ///
    /// The image half of the pipeline (screen capture, binarize, crop, Tesseract) is not
    /// covered here - it needs a live screen or a hand-cropped fixture.
    /// </summary>
    public class OcrHandlerTooltipClassificationTests
    {
        private OcrHandler _ocrHandler = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // The constructor parses every locale data file, so build it once. It reads
            // .\Data and .\tessdata relative to the working directory, which the test host
            // sets to the output folder.
            _ocrHandler = new OcrHandler(NullLogger<OcrHandler>.Instance, new SettingsManager());
        }

        [Test]
        public void ConvertToItemType_OneHandedSlashingSword_ResolvesToOneHandParent()
        {
            // Verbatim from an equipped item tooltip. The damage-type suffix is present but
            // there is no hand marker anywhere in the text - which is the reason mainhand
            // and offhand cannot be told apart by scanning.
            var result = _ocrHandler.ConvertToItemType("Ancestral Legendary Sword (Slashing)\n900 Item Power");

            Assert.Multiple(() =>
            {
                Assert.That(result.TypeId, Is.EqualTo(ItemTypeConstants.WeaponOneHand));
                Assert.That(result.TypeId, Is.Not.EqualTo(ItemTypeConstants.WeaponSlicing));
            });
        }

        // A narrow tooltip wraps the item-type line, putting the damage-type suffix on its
        // own line, and wraps a long item name across two lines above it. Every one of
        // these must still resolve, or the Arsenal split would silently degrade to plain
        // "weapon" purely because of how wide the tooltip happened to render.
        [TestCase("Ancestral Legendary Sword (Slashing)\n900 Item Power")]
        [TestCase("Ancestral Legendary Sword\n(Slashing)\n900 Item Power")]
        [TestCase("Ghoul King's Blade of Exorcism\nAncestral Legendary Sword\n(Slashing)\n900 Item Power")]
        [TestCase("Ghoul King's Blade of\nExorcism\nAncestral Legendary Sword\n(Slashing)\n900 Item Power\nArmory Loadout")]
        public void ConvertToItemType_WrappedTypeLine_StillResolvesTheDamageType(string rawText)
        {
            var result = _ocrHandler.ConvertToItemType(rawText);

            Assert.Multiple(() =>
            {
                Assert.That(result.TypeId, Is.EqualTo(ItemTypeConstants.WeaponOneHand));
                Assert.That(result.Type, Is.EqualTo("Ancestral Legendary Sword (Slashing)"));
            });
        }

        [Test]
        public void ConvertToItemType_TwoHandedMace_ResolvesToBludgeoning()
        {
            var result = _ocrHandler.ConvertToItemType("Ancestral Legendary Two-Handed Mace (Bludgeoning)\n900 Item Power");

            Assert.That(result.TypeId, Is.EqualTo(ItemTypeConstants.WeaponBludgeoning));
        }

        [Test]
        public void ConvertToItemType_Polearm_ResolvesToSlicingDespiteNoTwoHandedPrefix()
        {
            // Polearm is two-handed but its name carries no "Two-Handed" prefix, so this is
            // the case a prefix-only rule would misclassify as one-handed.
            var result = _ocrHandler.ConvertToItemType("Ancestral Legendary Polearm (Slashing)\n900 Item Power");

            Assert.That(result.TypeId, Is.EqualTo(ItemTypeConstants.WeaponSlicing));
        }

        [Test]
        public void ConvertToItemType_NeverProducesAnArsenalHand()
        {
            // The hands exist only in imported presets. If scanning ever started emitting
            // one, IsTypeMatch's parent rule would stop being the thing that connects a
            // scanned one-hander to an imported entry.
            string[] tooltips =
            {
                "Ancestral Legendary Sword (Slashing)\n900 Item Power",
                "Ancestral Legendary Mace (Bludgeoning)\n900 Item Power",
                "Ancestral Legendary Two-Handed Sword (Slashing)\n900 Item Power"
            };

            foreach (string tooltip in tooltips)
            {
                string typeId = _ocrHandler.ConvertToItemType(tooltip).TypeId;

                Assert.That(typeId, Is.Not.EqualTo(ItemTypeConstants.WeaponMainhand), tooltip);
                Assert.That(typeId, Is.Not.EqualTo(ItemTypeConstants.WeaponOffhand), tooltip);
            }
        }
    }
}
