namespace D4Companion.Entities
{
    /// <summary>
    /// Which marker the overlay draws beside a matched affix. The shape is the only signal
    /// besides colour, so anything that needs to be distinguishable in game has to be
    /// expressed here.
    /// </summary>
    public enum OverlayMarkKind
    {
        /// <summary>A dungeon sigil, drawn as a square that can carry the tier number.</summary>
        SigilDungeon,
        /// <summary>An ordinary wanted affix.</summary>
        Circle,
        /// <summary>Set to ignore the item type, or below the minimal affix value.</summary>
        Rectangle,
        /// <summary>The build wants a Greater Affix in this slot.</summary>
        Triangle
    }

    public static class OverlayMarkResolver
    {
        /// <summary>
        /// The single place the marker shape is decided. OverlayHandler draws the same
        /// tooltip in two passes and both used to carry their own copy of this ladder, so a
        /// rule added to one silently did not apply to the other. Keeping the decision here
        /// also lets it be asserted in a test without a live overlay surface.
        /// </summary>
        public static OverlayMarkKind Resolve(ItemAffix affix, bool isDungeonSigil, bool minimalValueFilterEnabled, bool isBelowMinimalValue)
        {
            if (isDungeonSigil) return OverlayMarkKind.SigilDungeon;
            if (affix.IsAnyType) return OverlayMarkKind.Rectangle;
            if (affix.IsGreater) return OverlayMarkKind.Triangle;
            if (minimalValueFilterEnabled && isBelowMinimalValue) return OverlayMarkKind.Rectangle;

            return OverlayMarkKind.Circle;
        }
    }
}
