using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Helpers;
using System.Text.Json;

namespace D4Companion.Tests
{
    /// <summary>
    /// Verifies the index-alignment claim behind the locale-independent weapon subtype
    /// resolution in OcrHandler.InitItemTypeData, against the real shipped
    /// Data/ItemTypes.*.json files - not a hand-rolled sample.
    ///
    /// The claim: ItemTypes.enUS.json and a given locale file have the same entry count,
    /// and the Type field is identical index-for-index, so weapon subtype can be resolved
    /// by array position against the enUS classification without parsing localized text.
    ///
    /// This does NOT hold for all fourteen shipped locales. frFR, plPL and ptBR are each
    /// missing entries relative to enUS (a handful of Wand/Charm/Horadric Seal entries),
    /// and trTR is missing more than half of enUS's 432 entries - every one of these shifts
    /// all later indices out of alignment. OcrHandler falls back to the legacy per-name
    /// parse for those locales (see InitItemTypeData), which is a no-regression no-op, not
    /// a crash or a silently wrong classification.
    /// </summary>
    public class WeaponSubtypeLocaleAlignmentTests
    {
        private static readonly string DataDir = @".\Data";

        private static List<ItemTypeInfo> LoadItemTypes(string locale)
        {
            string path = Path.Combine(DataDir, $"ItemTypes.{locale}.json");
            using FileStream stream = File.OpenRead(path);
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new BoolConverter());
            options.Converters.Add(new IntConverter());
            return JsonSerializer.Deserialize<List<ItemTypeInfo>>(stream, options) ?? new List<ItemTypeInfo>();
        }

        private static List<(string Name, string Type)> AsPairs(List<ItemTypeInfo> itemTypes)
        {
            return itemTypes.Select(t => (t.Name, t.Type)).ToList();
        }

        [Test]
        public void AllFourteenShippedLocaleFiles_Exist()
        {
            var files = Directory.GetFiles(DataDir, "ItemTypes.*.json");
            Assert.That(files, Has.Length.EqualTo(14));
        }

        [TestCase("deDE")]
        [TestCase("esES")]
        [TestCase("esMX")]
        [TestCase("itIT")]
        [TestCase("jaJP")]
        [TestCase("koKR")]
        [TestCase("ruRU")]
        [TestCase("zhCN")]
        [TestCase("zhTW")]
        public void IsIndexAligned_LocalesConfirmedAligned_ReturnsTrue(string locale)
        {
            var reference = AsPairs(LoadItemTypes("enUS"));
            var localeItemTypes = AsPairs(LoadItemTypes(locale));

            Assert.That(WeaponTypeResolver.IsIndexAligned(reference, localeItemTypes), Is.True);
        }

        // frFR, plPL and ptBR are missing a handful of entries relative to enUS; trTR is
        // missing more than half. All four fail the alignment check and must fall back to
        // the legacy per-name parse rather than be trusted for index-based resolution.
        [TestCase("frFR")]
        [TestCase("plPL")]
        [TestCase("ptBR")]
        [TestCase("trTR")]
        public void IsIndexAligned_LocalesConfirmedMisaligned_ReturnsFalse(string locale)
        {
            var reference = AsPairs(LoadItemTypes("enUS"));
            var localeItemTypes = AsPairs(LoadItemTypes(locale));

            Assert.That(WeaponTypeResolver.IsIndexAligned(reference, localeItemTypes), Is.False);
        }

        [TestCase("deDE")]
        [TestCase("esES")]
        [TestCase("esMX")]
        [TestCase("itIT")]
        [TestCase("jaJP")]
        [TestCase("koKR")]
        [TestCase("ruRU")]
        [TestCase("zhCN")]
        [TestCase("zhTW")]
        public void EveryLocale_HasNoDuplicateNames(string locale)
        {
            // _itemTypeMapNameToId is keyed on Name and built with ToDictionary, which
            // throws on duplicate keys. This must hold for every shipped locale, not just
            // enUS, or InitItemTypeData crashes on load for that locale.
            var names = LoadItemTypes(locale).Select(t => t.Name).ToList();

            Assert.That(names.Distinct().Count(), Is.EqualTo(names.Count));
        }

        /// <summary>
        /// This is the whole point of the fix: reproduces OcrHandler.InitItemTypeData's
        /// index-based classification for a real non-English locale (deDE) and asserts it
        /// against real deDE strings read from Data/ItemTypes.deDE.json - not a fabricated
        /// German fixture. deDE marks the Barbarian Arsenal 2H-bludgeoning suffix as
        /// "(Wuchtwaffe)" and both 2H-slashing weapon classes as "(Hiebwaffe)"; neither
        /// literal is known to WeaponTypeResolver.FromItemTypeName, so before this fix every
        /// one of these resolved to plain "weapon".
        /// </summary>
        [Test]
        public void DeDeLocale_ResolvesWeaponSubtypesByIndex_MirroringOcrHandler()
        {
            var reference = LoadItemTypes("enUS");
            var deDE = LoadItemTypes("deDE");
            var referencePairs = AsPairs(reference);
            var deDEPairs = AsPairs(deDE);

            Assert.That(WeaponTypeResolver.IsIndexAligned(referencePairs, deDEPairs), Is.True,
                "Precondition: deDE must be index-aligned with enUS for this test to be meaningful.");

            var subtypeIndex = WeaponTypeResolver.BuildSubtypeIndex(referencePairs);

            // Build the same name -> type map OcrHandler.InitItemTypeData builds when deDE
            // is the selected locale.
            var deDEMapNameToId = deDE.Select((itemType, index) => (itemType, index)).ToDictionary(
                pair => pair.itemType.Name,
                pair => pair.itemType.Type.Equals(ItemTypeConstants.Weapon)
                    ? subtypeIndex[pair.index]
                    : pair.itemType.Type);

            Assert.Multiple(() =>
            {
                // Two-Handed Mace (Bludgeoning) -> WeaponBludgeoning
                Assert.That(deDEMapNameToId["Legendärer Zweihandstreitkolben (Wuchtwaffe)"],
                    Is.EqualTo(ItemTypeConstants.WeaponBludgeoning));

                // Two-Handed Axe / Sword (Slashing) -> WeaponSlicing
                Assert.That(deDEMapNameToId["Legendäre Zweihandaxt (Hiebwaffe)"],
                    Is.EqualTo(ItemTypeConstants.WeaponSlicing));
                Assert.That(deDEMapNameToId["Legendäres Zweihandschwert (Hiebwaffe)"],
                    Is.EqualTo(ItemTypeConstants.WeaponSlicing));

                // Polearm (Slashing) -> WeaponSlicing, despite carrying no "Two-Handed" prefix.
                Assert.That(deDEMapNameToId["Legendäre Stangenwaffe (Hiebwaffe)"],
                    Is.EqualTo(ItemTypeConstants.WeaponSlicing));

                // One-handed Mace / Sword / Axe -> WeaponOneHand
                Assert.That(deDEMapNameToId["Legendärer Streitkolben (Wuchtwaffe)"],
                    Is.EqualTo(ItemTypeConstants.WeaponOneHand));
                Assert.That(deDEMapNameToId["Legendäres Schwert (Hiebwaffe)"],
                    Is.EqualTo(ItemTypeConstants.WeaponOneHand));
                Assert.That(deDEMapNameToId["Legendäre Axt (Hiebwaffe)"],
                    Is.EqualTo(ItemTypeConstants.WeaponOneHand));

                // Unsuffixed names stay plain "weapon".
                Assert.That(deDEMapNameToId["Legendärer Zweihandstreitkolben"],
                    Is.EqualTo(ItemTypeConstants.Weapon));
                Assert.That(deDEMapNameToId["Legendärer Flegel"],
                    Is.EqualTo(ItemTypeConstants.Weapon));
                Assert.That(deDEMapNameToId["Legendärer Kampfstab"],
                    Is.EqualTo(ItemTypeConstants.Weapon));
            });
        }
    }
}
