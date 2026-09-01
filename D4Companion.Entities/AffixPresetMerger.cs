using System.Windows.Media;

namespace D4Companion.Entities
{
    /// <summary>
    /// The recolour choices offered by the merge dialog: recolour everything that came
    /// from build 1, from build 2, and everything the two builds agree on.
    /// </summary>
    public sealed record AffixPresetMergeColors(
        bool ChangeColorBuild1, Color ColorBuild1,
        bool ChangeColorBuild2, Color ColorBuild2,
        bool ChangeColorBuild12, Color ColorBuild12)
    {
        /// <summary>Recolour nothing - every entry keeps the colour it was imported with.</summary>
        public static AffixPresetMergeColors None { get; } =
            new(false, Colors.Red, false, Colors.Red, false, Colors.Red);
    }

    /// <summary>
    /// Combines two affix presets into one.
    ///
    /// Extracted from the merge dialog because it was silently dropping entries and no
    /// test could reach it inside a WPF view model. It copied affixes, aspects and sigils
    /// only - uniques, runes and transfigurations were left behind entirely - and even the
    /// affixes it did copy lost IsAnyType and Rank.
    ///
    /// Two rules run through every list:
    /// - Stay at least as permissive as either input. A build-wide entry merged with a
    ///   slot-scoped one stays build-wide, or the merged preset silently narrows to one slot.
    /// - Keep the better ranking. Rank 1 is the highest priority, so the lower positive
    ///   number wins, and 0 means unranked rather than best.
    /// </summary>
    public static class AffixPresetMerger
    {
        public static AffixPreset Merge(AffixPreset build1, AffixPreset build2, string name, AffixPresetMergeColors colors)
        {
            var merged = new AffixPreset { Name = name };

            Color FromBuild1(Color color) => colors.ChangeColorBuild1 ? colors.ColorBuild1 : color;
            Color FromBuild2(Color color) => colors.ChangeColorBuild2 ? colors.ColorBuild2 : color;
            Color Shared(Color color) => colors.ChangeColorBuild12 ? colors.ColorBuild12 : color;

            merged.ItemAffixes.AddRange(build1.ItemAffixes.Select(a => new ItemAffix
            {
                Id = a.Id,
                Type = a.Type,
                Color = FromBuild1(a.Color),
                IsGreater = a.IsGreater,
                IsImplicit = a.IsImplicit,
                IsTempered = a.IsTempered,
                IsAnyType = a.IsAnyType,
                Rank = a.Rank
            }));
            merged.ItemAspects.AddRange(build1.ItemAspects.Select(a => new ItemAffix
            {
                Id = a.Id,
                Type = a.Type,
                IsAnyType = a.IsAnyType,
                Color = FromBuild1(a.Color)
            }));
            merged.ItemSigils.AddRange(build1.ItemSigils.Select(s => new ItemAffix
            {
                Id = s.Id,
                Type = s.Type,
                Color = FromBuild1(s.Color)
            }));
            merged.ItemUniques.AddRange(build1.ItemUniques.Select(u => new ItemAffix
            {
                Id = u.Id,
                Type = u.Type,
                IsAnyType = u.IsAnyType,
                Color = FromBuild1(u.Color)
            }));
            merged.ItemRunes.AddRange(build1.ItemRunes.Select(r => new ItemAffix
            {
                Id = r.Id,
                Type = r.Type,
                Color = FromBuild1(r.Color)
            }));
            merged.ItemTransfigurations.AddRange(build1.ItemTransfigurations.Select(t => new ItemAffix
            {
                Id = t.Id,
                Type = t.Type,
                IsAnyType = t.IsAnyType,
                IsTransfigured = true,
                Color = FromBuild1(t.Color)
            }));

            foreach (var affix in build2.ItemAffixes)
            {
                var existing = merged.ItemAffixes.FirstOrDefault(a => a.Id.Equals(affix.Id) && a.Type.Equals(affix.Type));
                if (existing == null)
                {
                    merged.ItemAffixes.Add(new ItemAffix
                    {
                        Id = affix.Id,
                        Type = affix.Type,
                        Color = FromBuild2(affix.Color),
                        IsGreater = affix.IsGreater,
                        IsImplicit = affix.IsImplicit,
                        IsTempered = affix.IsTempered,
                        IsAnyType = affix.IsAnyType,
                        Rank = affix.Rank
                    });
                    continue;
                }

                existing.Color = Shared(existing.Color);
                existing.IsAnyType = existing.IsAnyType || affix.IsAnyType;
                existing.Rank = BestRank(existing.Rank, affix.Rank);
            }

            foreach (var aspect in build2.ItemAspects)
            {
                var existing = merged.ItemAspects.FirstOrDefault(a => a.Id.Equals(aspect.Id) && a.Type.Equals(aspect.Type));
                if (existing == null)
                {
                    merged.ItemAspects.Add(new ItemAffix
                    {
                        Id = aspect.Id,
                        Type = aspect.Type,
                        IsAnyType = aspect.IsAnyType,
                        Color = FromBuild2(aspect.Color)
                    });
                    continue;
                }

                existing.Color = Shared(existing.Color);

                // Sources that cannot report which slot an aspect sits on (D4Builds,
                // Mobalytics) mark it IsAnyType, so the two builds routinely disagree here.
                existing.IsAnyType = existing.IsAnyType || aspect.IsAnyType;
            }

            foreach (var sigil in build2.ItemSigils)
            {
                var existing = merged.ItemSigils.FirstOrDefault(s => s.Id.Equals(sigil.Id) && s.Type.Equals(sigil.Type));
                if (existing == null)
                {
                    merged.ItemSigils.Add(new ItemAffix
                    {
                        Id = sigil.Id,
                        Type = sigil.Type,
                        Color = FromBuild2(sigil.Color)
                    });
                    continue;
                }

                existing.Color = Shared(existing.Color);
            }

            foreach (var unique in build2.ItemUniques)
            {
                var existing = merged.ItemUniques.FirstOrDefault(u => u.Id.Equals(unique.Id) && u.Type.Equals(unique.Type));
                if (existing == null)
                {
                    merged.ItemUniques.Add(new ItemAffix
                    {
                        Id = unique.Id,
                        Type = unique.Type,
                        IsAnyType = unique.IsAnyType,
                        Color = FromBuild2(unique.Color)
                    });
                    continue;
                }

                existing.Color = Shared(existing.Color);
                existing.IsAnyType = existing.IsAnyType || unique.IsAnyType;
            }

            foreach (var rune in build2.ItemRunes)
            {
                // Runes are not slot-scoped - a rune sits in a socket, not on a slot - so
                // this matches on id alone, the way AffixManager.GetRune resolves them.
                var existing = merged.ItemRunes.FirstOrDefault(r => r.Id.Equals(rune.Id));
                if (existing == null)
                {
                    merged.ItemRunes.Add(new ItemAffix
                    {
                        Id = rune.Id,
                        Type = rune.Type,
                        Color = FromBuild2(rune.Color)
                    });
                    continue;
                }

                existing.Color = Shared(existing.Color);
            }

            foreach (var transfiguration in build2.ItemTransfigurations)
            {
                var existing = merged.ItemTransfigurations.FirstOrDefault(t => t.Id.Equals(transfiguration.Id) && t.Type.Equals(transfiguration.Type));
                if (existing == null)
                {
                    merged.ItemTransfigurations.Add(new ItemAffix
                    {
                        Id = transfiguration.Id,
                        Type = transfiguration.Type,
                        IsAnyType = transfiguration.IsAnyType,
                        IsTransfigured = true,
                        Color = FromBuild2(transfiguration.Color)
                    });
                    continue;
                }

                existing.Color = Shared(existing.Color);
                existing.IsAnyType = existing.IsAnyType || transfiguration.IsAnyType;
            }

            return merged;
        }

        /// <summary>
        /// Rank 1 is the highest priority and 0 means unranked, so the lower POSITIVE rank
        /// wins and an unranked entry never beats a ranked one.
        /// </summary>
        private static int BestRank(int left, int right)
        {
            if (left <= 0) return right;
            if (right <= 0) return left;

            return Math.Min(left, right);
        }
    }
}
