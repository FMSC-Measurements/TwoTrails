using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    class VisibilityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
            {
                if (parameter is bool rev)
                {
                    if (rev)
                        return vis != Visibility.Visible;
                }

                return vis == Visibility.Visible;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool vis)
            {
                if (parameter is bool rev)
                    return vis ? Visibility.Collapsed : Visibility.Visible;
                
                return vis ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }
    }
}
