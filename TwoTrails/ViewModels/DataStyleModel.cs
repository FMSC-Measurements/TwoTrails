using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.ViewModels
{
    public class DataStyleModel
    {
        private readonly ReadOnlyObservableCollection<TtUnit> _Polygons;
        private Dictionary<String, Style> _PolygonStyles, _PolygonStylesAlt;
        public bool Disposed { get; private set; }

        public DataStyleModel(ReadOnlyObservableCollection<TtUnit> units)
        {
            _Polygons = units;
            _PolygonStyles = new Dictionary<string, Style>();
            _PolygonStylesAlt = new Dictionary<string, Style>();

            foreach (TtUnit poly in units)
                CreatePolygonStyle(poly);

            if (!_PolygonStyles.ContainsKey(Consts.EmptyGuid))
                CreatePolygonStyle(new PolygonUnit() { CN = Consts.EmptyGuid });

            ((INotifyCollectionChanged)units).CollectionChanged += DataStyleModel_CollectionChanged;
        }

        private void DataStyleModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TtUnit poly in e.NewItems)
                {
                    CreatePolygonStyle(poly);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TtUnit poly in e.OldItems)
                {
                    _PolygonStyles.Remove(poly.CN);
                    _PolygonStylesAlt.Remove(poly.CN);
                }
            }
        }

        private void CreatePolygonStyle(TtUnit polygon)
        {
            Style style = new Style(typeof(DataGridRow));
            
            //todo get poly color, if not default use that with adaptions

            SolidColorBrush rowBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush rowBrushAlt = new SolidColorBrush(Colors.WhiteSmoke);
            SolidColorBrush rowBrushHover = new SolidColorBrush(Colors.DarkSeaGreen);

            
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
            string id = point == null || !_PolygonStyles.ContainsKey(point.CN) ? Consts.EmptyGuid : point.UnitCN;

            return (point == null ? 0 : point.Index) % 2 == 0 ? _PolygonStyles[id] : _PolygonStylesAlt[id];
        }
    }
}
