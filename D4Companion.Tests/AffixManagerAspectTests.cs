using D4Companion.Constants;
using D4Companion.Services;

namespace D4Companion.Tests
{
    public class AffixManagerAspectTests
    {
        [Test]
        public void IsTypeMatch_ExactSlot_Matches()
        {
            Assert.That(AffixManager.IsTypeMatch(ItemTypeConstants.Boots, ItemTypeConstants.Boots), Is.True);
        }

        [Test]
        public void IsTypeMatch_DifferentSlot_DoesNotMatch()
        {
            Assert.That(AffixManager.IsTypeMatch(ItemTypeConstants.Boots, ItemTypeConstants.Chest), Is.False);
        }

        [TestCase(ItemTypeConstants.WeaponBludgeoning)]
        [TestCase(ItemTypeConstants.WeaponSlicing)]
        [TestCase(ItemTypeConstants.WeaponOneHand)]
        public void IsTypeMatch_WeaponPresetEntry_MatchesAnySubtypeItem(string itemType)
        {
            // Existing presets are typed "weapon" and must keep working.
            Assert.That(AffixManager.IsTypeMatch(ItemTypeConstants.Weapon, itemType), Is.True);
        }

        [TestCase(ItemTypeConstants.WeaponBludgeoning)]
        [TestCase(ItemTypeConstants.WeaponSlicing)]
        [TestCase(ItemTypeConstants.WeaponOneHand)]
        public void IsTypeMatch_SubtypePresetEntry_MatchesPlainWeaponItem(string presetType)
        {
            // Matching is SYMMETRIC across the weapon supertype, and this is load-bearing
            // in two cases that are not obvious:
            //
            // 1. Non-Barbarian classes. The Arsenal damage-type suffix only appears on
            //    Barbarian tooltips, so a Druid's two-handed mace resolves to plain
            //    "weapon" while its Maxroll item id still resolves to weapon_bludgeoning.
            // 2. Non-English locales. FromItemTypeName matches English literals, so it
            //    yields plain "weapon" for all 13 other shipped locales.
            //
            // Without this direction, both groups would silently stop matching weapon
            // entries entirely - a regression against current behaviour.
            Assert.That(AffixManager.IsTypeMatch(presetType, ItemTypeConstants.Weapon), Is.True);
        }

        [Test]
        public void IsTypeMatch_DifferentSubtypes_DoNotMatch()
        {
            // This is the only weapon pairing that must fail, and it is the whole point
            // of the split: a Bludgeoning aspect must not highlight on a Slicing weapon.
            Assert.That(
                AffixManager.IsTypeMatch(ItemTypeConstants.WeaponBludgeoning, ItemTypeConstants.WeaponSlicing),
                Is.False);
            Assert.That(
                AffixManager.IsTypeMatch(ItemTypeConstants.WeaponSlicing, ItemTypeConstants.WeaponOneHand),
                Is.False);
        }
    }
}
