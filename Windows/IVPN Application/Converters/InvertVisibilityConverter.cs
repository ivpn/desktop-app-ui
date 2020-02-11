using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IVPN.Converters
{
    public class InvertVisibilityConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                if (value != null)
                {
                    Visibility vis = (Visibility)value;
                    return (vis == Visibility.Collapsed || vis == Visibility.Hidden) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new Exception("Invalid call - one way only");
        }
    }
}
