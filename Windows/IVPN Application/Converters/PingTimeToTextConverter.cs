using System;
using System.Windows.Data;

namespace IVPN.Converters
{
    public class PingTimeToTextConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int pingTime = (int)value;
            if (pingTime == 0)
                return "";
            
            if (pingTime < 1000)
                return String.Format("{0:N0} ms", pingTime);
            else
                return String.Format("{0:N0} s", pingTime / 1000);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
