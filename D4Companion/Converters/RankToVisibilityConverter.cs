using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace D4Companion.Converters
{
    /// <summary>
    /// Shows the stat-priority badge only when the source build actually ranked the affix.
    /// Rank 0 means "no ranking published" - the flag-only build sites, and every preset
    /// saved before ranks existed - and must not render as a rank at all.
    /// </summary>
    public class RankToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int rank && rank > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
