using System.Windows.Media;

namespace D4Companion.Entities
{
    public class ItemAffix
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Color Color { get; set; } = Colors.Green;
        public bool IsAnyType { get; set; } = false;
        /// <summary>
        /// Position of this affix in the source build's stat-priority list for its slot,
        /// 1-based. Zero means the source published no ranking - the flag-only sources and
        /// every preset saved before ranks existed - and must render as no rank at all
        /// rather than as rank 0.
        /// </summary>
        public int Rank { get; set; } = 0;

        /// <summary>
        /// Orders two ranks so that unranked (0) sorts last. A plain numeric compare would
        /// put it first, ahead of the guide's top-priority stat.
        /// </summary>
        public static int CompareRank(int rankX, int rankY)
        {
            if (rankX == rankY) return 0;
            if (rankX == 0) return 1;
            if (rankY == 0) return -1;

            return rankX.CompareTo(rankY);
        }
        public bool IsGreater { get; set; } = false;
        public bool IsImplicit { get; set; } = false;
        public bool IsTempered { get; set; } = false;
    }
}
