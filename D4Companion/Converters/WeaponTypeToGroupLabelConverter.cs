using D4Companion.Localization;
using System;
using System.Globalization;
using System.Windows.Data;

namespace D4Companion.Converters
{
    /// <summary>
    /// Turns a weapon item type into the group header shown above the Arsenal sections of
    /// the weapon panel. Bound to a CollectionViewGroup's Name, which is the raw item type
    /// string produced by grouping on ItemAffix.Type.
    ///
    /// Anything that is not an Arsenal subtype - plain "weapon", and any non-weapon type a
    /// future caller might pass - falls back to the generic weapon caption, because such
    /// entries apply to every group rather than to one.
    ///
    /// Lookup goes through TranslationSource rather than the generated Resources class,
    /// which is internal to the Localization assembly. TranslationSource resolves by key
    /// against the ResourceManager, so it also picks up the current culture.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class WeaponTypeToGroupLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string key;
            switch (value as string)
            {
                case Constants.ItemTypeConstants.WeaponBludgeoning:
                    key = "rsCapWeaponBludgeoning";
                    break;
                case Constants.ItemTypeConstants.WeaponSlicing:
                    key = "rsCapWeaponSlicing";
                    break;
                case Constants.ItemTypeConstants.WeaponMainhand:
                    key = "rsCapWeaponMainhand";
                    break;
                case Constants.ItemTypeConstants.WeaponOffhand:
                    key = "rsCapWeaponOffhand";
                    break;
                case Constants.ItemTypeConstants.WeaponOneHand:
                    // Its own caption, not the generic one: an entry that reached here came
                    // from a source that knew the weapon was one-handed but not which hand,
                    // which is a narrower claim than "applies to any weapon".
                    key = "rsCapWeaponOneHand";
                    break;
                default:
                    key = "rsCapWeaponAny";
                    break;
            }

            return TranslationSource.Instance[key];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
