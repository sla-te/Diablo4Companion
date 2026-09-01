using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Services;
using Emgu.CV;
using Emgu.CV.Structure;

namespace D4Companion.Tests
{
    /// <summary>
    /// An eval over real in-game marker columns: it runs the shipped glyph templates through
    /// the app's own matcher and asserts which glyph is recognised on which row.
    ///
    /// Why it exists: three items in a set of twelve captures showed no overlay mark on rows the
    /// build clearly wanted. The cause was not the matching logic - those rows were never
    /// detected at all, because the game draws a second, one-pixel-larger Greater Affix star that
    /// the shipped dot-affixes_greater.png scores at 0.0809 against a 0.05 threshold. Nothing in
    /// the app reports a marker it failed to find, so the gap was invisible until measured.
    ///
    /// The fixtures are the marker column exactly as ScreenProcessHandler sees it: the same raw
    /// pixels, binarised here by the same inverted threshold. The matcher and the classifier are
    /// the production ones, not copies - a reimplementation would keep passing while the shipped
    /// code failed, which is the whole failure this is meant to catch.
    ///
    /// The templates are committed copies of the system preset's. The preset is downloaded at
    /// runtime and overwrites itself, so a template can only be pinned here, not in the app
    /// folder. If a preset update changes a glyph, this eval keeps testing the old one - it
    /// guards the recognition logic, not the currency of the download.
    /// </summary>
    public class AffixMarkerRecognitionTests
    {
        private readonly SettingsD4 _settings = new SettingsD4();
        private Dictionary<string, Image<Gray, byte>> _markers = new Dictionary<string, Image<Gray, byte>>();

        private static string FixturePath(string fileName)
            => Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Markers", fileName);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Mirrors LoadTemplateMatchingImageDirectory: every dot-affixes_* file in the preset,
            // each binarised on load. Loading the whole set rather than the few a fixture needs is
            // deliberate - it is what proves no other glyph steals a row.
            foreach (string file in Directory.GetFiles(FixturePath(string.Empty), "dot-affixes_*.png"))
            {
                var image = new Image<Gray, byte>(file)
                    .ThresholdBinaryInv(new Gray(_settings.ThresholdMin), new Gray(_settings.ThresholdMax));

                _markers.Add(Path.GetFileName(file).ToLower(), image);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            foreach (var marker in _markers.Values) marker.Dispose();
        }

        /// <summary>
        /// Runs every marker template over the fixture the way FindItemAffixLocations does, and
        /// returns what it found in top-to-bottom order.
        /// </summary>
        private List<(string Marker, string AffixType, int Y)> Recognise(string fixtureName)
        {
            using var source = new Image<Bgr, byte>(FixturePath(fixtureName));
            using var filtered = source.Convert<Gray, byte>()
                .ThresholdBinaryInv(new Gray(_settings.ThresholdMin), new Gray(_settings.ThresholdMax));

            var found = new List<ItemAffixLocationDescriptor>();
            foreach (var marker in _markers)
            {
                // FindAffixMarkers blanks each hit as it goes, so every template needs its own copy.
                using var scratch = filtered.Clone();
                found.AddRange(ScreenProcessHandler.FindAffixMarkers(scratch, marker.Key, marker.Value,
                    _settings.ThresholdSimilarityAffixLocation, ScreenProcessHandler.MaxAffixMarkersPerTooltip, out bool aborted));

                Assert.That(aborted, Is.False, $"{marker.Key} hit the iteration cap on {fixtureName}");
            }

            return found
                .OrderBy(location => location.Location.Y)
                .Select(location => (location.Name, ScreenProcessHandler.ClassifyAffixMarker(location.Name), location.Location.Y))
                .ToList();
        }

        [Test]
        public void RingColumn_RecognisesEveryGlyphKindOnOneItem()
        {
            // Six markers, five distinct glyphs, one item. Two of them earn their place here:
            //
            // y=248 is the larger Greater Affix star. It sits on the same item as y=133, which
            // dot-affixes_greater.png does match - which is what makes the two templates provably
            // complementary rather than one being a looser copy of the other.
            //
            // y=207 is the transfigured marker. Measured against an amulet it matches nothing, so
            // it looked like a template of the item-level header badge and its classifier arm
            // looked like dead code. It is neither: on an item that actually carries a
            // transfigured affix it scores 0.011.
            var recognised = Recognise("markers-ring.png");

            Assert.That(recognised.Select(r => (r.Marker, r.AffixType, r.Y)), Is.EqualTo(new[]
            {
                ("dot-affixes_normal.png", AffixTypeConstants.Normal, 28),
                ("dot-affixes_reroll.png", AffixTypeConstants.Normal, 67),
                ("dot-affixes_greater.png", AffixTypeConstants.Greater, 133),
                // A greater affix that was then masterworked draws its own combined glyph.
                ("dot-affixes_greater_master.png", AffixTypeConstants.Greater, 171),
                ("dot-affixes_transfiguring.png", AffixTypeConstants.Transfigured, 207),
                ("dot-affixes_greater_large.png", AffixTypeConstants.Greater, 248)
            }));
        }

        [Test]
        public void PantsColumn_RecognisesEveryMarkerOnTheItem()
        {
            // Five markers, four kinds of glyph. Before dot-affixes_greater_large.png existed the
            // bottom row was silently dropped and this item detected four affixes, not five.
            var recognised = Recognise("markers-pants.png");

            Assert.That(recognised.Select(r => (r.Marker, r.AffixType, r.Y)), Is.EqualTo(new[]
            {
                // Masterworking is not in the classifier's prefix chain, so it falls through to
                // Normal. That is correct: the glyph marks a normal affix that got masterworked.
                ("dot-affixes_masterworking.png", AffixTypeConstants.Normal, 24),
                ("dot-affixes_normal.png", AffixTypeConstants.Normal, 67),
                ("dot-affixes_reroll.png", AffixTypeConstants.Normal, 105),
                ("dot-affixes_normal.png", AffixTypeConstants.Normal, 144),
                ("dot-affixes_greater_large.png", AffixTypeConstants.Greater, 178)
            }));
        }

        [Test]
        public void TheLargeStar_IsOutOfReachOfTheSmallStarTemplate()
        {
            // The reason a second template exists rather than a looser threshold. Run wide enough
            // to see the near miss: dot-affixes_greater.png lands just past the 0.05 cutoff on this
            // row, so the app found nothing and drew nothing, with no error anywhere.
            using var source = new Image<Bgr, byte>(FixturePath("markers-pants.png"));
            using var filtered = source.Convert<Gray, byte>()
                .ThresholdBinaryInv(new Gray(_settings.ThresholdMin), new Gray(_settings.ThresholdMax));

            var nearMisses = ScreenProcessHandler.FindAffixMarkers(filtered, "dot-affixes_greater.png",
                _markers["dot-affixes_greater.png"], 0.2, ScreenProcessHandler.MaxAffixMarkersPerTooltip, out _);

            Assert.That(nearMisses, Is.Not.Empty, "the small-star template should still see the row, just not well enough");
            Assert.That(nearMisses[0].Similarity, Is.GreaterThan(_settings.ThresholdSimilarityAffixLocation)
                .And.LessThan(0.1), "a near miss, not a different glyph - which is why raising the threshold was the tempting fix");
        }

        [TestCase("dot-affixes_transfiguring.png", AffixTypeConstants.Transfigured)]
        [TestCase("dot-affixes_normal.png", AffixTypeConstants.Normal)]
        [TestCase("dot-affixes_reroll.png", AffixTypeConstants.Normal)]
        [TestCase("dot-affixes_greater.png", AffixTypeConstants.Greater)]
        [TestCase("dot-affixes_greater_large.png", AffixTypeConstants.Greater)]
        [TestCase("dot-affixes_greater_master.png", AffixTypeConstants.Greater)]
        [TestCase("dot-affixes_temper_offensive.png", AffixTypeConstants.Tempered)]
        [TestCase("dot-affixes_rune_ritual.png", AffixTypeConstants.Rune)]
        [TestCase("dot-affixes_masterworking.png", AffixTypeConstants.Normal)]
        public void EveryShippedGlyph_ClassifiesToItsAffixType(string markerName, string expected)
        {
            // The loader globs the preset folder, so a new glyph file is a new marker with no code
            // change - but only if its name starts with the prefix of the type it belongs to.
            Assert.That(ScreenProcessHandler.ClassifyAffixMarker(markerName), Is.EqualTo(expected));
        }

        [Test]
        public void EveryTemplateInTheFixtureSet_IsCoveredByTheClassifier()
        {
            // Guards the fall-through arm from becoming a dumping ground: anything that reaches it
            // is reported as a plain affix, which is silent and wrong for a new marker kind.
            var unclassified = _markers.Keys
                .Where(name => !name.StartsWith("dot-affixes_masterworking"))
                .Where(name => ScreenProcessHandler.ClassifyAffixMarker(name) == AffixTypeConstants.Normal)
                .Where(name => !name.StartsWith("dot-affixes_normal") && !name.StartsWith("dot-affixes_reroll"))
                .ToList();

            Assert.That(unclassified, Is.Empty);
        }
    }
}
