﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class NotNullVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is bool rev)
                return rev ? value == null : value != null;
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
