using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Helpers;
using System.Text.Json;

namespace D4Companion.Tests
{
    /// <summary>
    /// Exercises the name-to-type dictionary built by OcrHandler.InitItemTypeData against
    /// the real Data/ItemTypes.enUS.json. This mirrors the dictionary-building statement
    /// rather than calling into OcrHandler, so it stays a pure data check with no settings
    /// file or Tesseract data on the path.
    ///
    /// Being a copy, it can drift from the real InitItemTypeData - which has since grown
    /// locale index-alignment handling this does not model. For assertions that must track
    /// the shipped classifier, use OcrHandlerTooltipClassificationTests, which drives the
    /// actual OcrHandler.
    /// </summary>
    public class OcrHandlerItemTypeTests
    {
        private List<ItemTypeInfo> _itemTypes = new List<ItemTypeInfo>();
        private Dictionary<string, string> _itemTypeMapNameToId = new Dictionary<string, string>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            InitItemTypeData();
        }

        private void InitItemTypeData()
        {
            _itemTypes.Clear();
            string resourcePath = @$".\Data\ItemTypes.enUS.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    _itemTypes = JsonSerializer.Deserialize<List<ItemTypeInfo>>(stream, options) ?? new List<ItemTypeInfo>();
                }
            }

            // Mirrors OcrHandler.InitItemTypeData; keep the two in step.
            _itemTypeMapNameToId.Clear();
            _itemTypeMapNameToId = _itemTypes.ToDictionary(
                itemType => itemType.Name,
                itemType => itemType.Type.Equals(ItemTypeConstants.Weapon)
                    ? WeaponTypeResolver.FromItemTypeName(itemType.Name)
                    : itemType.Type);
        }

        [Test]
        public void InitItemTypeData_LoadsRealData()
        {
            // Sanity check that the fixture data loaded at all, so a false-green result
            // from an empty list can't hide behind the assertions below.
            Assert.That(_itemTypes, Is.Not.Empty);
        }

        [Test]
        public void ItemTypeMapNameToId_BuildsWithoutDuplicateNameCollisions()
        {
            // ToDictionary throws on duplicate keys. This asserts the real enUS data set
            // has one entry per Name, so the dictionary construction in OneTimeSetUp above
            // (which already ran) reflects every entry rather than silently dropping some.
            Assert.That(_itemTypeMapNameToId.Count, Is.EqualTo(_itemTypes.Count));
        }

        [TestCase("Legendary Mace (Bludgeoning)", ItemTypeConstants.WeaponOneHand)]
        [TestCase("Legendary Two-Handed Mace (Bludgeoning)", ItemTypeConstants.WeaponBludgeoning)]
        [TestCase("Legendary Axe (Slashing)", ItemTypeConstants.WeaponOneHand)]
        [TestCase("Legendary Two-Handed Axe (Slashing)", ItemTypeConstants.WeaponSlicing)]
        // Polearm is two-handed despite carrying no "Two-Handed" prefix in the data.
        [TestCase("Legendary Polearm (Slashing)", ItemTypeConstants.WeaponSlicing)]
        // Unsuffixed weapon names are shown to classes without the Arsenal mechanic and
        // must keep resolving to the plain "weapon" bucket.
        [TestCase("Legendary Mace", ItemTypeConstants.Weapon)]
        [TestCase("Legendary Axe", ItemTypeConstants.Weapon)]
        public void ItemTypeMapNameToId_ResolvesWeaponSubtypeFromName(string name, string expectedType)
        {
            Assert.That(_itemTypeMapNameToId[name], Is.EqualTo(expectedType));
        }

        [TestCase("Legendary Helm", ItemTypeConstants.Helm)]
        [TestCase("Legendary Chest Armor", ItemTypeConstants.Chest)]
        public void ItemTypeMapNameToId_LeavesNonWeaponTypesUnchanged(string name, string expectedType)
        {
            Assert.That(_itemTypeMapNameToId[name], Is.EqualTo(expectedType));
        }
    }
}
