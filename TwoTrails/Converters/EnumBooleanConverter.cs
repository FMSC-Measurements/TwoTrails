using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class EnumBooleanConverter : IValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //{
        //    return value.Equals(parameter);
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //{
        //    return value.Equals(true) ? parameter : Binding.DoNothing;
        //}

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;
            
            object parameterValue = Enum.Parse(value.GetType(), GetEnumValueString(parameterString));

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;
            
            return Enum.Parse(targetType, GetEnumValueString(parameterString));
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
