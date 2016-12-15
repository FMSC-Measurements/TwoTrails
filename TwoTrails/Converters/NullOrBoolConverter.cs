using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class NullOrBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 1)
            {
                if (values[0] != null)
                {
                    bool r = true;
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (values[i] is bool)
                        {
                            r &= (bool)values[i];

                            if (!r)
                                return r;
                        }
                        else
                            return false;

                    }

                    return r;
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
