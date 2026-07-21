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

        public static bool IsWeaponSubtype(string? itemType)
        {
            if (string.IsNullOrEmpty(itemType)) return false;

            return itemType.Equals(ItemTypeConstants.WeaponBludgeoning, StringComparison.Ordinal)
                || itemType.Equals(ItemTypeConstants.WeaponSlicing, StringComparison.Ordinal)
                || itemType.Equals(ItemTypeConstants.WeaponOneHand, StringComparison.Ordinal);
        }
    }
}
