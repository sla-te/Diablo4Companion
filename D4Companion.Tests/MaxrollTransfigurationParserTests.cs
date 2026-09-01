using D4Companion.Entities;
using D4Companion.Services;
using System.Text.Json;

namespace D4Companion.Tests
{
    public class MaxrollTransfigurationParserTests
    {
        private MaxrollBuildDataJson _data = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string json = File.ReadAllText(@".\Fixtures\ce9zox0y.json");
            var outer = JsonSerializer.Deserialize<MaxrollBuildJson>(json)!;
            _data = JsonSerializer.Deserialize<MaxrollBuildDataJson>(outer.Data)!;
        }

        private List<MaxrollTransfigurationEntry> ParseProfile(string name)
            => MaxrollTransfigurationParser.Parse(
                _data.Profiles.Single(p => p.Name.Equals(name)).WidgetNotes?.Equipment);

        [Test]
        public void Endgame_YieldsTheSixOptimalStats()
        {
            var stats = ParseProfile("Endgame").Select(e => e.Stat).ToList();

            Assert.That(stats, Is.EqualTo(new[]
            {
                "% Physical Damage",
                "Cooldown",
                "Critical Strike Chance",
                "Attack Speed",
                "Strength",
                "All Stats"
            }));
        }

        [Test]
        public void MisspelledHeading_IsStillFound()
        {
            // The guide writes "Optimal Tranfigurations", missing the s. A regex of
            // "transfigur" matches only the prose paragraph above it and returns an
            // empty list without failing. This test is the guard against that.
            Assert.That(ParseProfile("Endgame"), Is.Not.Empty);
        }

        [Test]
        public void ParentheticalQualifier_BecomesTheScope()
        {
            var cooldown = ParseProfile("Endgame").Single(e => e.Stat.Equals("Cooldown"));

            Assert.That(cooldown.Scope, Is.EqualTo("2-Handed Weapons"));
        }

        [Test]
        public void UnqualifiedEntry_HasNoScope()
        {
            var strength = ParseProfile("Endgame").Single(e => e.Stat.Equals("Strength"));

            Assert.That(strength.Scope, Is.Empty);
        }

        [Test]
        public void ProfileWithoutNotes_YieldsNothing()
        {
            // 7 of the 8 profiles in this build have no transfiguration section at all.
            // Absent is the normal case, not an error.
            Assert.That(ParseProfile("Starter"), Is.Empty);
        }
    }
}
