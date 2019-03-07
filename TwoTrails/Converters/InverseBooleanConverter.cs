using System;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return Invert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return Invert(value);
        }

        private bool? Invert(object value) => (value != null && value is bool b) ? !(bool)value : (bool?)null;
        #endregion
    }
}
