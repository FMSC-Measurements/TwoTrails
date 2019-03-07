using FMSC.GeoSpatial;
using System;
using System.Globalization;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class DmsToDecDegConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DMS dms)
            {
                switch (dms)
                {
                    case Latitude lat: return lat.toSignedDecimal();
                    case Longitude lon: return lon.toSignedDecimal();
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
