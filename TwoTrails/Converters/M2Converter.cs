using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        case AreaDef.Acres: return (val * 0.00024711).ToString("F3");
                        case AreaDef.HectaAcres: return (val * 0.0001).ToString("F3");
                    }
                }
                catch
                {
                    //
                }
            }

            return String.Empty;
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
