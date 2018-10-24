using FMSC.Core.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TwoTrails.Mapping
{
    public class UglyFixFlipCheckBoxConverter : IMultiValueConverter
    {
        private bool ignore = false;
        private List<string> keys = new List<string>();
        //Dictionary<string, FlipCheckBox> flipCheckBoxes = new Dictionary<string, FlipCheckBox>();
        //Dictionary<string, TtMapPolygonManager> mapPolygonManagers = new Dictionary<string, TtMapPolygonManager>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            FlipCheckBox fcb = values[0] as FlipCheckBox;
            bool isChecked = (bool)values[1];
            string propertyName = parameter as string;
            

            if (fcb != null)
            {
                TtMapPolygonManager mpm = fcb.DataContext as TtMapPolygonManager;

                string key = $"{propertyName}_{mpm.Polygon.CN}";

                if (mpm != null && !keys.Contains(key))// !mapPolygonManagers.ContainsKey(key))
                {
                    //flipCheckBoxes.Add(key, fcb);
                    //mapPolygonManagers.Add(key, mpm);

                    keys.Add(key);

                    fcb.CheckedChange += (s, e) =>
                    {
                        if (!ignore)
                            mpm.UpdateVisibility(propertyName, fcb.IsChecked);
                    };
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
