using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Controls
{
    public class DataStyleModel
    {
        private Dictionary<String, Style> _PolygonStyles, _PolygonStylesAlt;


        public DataStyleModel(TtProject project)
        {
            _PolygonStyles = new Dictionary<string, Style>();
            _PolygonStylesAlt = new Dictionary<string, Style>();

            foreach (TtPolygon poly in project.HistoryManager.GetPolyons())
            {
                CreatePolygonStyle(poly);
            }
        }
        
        private void CreatePolygonStyle(TtPolygon polygon)
        {
            Style style = new Style(typeof(DataGridRow));
            
            //get poly color, if not default use that with adaptions

            SolidColorBrush rowBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush rowBrushAlt = new SolidColorBrush(Colors.WhiteSmoke);
            SolidColorBrush rowBrushHover = new SolidColorBrush(Colors.Aqua);

            
            style.Setters.Add(new Setter(DataGridRow.BackgroundProperty, rowBrush));
            
            Trigger trig = new Trigger()
            {
                Property = DataGridRow.IsMouseOverProperty,
                Value = true
            };
            trig.Setters.Add(new Setter(DataGridRow.BackgroundProperty, rowBrushHover));
            style.Triggers.Add(trig);

            _PolygonStyles.Add(polygon.CN, style);


            style = new Style(typeof(DataGridRow));

            style.Setters.Add(new Setter(DataGridRow.BackgroundProperty, rowBrushAlt));

            trig = new Trigger()
            {
                Property = DataGridRow.IsMouseOverProperty,
                Value = true
            };
            trig.Setters.Add(new Setter(DataGridRow.BackgroundProperty, rowBrushHover));
            style.Triggers.Add(trig);

            _PolygonStylesAlt.Add(polygon.CN, style);
        }

        public Style GetRowStyle(TtPoint point)
        {
            return point.Index % 2 == 0 ? _PolygonStyles[point.PolygonCN] : _PolygonStylesAlt[point.PolygonCN];
        }
    }
}
