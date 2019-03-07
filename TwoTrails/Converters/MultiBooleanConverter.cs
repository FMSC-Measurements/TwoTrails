using System;
using System.Globalization;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class MultiBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool value = true;

            foreach (object val in values)
                if (val is bool)
                    value &= (bool)val;

            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
