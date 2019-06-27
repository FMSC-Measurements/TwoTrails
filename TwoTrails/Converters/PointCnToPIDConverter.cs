using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TwoTrails.Core.Points;

namespace TwoTrails.Converters
{
    public class PointCnToPIDConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length > 1 && values[0] is string cn && values[1] is IEnumerable<TtPoint> points)
            {
                return points.FirstOrDefault(p => p.CN == cn)?.PID.ToString();
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

