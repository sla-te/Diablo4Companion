using D4Companion.Comparers;
using D4Companion.Constants;
using D4Companion.Entities;

namespace D4Companion.Tests
{
    public class WeaponGroupComparerTests
    {
        [Test]
        public void Sorting_OrdersArsenalGroupsAsBuildGuidesPresentThem()
        {
            var affixes = new List<ItemAffix>
            {
                new ItemAffix { Id = "d", Type = ItemTypeConstants.WeaponOffhand },
                new ItemAffix { Id = "a", Type = ItemTypeConstants.WeaponBludgeoning },
                new ItemAffix { Id = "c", Type = ItemTypeConstants.WeaponMainhand },
                new ItemAffix { Id = "b", Type = ItemTypeConstants.WeaponSlicing }
            };

            affixes.Sort((x, y) => new WeaponGroupComparer().Compare(x, y));

            Assert.That(affixes.Select(a => a.Id), Is.EqualTo(new[] { "a", "b", "c", "d" }));
        }

        [Test]
        public void RankOf_PlainWeapon_SortsAfterEveryArsenalGroup()
        {
            // Plain-weapon entries apply to all four groups rather than one, so they belong
            // in a trailing section instead of interleaved among the specific ones.
            int plainRank = WeaponGroupComparer.RankOf(ItemTypeConstants.Weapon);

            Assert.Multiple(() =>
            {
                Assert.That(plainRank, Is.GreaterThan(WeaponGroupComparer.RankOf(ItemTypeConstants.WeaponBludgeoning)));
                Assert.That(plainRank, Is.GreaterThan(WeaponGroupComparer.RankOf(ItemTypeConstants.WeaponSlicing)));
                Assert.That(plainRank, Is.GreaterThan(WeaponGroupComparer.RankOf(ItemTypeConstants.WeaponMainhand)));
                Assert.That(plainRank, Is.GreaterThan(WeaponGroupComparer.RankOf(ItemTypeConstants.WeaponOffhand)));
            });
        }

        [Test]
        public void RankOf_OneHandParent_GetsItsOwnSectionBetweenTheHandsAndPlainWeapon()
        {
            // Presets imported before the hands were split, and D2Core imports today, carry
            // weapon_onehand. It must not share a rank with plain "weapon": grouping is on
            // the exact type, so the two would form separate sections that both rendered
            // under the same caption - two identical headers in a row.
            int oneHandRank = WeaponGroupComparer.RankOf(ItemTypeConstants.WeaponOneHand);

            Assert.Multiple(() =>
            {
                Assert.That(oneHandRank, Is.GreaterThan(WeaponGroupComparer.RankOf(ItemTypeConstants.WeaponOffhand)));
                Assert.That(oneHandRank, Is.LessThan(WeaponGroupComparer.RankOf(ItemTypeConstants.Weapon)));
            });
        }

        [Test]
        public void Sorting_WithinAGroup_ReproducesProjectorOrderImplicitFirstTemperedLast()
        {
            // CustomSort replaces BuildPresetProjector.SortAffixes for this panel rather
            // than layering on it, so the projector's ordering has to be restated here or
            // the weapon panel silently orders differently from every other slot panel.
            var affixes = new List<ItemAffix>
            {
                new ItemAffix { Id = "tempered", Type = ItemTypeConstants.WeaponMainhand, IsTempered = true },
                new ItemAffix { Id = "normal", Type = ItemTypeConstants.WeaponMainhand },
                new ItemAffix { Id = "implicit", Type = ItemTypeConstants.WeaponMainhand, IsImplicit = true }
            };

            affixes.Sort((x, y) => new WeaponGroupComparer().Compare(x, y));

            Assert.That(affixes.Select(a => a.Id), Is.EqualTo(new[] { "implicit", "normal", "tempered" }));
        }

        [Test]
        public void Sorting_IdenticalGroupAndFlags_IsTotalSoOrderCannotPermute()
        {
            // ListCollectionView.CustomSort runs an unstable sort. Without a total ordering,
            // entries alike in group and flags could swap places on any refresh.
            var a = new ItemAffix { Id = "aaa", Type = ItemTypeConstants.WeaponOffhand };
            var b = new ItemAffix { Id = "bbb", Type = ItemTypeConstants.WeaponOffhand };

            var comparer = new WeaponGroupComparer();

            Assert.Multiple(() =>
            {
                Assert.That(comparer.Compare(a, b), Is.LessThan(0));
                Assert.That(comparer.Compare(b, a), Is.GreaterThan(0));
                Assert.That(comparer.Compare(a, a), Is.Zero);
            });
        }
    }
}
