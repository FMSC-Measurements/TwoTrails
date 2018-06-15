using System;
using System.Windows;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is string parameterString && Enum.IsDefined(value.GetType(), value))
            {
                object parameterValue = Enum.Parse(value.GetType(), GetEnumValueString(parameterString));

                return parameterValue.Equals(value);
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter is string parameterString)
                return Enum.Parse(targetType, GetEnumValueString(parameterString));
            return DependencyProperty.UnsetValue;
        }

        private string GetEnumValueString(string value)
        {
            if (value.Contains("."))
                return value.Substring(value.LastIndexOf(".") + 1);
            else
                return value;
        }
    }
}
