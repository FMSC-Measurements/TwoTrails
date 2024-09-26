using System;
using System.ComponentModel;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class GetDescConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object[] attrs = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (attrs != null && attrs.Length > 0 && attrs[0] is DescriptionAttribute descAttr) ?
                descAttr.Description ?? value.ToString() : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
