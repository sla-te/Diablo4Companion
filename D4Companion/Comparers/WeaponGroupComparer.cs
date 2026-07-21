using D4Companion.Constants;
using D4Companion.Entities;
using System.Collections;

namespace D4Companion.Comparers
{
    /// <summary>
    /// Orders weapon entries by Arsenal group so the weapon panel reads in the order build
    /// guides use: Bludgeoning, Slicing, Mainhand, Offhand. The unspecified one-handed
    /// group follows, then plain "weapon" last - neither is specific to a single Arsenal
    /// slot, so a trailing position keeps the specific groups on top.
    ///
    /// Grouping is what forms the sections; this comparer only decides the order they
    /// appear in, because WPF forms groups in the order it encounters items.
    ///
    /// Within a group the comparer must reproduce BuildPresetProjector.SortAffixes
    /// (implicit first, tempered last, then by stat priority) - CustomSort replaces the
    /// projector's ordering rather than layering on top of it, so anything not restated
    /// here is lost. Note GroupRankOf's "rank" is the Arsenal section, unrelated to
    /// ItemAffix.Rank, which is the build guide's stat priority. The final
    /// Id tie-break exists because ListCollectionView.CustomSort runs an unstable sort:
    /// without a total ordering, entries sharing a group could permute on every refresh.
    /// </summary>
    public class WeaponGroupComparer : IComparer
    {
        public static int GroupRankOf(string? itemType)
        {
            switch (itemType)
            {
                case ItemTypeConstants.WeaponBludgeoning:
                    return 0;
                case ItemTypeConstants.WeaponSlicing:
                    return 1;
                case ItemTypeConstants.WeaponMainhand:
                    return 2;
                case ItemTypeConstants.WeaponOffhand:
                    return 3;
                case ItemTypeConstants.WeaponOneHand:
                    return 4;
                default:
                    return 5;
            }
        }

        public int Compare(object? x, object? y)
        {
            var affixX = x as ItemAffix;
            var affixY = y as ItemAffix;

            int groupComparison = GroupRankOf(affixX?.Type).CompareTo(GroupRankOf(affixY?.Type));
            if (groupComparison != 0) return groupComparison;

            // Distinct types can share a rank (anything falling to the default), so the
            // type still has to separate them into their own groups.
            int typeComparison = string.CompareOrdinal(affixX?.Type, affixY?.Type);
            if (typeComparison != 0) return typeComparison;

            if (affixX?.IsTempered != affixY?.IsTempered) return affixX?.IsTempered == true ? 1 : -1;
            if (affixX?.IsImplicit != affixY?.IsImplicit) return affixX?.IsImplicit == true ? -1 : 1;

            int statRankComparison = ItemAffix.CompareRank(affixX?.Rank ?? 0, affixY?.Rank ?? 0);
            if (statRankComparison != 0) return statRankComparison;

            return string.CompareOrdinal(affixX?.Id, affixY?.Id);
        }
    }
}
