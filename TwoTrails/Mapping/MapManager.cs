using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Mapping
{
    public class MapManager
    {
        Dictionary<string, Collection<TtPoint>> _PointsByPolys = new Dictionary<string, Collection<TtPoint>>();
        Dictionary<string, MapPolygonManager> _PolygonManagers = new Dictionary<string, MapPolygonManager>();
        ReadOnlyObservableCollection<TtPoint> _Points;
        Map _Map;


        public MapManager(Map map, ReadOnlyObservableCollection<TtPoint> points, ReadOnlyObservableCollection<TtPolygon> polygons)
        {
            _Map = map;
            _Points = points;
            
            foreach (TtPolygon poly in polygons)
            {
                ObservableCollection<TtPoint> ocPoints = new ObservableCollection<TtPoint>(_Points.Where(p => p.PolygonCN == poly.CN));
                _PointsByPolys.Add(poly.CN, ocPoints);

                MapPolygonManager mpm = new MapPolygonManager(_Map, poly, ocPoints);
                _PolygonManagers.Add(poly.CN, mpm);

                if (poly.Name.StartsWith("PointLocationWcover2"))
                {
                    mpm.AdjBndPointsVisible = true;
                    mpm.AdjBndVisible = true;
                }
            }


            ((INotifyCollectionChanged)_Points).CollectionChanged += Points_CollectionChanged;
            ((INotifyCollectionChanged)polygons).CollectionChanged += Polygons_CollectionChanged;


            //hook into all points to see if they change polygons
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
                        _PolygonManagers.Add(poly.CN, new MapPolygonManager(_Map, poly, ocPoints));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPolygon p in e.OldItems)
                    {
                        _PolygonManagers[p.CN].Detach();
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
