using System;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class RemoveNewLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value as string ?? string.Empty;
            return val.Replace(Environment.NewLine, " ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Method not implemented");
        }
    }
}
