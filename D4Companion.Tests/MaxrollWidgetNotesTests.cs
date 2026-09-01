using D4Companion.Entities;
using System.Text.Json;

namespace D4Companion.Tests
{
    /// <summary>
    /// Maxroll keeps transfiguration recommendations in guide prose, not in the item
    /// schema. They live in profiles[N].widgetNotes.equipment as a Lexical document.
    /// </summary>
    public class MaxrollWidgetNotesTests
    {
        private MaxrollBuildDataJson _data = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string json = File.ReadAllText(@".\Fixtures\ce9zox0y.json");
            var outer = JsonSerializer.Deserialize<MaxrollBuildJson>(json)!;
            _data = JsonSerializer.Deserialize<MaxrollBuildDataJson>(outer.Data)!;
        }

        private MaxrollBuildDataProfileJson Profile(string name)
            => _data.Profiles.Single(p => p.Name.Equals(name));

        [Test]
        public void EndgameProfile_CarriesAnEquipmentNotesDocument()
        {
            Assert.That(Profile("Endgame").WidgetNotes?.Equipment?.Root, Is.Not.Null);
        }

        [Test]
        public void EquipmentDocument_HasChildNodes()
        {
            Assert.That(Profile("Endgame").WidgetNotes!.Equipment!.Root!.Children,
                Is.Not.Empty);
        }
    }
}
