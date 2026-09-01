using System.Text.RegularExpressions;
using D4Companion.Entities;

namespace D4Companion.Services
{
    /// <summary>One transfiguration the guide recommends. Scope is empty when build-wide.</summary>
    public sealed record MaxrollTransfigurationEntry(string Stat, string Scope);

    /// <summary>
    /// Lifts the transfiguration stat list out of a Maxroll guide's equipment notes.
    ///
    /// The recommendations are prose, not planner data: no item in the payload carries a
    /// transfiguration field, so this document is the only place they exist.
    /// </summary>
    public static partial class MaxrollTransfigurationParser
    {
        // "tran[sf]?figur", not "transfigur". The guide's stat-list heading is misspelled
        // "Optimal Tranfigurations". Tightening this regex compiles, runs, and silently
        // returns an empty list.
        [GeneratedRegex(@"tran[sf]?figur", RegexOptions.IgnoreCase)]
        private static partial Regex HeadingPattern();

        // A trailing "(on 2-Handed Weapons)" qualifier. "on" is optional.
        [GeneratedRegex(@"^(?<stat>.*?)\s*\((?:on\s+)?(?<scope>[^)]+)\)\s*$", RegexOptions.IgnoreCase)]
        private static partial Regex ScopePattern();

        public static List<MaxrollTransfigurationEntry> Parse(MaxrollLexicalNodeJson? document)
        {
            var entries = new List<MaxrollTransfigurationEntry>();
            if (document is null) return entries;

            var blocks = Flatten(document.Root ?? document);

            // Take the LAST transfiguration heading, not the first. The first is the prose
            // section ("Amulet Extra Aspect Transfiguration"), which names an aspect rather
            // than a stat; the stat list sits under the second heading.
            int start = blocks.FindLastIndex(b => b.IsHeading && HeadingPattern().IsMatch(b.Text));
            if (start < 0) return entries;

            for (int i = start + 1; i < blocks.Count; i++)
            {
                if (blocks[i].IsHeading) break;

                var match = ScopePattern().Match(blocks[i].Text);
                entries.Add(match.Success
                    ? new MaxrollTransfigurationEntry(
                        match.Groups["stat"].Value.Trim(),
                        match.Groups["scope"].Value.Trim())
                    : new MaxrollTransfigurationEntry(blocks[i].Text, string.Empty));
            }

            return entries;
        }

        private sealed record Block(bool IsHeading, string Text);

        private static List<Block> Flatten(MaxrollLexicalNodeJson node)
        {
            var blocks = new List<Block>();
            Walk(node, blocks);
            return blocks;
        }

        private static void Walk(MaxrollLexicalNodeJson node, List<Block> blocks)
        {
            if (node.Type is "heading" or "paragraph" or "listitem")
            {
                string text = CollectText(node).Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    blocks.Add(new Block(node.Type.Equals("heading"), text));
                }

                return;
            }

            foreach (var child in node.Children)
            {
                Walk(child, blocks);
            }
        }

        private static string CollectText(MaxrollLexicalNodeJson node)
        {
            string text = node.Text;
            foreach (var child in node.Children)
            {
                text += CollectText(child);
            }

            return text;
        }
    }
}
