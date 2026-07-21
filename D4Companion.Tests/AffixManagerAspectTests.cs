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

        [TestCase(ItemTypeConstants.WeaponMainhand)]
        [TestCase(ItemTypeConstants.WeaponOffhand)]
        public void IsTypeMatch_ArsenalHandPresetEntry_MatchesScannedOneHandedItem(string presetType)
        {
            // Build sites know which Arsenal hand an item occupies; a tooltip does not. OCR
            // therefore resolves every one-hander to the parent weapon_onehand, and both
            // hand-specific preset entries must still match it - otherwise every imported
            // one-handed entry would be permanently unmatchable.
            Assert.That(AffixManager.IsTypeMatch(presetType, ItemTypeConstants.WeaponOneHand), Is.True);
            Assert.That(AffixManager.IsTypeMatch(ItemTypeConstants.WeaponOneHand, presetType), Is.True);
        }

        [TestCase(ItemTypeConstants.WeaponMainhand)]
        [TestCase(ItemTypeConstants.WeaponOffhand)]
        public void IsTypeMatch_ArsenalHandEntry_StillMatchesPlainWeapon(string handType)
        {
            // The hands are weapon subtypes, so the plain-weapon supertype rule must reach
            // them too - same non-Barbarian and non-English-locale argument as above.
            Assert.That(AffixManager.IsTypeMatch(ItemTypeConstants.Weapon, handType), Is.True);
            Assert.That(AffixManager.IsTypeMatch(handType, ItemTypeConstants.Weapon), Is.True);
        }

        [Test]
        public void IsTypeMatch_MainhandAndOffhand_DoNotMatchEachOther()
        {
            // Scanning never produces this pairing - both sides only ever meet through the
            // weapon_onehand parent - but the UI counts and groups entries per hand, and
            // collapsing the two here would merge those sections back together.
            Assert.That(
                AffixManager.IsTypeMatch(ItemTypeConstants.WeaponMainhand, ItemTypeConstants.WeaponOffhand),
                Is.False);
            Assert.That(
                AffixManager.IsTypeMatch(ItemTypeConstants.WeaponOffhand, ItemTypeConstants.WeaponMainhand),
                Is.False);
        }

        [TestCase(ItemTypeConstants.WeaponBludgeoning)]
        [TestCase(ItemTypeConstants.WeaponSlicing)]
        public void IsTypeMatch_ArsenalHandEntry_DoesNotMatchTwoHandedSubtype(string twoHandedType)
        {
            // A one-handed entry must not highlight on a two-hander. This is the same
            // separation the Bludgeoning/Slicing split enforces, one level down.
            Assert.That(AffixManager.IsTypeMatch(ItemTypeConstants.WeaponMainhand, twoHandedType), Is.False);
            Assert.That(AffixManager.IsTypeMatch(twoHandedType, ItemTypeConstants.WeaponOffhand), Is.False);
        }
    }
}
