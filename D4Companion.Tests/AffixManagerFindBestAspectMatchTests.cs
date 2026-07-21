using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Services;

namespace D4Companion.Tests
{
    /// <summary>
    /// Covers AffixManager.FindBestAspectMatch, the single-source-of-truth helper
    /// extracted out of GetAspect so multi-build mode (ScreenProcessHandler) can apply
    /// the exact same three-step rule without going through the selected-preset lookup
    /// that GetAspect itself performs. No AffixManager instance is needed here - the
    /// helper is static and operates directly on a candidate ItemAspects list, mirroring
    /// the style of AffixManagerAspectTests.cs.
    /// </summary>
    public class AffixManagerFindBestAspectMatchTests
    {
        [Test]
        public void ExactSlotMatch_ReturnsEntryAndExactSlotKind()
        {
            var candidates = new List<ItemAffix>
            {
                new ItemAffix { Id = "aspect_test", Type = ItemTypeConstants.Boots }
            };

            var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", ItemTypeConstants.Boots, out var kind);

            Assert.That(match, Is.Not.Null);
            Assert.That(match, Is.SameAs(candidates[0]));
            Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.ExactSlot));
        }

        [Test]
        public void IsAnyTypeEntry_FallsBackAndReturnsAnyTypeKind()
        {
            // No exact slot entry exists at all; only an IsAnyType entry (D4Builds and
            // Mobalytics presets, and manually added aspects, store aspects this way).
            var candidates = new List<ItemAffix>
            {
                new ItemAffix { Id = "aspect_test", Type = ItemTypeConstants.Weapon, IsAnyType = true }
            };

            var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", ItemTypeConstants.Boots, out var kind);

            Assert.That(match, Is.Not.Null);
            Assert.That(match!.IsAnyType, Is.True);
            Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.AnyType));
        }

        [Test]
        public void OffSlotEntry_IsDetectedAsOffSlotNotExactOrAnyType()
        {
            // The aspect is in the build, but only recorded for a different slot and not
            // marked IsAnyType: an extraction target, not a wearable upgrade here.
            var candidates = new List<ItemAffix>
            {
                new ItemAffix { Id = "aspect_test", Type = ItemTypeConstants.Helm }
            };

            var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", ItemTypeConstants.Boots, out var kind);

            Assert.That(match, Is.Not.Null);
            Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.OffSlot));
        }

        [Test]
        public void NoMatchingId_ReturnsNullAndNoneKind()
        {
            var candidates = new List<ItemAffix>
            {
                new ItemAffix { Id = "aspect_other", Type = ItemTypeConstants.Boots }
            };

            var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", ItemTypeConstants.Boots, out var kind);

            Assert.That(match, Is.Null);
            Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.None));
        }

        [TestCase(ItemTypeConstants.WeaponBludgeoning)]
        [TestCase(ItemTypeConstants.WeaponSlicing)]
        [TestCase(ItemTypeConstants.WeaponOneHand)]
        public void WeaponPresetEntry_MatchesAnySubtypeItem_AsExactSlot(string itemType)
        {
            // Existing presets typed plain "weapon" (the common case pre-Arsenal-split)
            // must keep matching every detected weapon subtype as an exact-slot hit.
            var candidates = new List<ItemAffix>
            {
                new ItemAffix { Id = "aspect_test", Type = ItemTypeConstants.Weapon }
            };

            var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", itemType, out var kind);

            Assert.That(match, Is.Not.Null);
            Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.ExactSlot));
        }

        [TestCase(ItemTypeConstants.WeaponBludgeoning)]
        [TestCase(ItemTypeConstants.WeaponSlicing)]
        [TestCase(ItemTypeConstants.WeaponOneHand)]
        public void SubtypePresetEntry_MatchesPlainWeaponItem_AsExactSlot(string presetType)
        {
            // Symmetric direction: an English Barbarian-authored preset entry typed
            // weapon_bludgeoning must still match a plain "weapon" detection from a
            // non-Barbarian class or a non-English locale.
            var candidates = new List<ItemAffix>
            {
                new ItemAffix { Id = "aspect_test", Type = presetType }
            };

            var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", ItemTypeConstants.Weapon, out var kind);

            Assert.That(match, Is.Not.Null);
            Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.ExactSlot));
        }

        [Test]
        public void DifferentWeaponSubtypes_DoNotMatchAsExactSlot_ButAreDetectedOffSlot()
        {
            // A Bludgeoning aspect must not exact-match a Slicing weapon - that is the
            // entire point of the Arsenal split - but since the entry IS present in the
            // build (just for a different weapon subtype), it still resolves off-slot.
            var candidates = new List<ItemAffix>
            {
                new ItemAffix { Id = "aspect_test", Type = ItemTypeConstants.WeaponBludgeoning }
            };

            var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", ItemTypeConstants.WeaponSlicing, out var kind);

            Assert.That(match, Is.Not.Null);
            Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.OffSlot));
        }

        [Test]
        public void LegacyTenEntryPerSlotPreset_StillResolvesExactSlotForEachSlot()
        {
            // Config/AffixPresets-v2.json has no version field and no migration logic.
            // A pre-existing preset holds ten entries per aspect, one per slot, all
            // IsAnyType false. Every one of those slots must keep resolving exactly as
            // before: an exact-slot match, not a fallback.
            string[] slots =
            {
                ItemTypeConstants.Helm, ItemTypeConstants.Chest, ItemTypeConstants.Gloves,
                ItemTypeConstants.Pants, ItemTypeConstants.Boots, ItemTypeConstants.Amulet,
                ItemTypeConstants.Ring, ItemTypeConstants.Weapon, ItemTypeConstants.Ranged,
                ItemTypeConstants.Offhand
            };
            var candidates = slots
                .Select(slot => new ItemAffix { Id = "aspect_test", Type = slot, IsAnyType = false })
                .ToList();

            foreach (var slot in slots)
            {
                var match = AffixManager.FindBestAspectMatch(candidates, "aspect_test", slot, out var kind);

                Assert.That(match, Is.Not.Null, $"slot {slot} should resolve");
                Assert.That(kind, Is.EqualTo(AffixManager.AspectMatchKind.ExactSlot), $"slot {slot} should be an exact-slot match");
                Assert.That(match!.Type, Is.EqualTo(slot));
            }
        }
    }
}
