using D4Companion.Entities;
using System.Text.Json;

namespace D4Companion.Tests
{
    /// <summary>
    /// Maxroll's per-item "explicits" list is a ranked stat-priority list, not the four
    /// affixes an item can physically roll, and each entry may carry an arrow count meaning
    /// "aim for a Greater Affix here". Both were being discarded on import.
    /// </summary>
    public class MaxrollStatPriorityTests
    {
        private MaxrollBuildDataJson _data = null!;
        private Dictionary<int, int> _midgameSlots = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string json = File.ReadAllText(@".\Fixtures\ce9zox0y.json");
            var outer = JsonSerializer.Deserialize<MaxrollBuildJson>(json)!;
            _data = JsonSerializer.Deserialize<MaxrollBuildDataJson>(outer.Data)!;
            _midgameSlots = _data.Profiles.Single(p => p.Name.Equals("Midgame")).Items;
        }

        private MaxrollBuildDataItemJson ItemInSlot(int slotId) => _data.Items[_midgameSlots[slotId]];

        [Test]
        public void Amulet_ListsMoreStatsThanAnItemCanRoll()
        {
            // Slot 18. The guide's amulet panel numbers seven stats; an item holds four
            // plus a temper. Truncating to four is what left Strength (rank 6) unmatched.
            Assert.That(ItemInSlot(18).Explicits, Has.Count.EqualTo(7));
        }

        [Test]
        public void ExplicitOrder_IsThePriorityRanking()
        {
            // The guide lists the amulet as 1. Critical Strike Chance, ... 6. Strength,
            // 7. Maximum Life. Array position carries that ranking - there is no separate
            // rank field - so anything reordering this list loses the priority.
            var nids = ItemInSlot(18).Explicits.Select(e => e.Nid).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(nids[0], Is.EqualTo(1829582), "rank 1 should be Critical Strike Chance");
                Assert.That(nids[5], Is.EqualTo(2602159), "rank 6 should be Strength");
                Assert.That(nids[6], Is.EqualTo(2605602), "rank 7 should be Maximum Life");
            });
        }

        [TestCase(18, 0, TestName = "Amulet rank 1 shows two arrows")]
        [TestCase(8, 0, TestName = "Bludgeoning weapon rank 1 shows three arrows")]
        public void UpgradePriority_MarksAStatAsGreater(int slotId, int explicitIndex)
        {
            Assert.That(ItemInSlot(slotId).Explicits[explicitIndex].IsGreaterAffix, Is.True);
        }

        [Test]
        public void UpgradeFieldAlone_DoesNotMarkAStatAsGreater()
        {
            // Gloves rank 1 (Cooldown Reduction) carries "upgrade": 1 but no
            // upgradePriority, and the guide draws no arrows beside it. Treating "upgrade"
            // as the greater flag would wrongly promote it.
            var glovesFirst = ItemInSlot(13).Explicits[0];

            Assert.Multiple(() =>
            {
                Assert.That(glovesFirst.UpgradePriority, Is.Null);
                Assert.That(glovesFirst.IsGreaterAffix, Is.False);
            });
        }

        [Test]
        public void GlovesAspectEntry_IsLastSoEarlierRanksSurviveIt()
        {
            // The gloves are a unique and their seventh explicit (sno 2577891) is the item's
            // own aspect, not an affix - it resolves to nothing and is skipped on import.
            // Ranks come from the loop index rather than a counter over resolved affixes, so
            // this entry must not shift the ranks of the six real stats above it.
            var gloves = ItemInSlot(13);

            Assert.Multiple(() =>
            {
                Assert.That(gloves.Explicits, Has.Count.EqualTo(7));
                Assert.That(gloves.Explicits[6].Nid, Is.EqualTo(2577891));
            });
        }

        [Test]
        public void GreaterKeyAlone_StillMarksAStatAsGreater()
        {
            // Maxroll emits two independent markers. "greater" is a flat yes/no, and it is
            // what the Endgame helm's second stat carries with no arrow count beside it.
            // Reading only upgradePriority would drop every stat flagged this way.
            var endgameSlots = _data.Profiles.Single(p => p.Name.Equals("Endgame")).Items;
            var helmSecond = _data.Items[endgameSlots[4]].Explicits[1];

            Assert.Multiple(() =>
            {
                Assert.That(helmSecond.Greater, Is.True);
                Assert.That(helmSecond.UpgradePriority, Is.Null);
                Assert.That(helmSecond.IsGreaterAffix, Is.True);
            });
        }

        [Test]
        public void BothMarkers_AreInUseAndOverlapOnlyPartly()
        {
            // Neither key subsumes the other: 125 explicits in this build carry only
            // "greater", 37 only "upgradePriority", 40 both. That is why IsGreaterAffix ORs
            // them rather than picking one - the Midgame variant happens to use only
            // upgradePriority, so testing against Midgame alone would hide the greater path.
            var explicits = _data.Items.Values.SelectMany(i => i.Explicits).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(explicits.Any(e => e.Greater && e.UpgradePriority == null), Is.True, "greater-only");
                Assert.That(explicits.Any(e => !e.Greater && e.UpgradePriority > 0), Is.True, "upgradePriority-only");
                Assert.That(explicits.Any(e => e.Greater && e.UpgradePriority > 0), Is.True, "both");
            });
        }
    }
}
