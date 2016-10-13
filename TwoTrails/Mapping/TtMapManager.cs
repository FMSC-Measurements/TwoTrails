using FMSC.GeoSpatial;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Mapping
{
    public class TtMapManager
    {
        private Dictionary<string, ObservableCollection<TtPoint>> _PointsByPolys = new Dictionary<string, ObservableCollection<TtPoint>>();
        private Dictionary<string, TtMapPolygonManager> _PolygonManagers = new Dictionary<string, TtMapPolygonManager>();
        private ReadOnlyObservableCollection<TtPoint> _Points;
        private ReadOnlyObservableCollection<TtPolygon> _Polygons;
        private Map _Map;

        public ObservableCollection<TtMapPolygonManager> PolygonManagers { get; } = new ObservableCollection<TtMapPolygonManager>();

        private TtManager _Manager;


        public TtMapManager(Map map, TtManager manager)
        {
            _Map = map;
            _Manager = manager;
            _Polygons = _Manager.Polygons;
            _Points = _Manager.Points;
            
            foreach (TtPolygon poly in _Polygons)
            {
                ObservableCollection<TtPoint> ocPoints = new ObservableCollection<TtPoint>(_Points.Where(p => p.PolygonCN == poly.CN));
                _PointsByPolys.Add(poly.CN, ocPoints);

                TtMapPolygonManager mpm = new TtMapPolygonManager(_Map, poly, ocPoints, _Manager.GetPolygonGraphicOption(poly.CN));
                _PolygonManagers.Add(poly.CN, mpm);
                PolygonManagers.Add(mpm);
            }


            ((INotifyCollectionChanged)_Points).CollectionChanged += Points_CollectionChanged;
            ((INotifyCollectionChanged)_Polygons).CollectionChanged += Polygons_CollectionChanged;

            if (map.ActualHeight > 0)
            {
                IEnumerable<Location> locs = PolygonManagers.SelectMany(mpm => mpm.Points.Select(p => p.AdjLocation));
                if (locs.Any())
                    map.SetView(locs, new Thickness(10), 0);
            }

            //TODO hook into all points to see if they change polygons
        }

        private void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //add or remove points from the polygons

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TtPolygon poly in e.NewItems)
                    {
                        ObservableCollection<TtPoint> ocPoints = new ObservableCollection<TtPoint>(_Points.Where(p => p.PolygonCN == poly.CN));
                        _PointsByPolys.Add(poly.CN, ocPoints);
                        TtMapPolygonManager mpm = new TtMapPolygonManager(_Map, poly, ocPoints, _Manager.GetPolygonGraphicOption(poly.CN));
                        _PolygonManagers.Add(poly.CN, mpm);
                        PolygonManagers.Add(mpm);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPolygon p in e.OldItems)
                    {
                        TtMapPolygonManager mpm = _PolygonManagers[p.CN];
                        mpm.Detach();
                        PolygonManagers.Remove(mpm);
                        _PolygonManagers.Remove(p.CN);
                        _PointsByPolys.Remove(p.CN);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }

        private void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //add or remove points from the polygons

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TtPoint p in e.NewItems)
                    {
                        _PointsByPolys[p.PolygonCN].Insert(p.Index, p);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPoint p in e.OldItems)
                    {
                        _PointsByPolys[p.PolygonCN].Remove(p);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    break;
            }
        }
    }
}
