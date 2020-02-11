using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IVPN.Converters
{
    public class BoolToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string s)
            {
                if (bool.TryParse(s, out var isHide) && isHide)
                    return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Hidden;
            }
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }

    public class BoolToVisiblityInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string s)
            {
                if (bool.TryParse(s, out var isHide) && isHide)
                    return (value is bool && (bool)value) ? Visibility.Hidden : Visibility.Visible;
            }

            return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Visibility && (Visibility)value != Visibility.Visible);
        }
    }
}
