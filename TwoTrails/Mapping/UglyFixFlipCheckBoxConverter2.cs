﻿using FMSC.Core.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TwoTrails.Mapping
{
    public class UglyFixFlipCheckBoxConverter2 : IMultiValueConverter
    {
        private bool ignore = false;
        private List<string> keys = new List<string>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            FlipCheckBox fcb = values[0] as FlipCheckBox;

            bool? isChecked = null;
            if (values[1] is bool val)
                isChecked = val;
            
            string propertyName = parameter as string;
            

            if (fcb != null)
            {
                if (fcb.DataContext is PolygonVisibilityControl pvc)
                {
                    string key = $"{propertyName}";

                    if (pvc != null && !keys.Contains(key))
                    {
                        keys.Add(key);

                        fcb.CheckedChange += (s, e) =>
                        {
                            if (!ignore)
                                pvc.UpdateVisibility(propertyName, fcb.IsChecked);
                        };
                    }
                }

                ignore = true;
                fcb.IsChecked = isChecked;
                ignore = false;

            }

            return isChecked;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { null, true, null };
        }
    }
}
