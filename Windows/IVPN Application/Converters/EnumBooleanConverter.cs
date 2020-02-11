using System;
using System.Globalization;
using System.Windows.Data;

namespace IVPN.Converters
{
    /// <summary>
    /// For binding RadioButtons to same enum property
    ///
    /// <example>
    ///     <RadioButton IsChecked = "{Binding SelectedOption, Converter={StaticResource EnumBooleanConverter}, ConverterParameter={x:Static local:RadioOptions.Option1}}"/>
    ///     <RadioButton IsChecked = "{Binding SelectedOption, Converter={StaticResource EnumBooleanConverter}, ConverterParameter={x:Static local:RadioOptions.Option2}}" />
    /// </example>
    /// </summary>
    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && ((bool) value) ? parameter : Binding.DoNothing;
        }
    }
}