﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace TwoTrails.Converters
{
    public class M2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string par = parameter as string;
            if (value != null && par != null)
            {
                try
                {
                    double val = (double)value;
                    AreaDef def = (AreaDef)Enum.Parse(typeof(AreaDef), par);

                    switch (def)
                    {
                        case AreaDef.Acres: return (val * 0.00024711);
                        case AreaDef.HectaAcres: return (val * 0.0001);
                    }
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

    public enum AreaDef
    {
        Acres,
        HectaAcres
    }
}
