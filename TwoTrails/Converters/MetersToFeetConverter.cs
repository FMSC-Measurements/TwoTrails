using System;
using System.Globalization;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class MetersToFeetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? dvalue = null;

            if (value != null)
            {
                if (value is double)
                {
                    dvalue = (double)value;
                }
                else if (double.TryParse(value as string, out double dv))
                {
                    dvalue = dv;
                }

                if ((parameter is bool rev && rev) || (parameter is string str && str.ToLower() == "true"))
                    return dvalue * FMSC.Core.Convert.FeetToMeters_Coeff;
                return dvalue * FMSC.Core.Convert.MetersToFeet_Coeff;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
