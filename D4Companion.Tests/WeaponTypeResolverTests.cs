using D4Companion.Constants;
using D4Companion.Helpers;

namespace D4Companion.Tests
{
    public class WeaponTypeResolverTests
    {
        [TestCase("Ancestral Legendary Two-Handed Mace (Bludgeoning)", ItemTypeConstants.WeaponBludgeoning)]
        [TestCase("Legendary Two-Handed Axe (Slashing)", ItemTypeConstants.WeaponSlicing)]
        [TestCase("Legendary Two-Handed Sword (Slashing)", ItemTypeConstants.WeaponSlicing)]
        // Polearm is two-handed but has no "Two-Handed" prefix in the data.
        [TestCase("Legendary Polearm (Slashing)", ItemTypeConstants.WeaponSlicing)]
        [TestCase("Legendary Mace (Bludgeoning)", ItemTypeConstants.WeaponOneHand)]
        [TestCase("Legendary Sword (Slashing)", ItemTypeConstants.WeaponOneHand)]
        [TestCase("Legendary Axe (Slashing)", ItemTypeConstants.WeaponOneHand)]
        // Unsuffixed names are shown to classes without the Arsenal mechanic.
        [TestCase("Legendary Two-Handed Mace", ItemTypeConstants.Weapon)]
        [TestCase("Legendary Flail", ItemTypeConstants.Weapon)]
        [TestCase("Legendary Quarterstaff", ItemTypeConstants.Weapon)]
        public void FromItemTypeName_MapsExpectedSubtype(string name, string expected)
        {
            Assert.That(WeaponTypeResolver.FromItemTypeName(name), Is.EqualTo(expected));
        }

        [TestCase("2HMace_Legendary_Generic_006", ItemTypeConstants.WeaponBludgeoning)]
        [TestCase("2HPolearm_Legendary_Generic_001", ItemTypeConstants.WeaponSlicing)]
        [TestCase("2HAxe_Legendary_Generic_001", ItemTypeConstants.WeaponSlicing)]
        [TestCase("2HSword_Legendary_Generic_001", ItemTypeConstants.WeaponSlicing)]
        [TestCase("1HSword_Legendary_Generic_001", ItemTypeConstants.WeaponOneHand)]
        [TestCase("1HMace_Legendary_Generic_001", ItemTypeConstants.WeaponOneHand)]
        [TestCase("1HAxe_Legendary_Generic_001", ItemTypeConstants.WeaponOneHand)]
        [TestCase("Chest_Legendary_Generic_053", ItemTypeConstants.Weapon)]
        public void FromMaxrollItemId_MapsExpectedSubtype(string itemId, string expected)
        {
            Assert.That(WeaponTypeResolver.FromMaxrollItemId(itemId), Is.EqualTo(expected));
        }

        [TestCase(ItemTypeConstants.WeaponBludgeoning, true)]
        [TestCase(ItemTypeConstants.WeaponSlicing, true)]
        [TestCase(ItemTypeConstants.WeaponOneHand, true)]
        [TestCase(ItemTypeConstants.Weapon, false)]
        [TestCase(ItemTypeConstants.Helm, false)]
        public void IsWeaponSubtype_IdentifiesSubtypesOnly(string itemType, bool expected)
        {
            Assert.That(WeaponTypeResolver.IsWeaponSubtype(itemType), Is.EqualTo(expected));
        }
    }
}
