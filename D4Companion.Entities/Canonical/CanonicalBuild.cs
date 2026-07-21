namespace D4Companion.Entities.Canonical
{
    /// <summary>
    /// Source-independent representation of an imported build.
    ///
    /// Aspects hang off items rather than off the build. That is load-bearing: it makes
    /// the historical "add every aspect to all ten slots" fan-out unrepresentable.
    /// </summary>
    public class CanonicalBuild
    {
        public string Name { get; set; } = string.Empty;
        public List<CanonicalVariant> Variants { get; set; } = new List<CanonicalVariant>();
    }

    public class CanonicalVariant
    {
        public string Name { get; set; } = string.Empty;
        public List<CanonicalItem> Items { get; set; } = new List<CanonicalItem>();
        public List<ParagonBoard> ParagonBoards { get; set; } = new List<ParagonBoard>();
    }

    public class CanonicalItem
    {
        /// <summary>An ItemTypeConstants value. Required.</summary>
        public string Slot { get; set; } = string.Empty;

        /// <summary>
        /// False when the source cannot say which slot this item occupies. D4Builds and
        /// Mobalytics scrape aspects without slot association; they set this false and
        /// the projector emits IsAnyType entries rather than inventing a slot.
        /// </summary>
        public bool SlotIsKnown { get; set; } = true;

        public List<CanonicalAffix> Affixes { get; set; } = new List<CanonicalAffix>();
        public List<string> AspectIds { get; set; } = new List<string>();
        public List<string> UniqueIds { get; set; } = new List<string>();
        public List<string> RuneIds { get; set; } = new List<string>();
    }

    public class CanonicalAffix
    {
        public string Id { get; set; } = string.Empty;
        public bool IsGreater { get; set; } = false;
        public bool IsImplicit { get; set; } = false;
        public bool IsTempered { get; set; } = false;
    }
}
