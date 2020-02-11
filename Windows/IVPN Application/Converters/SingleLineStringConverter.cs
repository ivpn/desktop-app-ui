using System;
using System.Globalization;
using System.Windows.Data;

namespace IVPN.Converters
{
    class SingleLineStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value as string;

            return s?.Replace(Environment.NewLine, " ").Replace('\n', ' ');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
