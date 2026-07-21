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
        // Constants are named for the Arsenal slot ("Slicing"); the tooltip says
        // "(Slashing)". Do not unify these two terms.
        public const string WeaponBludgeoning = "weapon_bludgeoning";
        public const string WeaponSlicing = "weapon_slicing";

        // One-handed weapons form a two-level family. Build sites distinguish the two
        // Arsenal hands, so imports produce WeaponMainhand or WeaponOffhand. A scanned
        // tooltip cannot: it carries no hand marker, so OCR only ever produces the
        // parent WeaponOneHand, which matches both children. WeaponOneHand also remains
        // the value written by presets imported before the hands were split.
        //
        // WeaponOffhand is NOT the Offhand declared above. Offhand is the equipment slot
        // holding a shield, totem or focus; WeaponOffhand is a Barbarian's second
        // one-handed weapon. They never match each other - see AffixManager.IsTypeMatch.
        public const string WeaponOneHand = "weapon_onehand";
        public const string WeaponMainhand = "weapon_mainhand";
        public const string WeaponOffhand = "weapon_offhand";

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