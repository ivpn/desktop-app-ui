using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace IVPN.Converters
{
    class PingStatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is double))
                return null;
            double pingTimeRelative = (double)value;

            string imageResourceUri;

            if (pingTimeRelative < 0.6)
                imageResourceUri = "pack://application:,,,/IVPN Client;component/Resources/iconStatusGood.png";
            else if (pingTimeRelative <= 0.9)
                imageResourceUri = "pack://application:,,,/IVPN Client;component/Resources/iconStatusModerate.png";
            else
                imageResourceUri = "pack://application:,,,/IVPN Client;component/Resources/iconStatusBad.png";
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
