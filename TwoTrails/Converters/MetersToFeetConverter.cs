using System;
using System.Globalization;
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
                        return dvalue * FMSC.Core.Convert.FeetToMeters_Coeff;
                    return dvalue * FMSC.Core.Convert.MetersToFeet_Coeff;

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
