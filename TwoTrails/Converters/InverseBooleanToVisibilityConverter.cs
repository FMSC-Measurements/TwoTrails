using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool vis)
            {
                if (parameter is bool rev)
                    return vis ? Visibility.Visible : Visibility.Collapsed;
                
                return vis ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
            {
                if (parameter is bool rev)
                {
                    if (rev)
                        return vis != Visibility.Collapsed;
                }

                return vis == Visibility.Collapsed;
            }

            return false;
        }
    }
}
