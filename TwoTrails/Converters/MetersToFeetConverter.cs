using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class MetersToFeetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && double.TryParse(value as string, out double dvalue))
            {
                try
                {
                    if ((parameter is bool rev && rev) || (parameter is string str && str.ToLower() == "true"))
                        return (dvalue * 1200d / 3937d);
                    return (dvalue * 3937d / 1200d);
                }
                catch
                {
                    //
                }
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
