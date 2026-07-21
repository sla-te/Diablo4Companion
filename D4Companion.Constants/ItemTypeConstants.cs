namespace D4Companion.Constants
{
    public class ItemTypeConstants
    {
        public const string Helm = "helm";
        public const string Chest = "chest";
        public const string Gloves = "gloves";
        public const string Pants = "pants";
        public const string Boots = "boots";
        public const string Amulet = "amulet";
        public const string Ring = "ring";
        public const string Weapon = "weapon";
        public const string Ranged = "ranged";
        public const string Offhand = "offhand";

        // Barbarian Arsenal. The character sheet lays these out as:
        //   [2H Bludgeoning] [1H mainhand] [1H offhand] [2H Slashing]
        // Mainhand and offhand produce identical tooltips and are therefore merged.
        // Constants are named for the Arsenal slot ("Slicing"); the tooltip says
        // "(Slashing)". Do not unify these two terms.
        public const string WeaponBludgeoning = "weapon_bludgeoning";
        public const string WeaponSlicing = "weapon_slicing";
        public const string WeaponOneHand = "weapon_onehand";

        public const string Charm = "charm";
        public const string HoradricSeal = "horadricseal";
        public const string Rune = "rune";
        public const string Sigil = "sigil";
        public const string Temper = "temper";
        public const string OccultGem = "occultgem"; // Season 7
        public const string WitcherSigil = "witchersigil"; // Whispering Wood - Season 7
        public const string DungeonEscalation = "dungeonescalation"; // Escalation Sigil - Season 9+
        public const string HoradricJewel = "horadricjewel"; // Season 9
        public const string BloodiedLair = "bloodiedlair"; // BloodiedLair Sigil - Season 12
    }
}