using D4Companion.Constants;

namespace D4Companion.Helpers
{
    /// <summary>
    /// Resolves Barbarian Arsenal weapon subtypes.
    ///
    /// Handedness cannot be derived from a "Two-Handed" name prefix: Polearm is a
    /// two-handed slashing weapon and carries no such prefix. Both directions below
    /// therefore key on the weapon class explicitly.
    /// </summary>
    public static class WeaponTypeResolver
    {
        private const string BludgeoningSuffix = "(Bludgeoning)";
        private const string SlashingSuffix = "(Slashing)";

        // Weapon classes that are two-handed despite lacking a "Two-Handed" prefix.
        private static readonly string[] TwoHandedWithoutPrefix = { "Polearm" };

        private static readonly Dictionary<string, string> MaxrollPrefixMap = new()
        {
            { "2HMace", ItemTypeConstants.WeaponBludgeoning },
            { "2HAxe", ItemTypeConstants.WeaponSlicing },
            { "2HSword", ItemTypeConstants.WeaponSlicing },
            { "2HPolearm", ItemTypeConstants.WeaponSlicing },
            { "1HMace", ItemTypeConstants.WeaponOneHand },
            { "1HAxe", ItemTypeConstants.WeaponOneHand },
            { "1HSword", ItemTypeConstants.WeaponOneHand }
        };

        /// <summary>
        /// Maps a matched ItemTypes entry name to an item type constant.
        /// Returns ItemTypeConstants.Weapon when the name carries no damage-type suffix.
        /// </summary>
        public static string FromItemTypeName(string itemTypeName)
        {
            if (string.IsNullOrEmpty(itemTypeName)) return ItemTypeConstants.Weapon;

            bool isBludgeoning = itemTypeName.Contains(BludgeoningSuffix, StringComparison.Ordinal);
            bool isSlashing = itemTypeName.Contains(SlashingSuffix, StringComparison.Ordinal);
            if (!isBludgeoning && !isSlashing) return ItemTypeConstants.Weapon;

            bool isTwoHanded = itemTypeName.Contains("Two-Handed", StringComparison.Ordinal)
                || TwoHandedWithoutPrefix.Any(c => itemTypeName.Contains(c, StringComparison.Ordinal));

            if (!isTwoHanded) return ItemTypeConstants.WeaponOneHand;

            return isBludgeoning ? ItemTypeConstants.WeaponBludgeoning : ItemTypeConstants.WeaponSlicing;
        }

        /// <summary>
        /// Maps a Maxroll item id such as "2HMace_Legendary_Generic_006" to an item type
        /// constant. Returns ItemTypeConstants.Weapon for non-weapon or unknown ids.
        /// </summary>
        public static string FromMaxrollItemId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return ItemTypeConstants.Weapon;

            string prefix = itemId.Split('_')[0];
            return MaxrollPrefixMap.TryGetValue(prefix, out string? itemType)
                ? itemType
                : ItemTypeConstants.Weapon;
        }

        /// <summary>
        /// Refines a one-handed weapon to the Arsenal hand a build site assigned it.
        /// Only <see cref="ItemTypeConstants.WeaponOneHand"/> is refined; two-handed
        /// subtypes and non-weapons pass through untouched, so a build site that puts a
        /// two-hander in a hand slot keeps its damage-type classification.
        /// </summary>
        public static string RefineOneHandToHand(string itemType, bool isMainhand)
        {
            if (!itemType.Equals(ItemTypeConstants.WeaponOneHand, StringComparison.Ordinal)) return itemType;

            return isMainhand ? ItemTypeConstants.WeaponMainhand : ItemTypeConstants.WeaponOffhand;
        }

        /// <summary>
        /// True for the two hand-specific one-handed types. These originate only from
        /// build imports - OCR cannot produce them.
        /// </summary>
        public static bool IsOneHandedHand(string? itemType)
        {
            if (string.IsNullOrEmpty(itemType)) return false;

            return itemType.Equals(ItemTypeConstants.WeaponMainhand, StringComparison.Ordinal)
                || itemType.Equals(ItemTypeConstants.WeaponOffhand, StringComparison.Ordinal);
        }

        public static bool IsWeaponSubtype(string? itemType)
        {
            if (string.IsNullOrEmpty(itemType)) return false;

            return itemType.Equals(ItemTypeConstants.WeaponBludgeoning, StringComparison.Ordinal)
                || itemType.Equals(ItemTypeConstants.WeaponSlicing, StringComparison.Ordinal)
                || itemType.Equals(ItemTypeConstants.WeaponOneHand, StringComparison.Ordinal)
                || IsOneHandedHand(itemType);
        }

        /// <summary>
        /// Builds an index-aligned weapon-subtype classification from an authoritative
        /// reference locale's ItemTypes entries (normally enUS, the only locale whose Name
        /// carries the "(Bludgeoning)"/"(Slashing)" damage-type suffix).
        ///
        /// Position i of the result corresponds to referenceItemTypes[i]. Non-weapon entries
        /// keep their original Type value unchanged.
        /// </summary>
        public static IReadOnlyList<string> BuildSubtypeIndex(IEnumerable<(string Name, string Type)> referenceItemTypes)
        {
            return referenceItemTypes
                .Select(entry => entry.Type.Equals(ItemTypeConstants.Weapon, StringComparison.Ordinal)
                    ? FromItemTypeName(entry.Name)
                    : entry.Type)
                .ToList();
        }

        /// <summary>
        /// Verifies that a locale's ItemTypes list is index-aligned with a reference list:
        /// same entry count, and an identical Type value at every index. When this holds,
        /// BuildSubtypeIndex(reference) can be applied to the locale by array index instead
        /// of parsing the locale's own (possibly non-English) Name text.
        ///
        /// Not every shipped locale satisfies this - some ItemTypes.*.json files are missing
        /// entries relative to enUS, which shifts every later index out of alignment. Callers
        /// must check this per locale rather than assuming it holds universally.
        /// </summary>
        public static bool IsIndexAligned(IReadOnlyList<(string Name, string Type)> reference, IReadOnlyList<(string Name, string Type)> locale)
        {
            if (reference.Count != locale.Count) return false;

            for (int i = 0; i < reference.Count; i++)
            {
                if (!reference[i].Type.Equals(locale[i].Type, StringComparison.Ordinal)) return false;
            }

            return true;
        }
    }
}
