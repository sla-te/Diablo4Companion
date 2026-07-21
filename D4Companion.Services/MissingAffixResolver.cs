using D4Companion.Constants;
using D4Companion.Entities;
using System.Windows.Media;

namespace D4Companion.Services
{
    /// <summary>
    /// Decides which of the build's wanted stats an item does not have.
    ///
    /// The overlay markers can only speak about rows the tooltip actually carries, so until
    /// this existed nothing said anything about a stat that is simply absent - which is the
    /// case that decides whether an item is worth keeping.
    ///
    /// Separate from OverlayHandler so the rule can be asserted without a live overlay
    /// surface, the same reason OverlayMarkResolver was split out.
    /// </summary>
    public static class MissingAffixResolver
    {
        /// <summary>
        /// Wanted stats for <paramref name="itemType"/> that are not among
        /// <paramref name="matchedAffixIds"/>, ordered by the build's own priority.
        /// </summary>
        public static List<ItemAffix> Resolve(AffixPreset? preset, string itemType, IEnumerable<string> matchedAffixIds)
        {
            if (preset == null || string.IsNullOrWhiteSpace(itemType)) return new List<ItemAffix>();

            // Sigils and runes are picked from a catalogue rather than rolled: a preset lists
            // many and an item carries one, so every other entry would read as missing. That
            // is noise, not information.
            if (itemType.Equals(ItemTypeConstants.Sigil) || itemType.Equals(ItemTypeConstants.Rune)) return new List<ItemAffix>();

            var matched = matchedAffixIds.ToHashSet(StringComparer.Ordinal);

            return preset.ItemAffixes
                // An implicit comes with the item type rather than being rolled, so the item
                // cannot be missing one. Red is how a preset marks a stat it does not want,
                // which is the opposite of missing.
                .Where(affix => !affix.IsImplicit && !affix.Color.Equals(Colors.Red))
                // The same rule that decided which preset entries could match this tooltip at
                // all. Comparing Type strings directly would report every weapon stat as
                // missing from a two-handed mace, because the preset stores those under a
                // subtype.
                .Where(affix => affix.IsAnyType || AffixManager.IsTypeMatch(affix.Type, itemType))
                .Where(affix => !matched.Contains(affix.Id))
                .OrderBy(affix => affix, Comparer<ItemAffix>.Create((x, y) => ItemAffix.CompareRank(x.Rank, y.Rank)))
                .ToList();
        }
    }
}
