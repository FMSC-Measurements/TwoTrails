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

namespace TwoTrails.ViewModels
{
    public class DataStyleModel : IDisposable
    {
        private readonly ReadOnlyObservableCollection<TtPolygon> _Polygons;
        private Dictionary<String, Style> _PolygonStyles, _PolygonStylesAlt;
        public bool Disposed { get; private set; }

        public DataStyleModel(ReadOnlyObservableCollection<TtPolygon> polygons)
        {
            _Polygons = polygons;
            _PolygonStyles = new Dictionary<string, Style>();
            _PolygonStylesAlt = new Dictionary<string, Style>();

            foreach (TtPolygon poly in project.Manager.GetPolygons())
                CreatePolygonStyle(poly);

            if (!_PolygonStyles.ContainsKey(Consts.EmptyGuid))
                CreatePolygonStyle(new TtPolygon() { CN = Consts.EmptyGuid });

            ((INotifyCollectionChanged)project.Manager.Polygons).CollectionChanged += DataStyleModel_CollectionChanged;
        }

        private void DataStyleModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TtPolygon poly in e.NewItems)
                {
                    CreatePolygonStyle(poly);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TtPolygon poly in e.OldItems)
                {
                    _PolygonStyles.Remove(poly.CN);
                    _PolygonStylesAlt.Remove(poly.CN);
                }
            }
        }

        private void CreatePolygonStyle(TtPolygon polygon)
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
            string id = point == null || !_PolygonStyles.ContainsKey(point.CN) ? Consts.EmptyGuid : point.PolygonCN;

            return (point == null ? 0 : point.Index) % 2 == 0 ? _PolygonStyles[id] : _PolygonStylesAlt[id];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                ((INotifyCollectionChanged)_Polygons).CollectionChanged -= DataStyleModel_CollectionChanged;

                _PolygonStyles.Clear();
                _PolygonStylesAlt.Clear();

                Disposed = true;
            }
        }
    }
}
