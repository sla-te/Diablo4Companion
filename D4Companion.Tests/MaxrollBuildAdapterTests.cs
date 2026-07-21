using System.Text.Json;
using D4Companion.Constants;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;
using D4Companion.Services.BuildAdapters;

namespace D4Companion.Tests
{
    public class MaxrollBuildAdapterTests
    {
        private CanonicalVariant _midgame = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string json = File.ReadAllText(@".\Fixtures\ce9zox0y.json");
            var outer = JsonSerializer.Deserialize<MaxrollBuildJson>(json)!;
            var data = JsonSerializer.Deserialize<MaxrollBuildDataJson>(outer.Data)!;
            var build = new MaxrollBuild { Id = outer.Id, Name = outer.Name, Data = data };

            var adapter = new MaxrollBuildAdapter();
            _midgame = adapter.ToCanonical(build).Variants.Single(v => v.Name.Equals("Midgame"));
        }

        [Test]
        public void ToCanonical_EveryItemHasKnownSlot()
        {
            Assert.That(_midgame.Items.All(i => i.SlotIsKnown), Is.True);
            Assert.That(_midgame.Items.All(i => !string.IsNullOrEmpty(i.Slot)), Is.True);
        }

        [Test]
        public void ToCanonical_BootsAspectBindsToBootsOnly()
        {
            // "of Anger Management" is a boots aspect (raw Maxroll Nid 2620618, resolved
            // IdName "S05_BSK_Barbarian_001_x2" - this adapter is pure and stores the raw
            // Nid, not the IdName; resolution is Task 13's job).
            // The original defect made it match on chest. Assert it is present on boots
            // and nowhere else.
            const string angerManagement = "2620618";

            var slots = _midgame.Items
                .Where(i => i.AspectIds.Contains(angerManagement))
                .Select(i => i.Slot)
                .Distinct()
                .ToList();

            Assert.That(slots, Is.EquivalentTo(new[] { ItemTypeConstants.Boots }));
        }

        [Test]
        public void ToCanonical_BludgeoningAndSlicingWeaponsAreDistinct()
        {
            // Heavy Hitting (raw Nid 2557986) sits on the 2H Mace, of Channeling
            // (raw Nid 2574509) on the 2H Polearm.
            var heavyHitting = _midgame.Items.Single(i => i.AspectIds.Contains("2557986"));
            var channeling = _midgame.Items.Single(i => i.AspectIds.Contains("2574509"));

            Assert.Multiple(() =>
            {
                Assert.That(heavyHitting.Slot, Is.EqualTo(ItemTypeConstants.WeaponBludgeoning));
                Assert.That(channeling.Slot, Is.EqualTo(ItemTypeConstants.WeaponSlicing));
            });
        }

        [Test]
        public void ToCanonical_OneHandedWeaponsMergeIntoOneHandSlot()
        {
            // Edgemaster's (raw Nid 578875) is on a 1H sword. Mainhand and offhand are
            // indistinguishable.
            var edgemasters = _midgame.Items.Single(i => i.AspectIds.Contains("578875"));

            Assert.That(edgemasters.Slot, Is.EqualTo(ItemTypeConstants.WeaponOneHand));
        }

        [Test]
        public void ToCanonical_MidgameHasEightDistinctAspects()
        {
            var aspectIds = _midgame.Items.SelectMany(i => i.AspectIds).Distinct().ToList();

            Assert.That(aspectIds, Has.Count.EqualTo(8));
        }
    }
}
