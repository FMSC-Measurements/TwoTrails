﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class NullableDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && value is double ? value.ToString() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (str.EndsWith("."))
                    return Binding.DoNothing;
                else if (Double.TryParse(str, out double val))
                    return val;
            }

            return null;
        }
    }
}
