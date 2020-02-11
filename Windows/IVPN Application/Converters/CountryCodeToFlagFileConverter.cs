using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace IVPN.Converters
{
    public class CountryCodeToFlagFileConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string countryCode = (string)value;
            if (value == null)
                return null;

            var imageResourceUri = $"pack://application:,,,/IVPN Client;component/Resources/flags/{countryCode.ToLower()}.png";

            try
            {
                return new BitmapImage(new Uri(imageResourceUri));
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/IVPN Client;component/Resources/flags/unk.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Same as CountryCodeToFlagFileConverter but returns null when no country flag defined
    /// </summary>
    public class CountryCodeToFlagFileConverterCanEmpty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string countryCode = (string)value;
            if (value == null)
                return null;

            var imageResourceUri = $"pack://application:,,,/IVPN Client;component/Resources/flags/{countryCode.ToLower()}.png";

            try
            {
                return new BitmapImage(new Uri(imageResourceUri));
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts country code to flag image (from set of all possible countries)
    /// </summary>
    public class CountryCodeToFlagFileConverterAllSet : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string countryCode = (string)value;
            if (value == null)
                return null;

            var imageResourceUri = $"pack://application:,,,/IVPN Client;component/Resources/flags/all/24/{countryCode.ToLower()}.png";

            try
            {
                return new BitmapImage(new Uri(imageResourceUri));
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
