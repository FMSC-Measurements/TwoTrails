using FMSC.Core.Utilities;
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
using System.Windows.Input;
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

        private TtPoint _LastPoint;
        private List<TtPoint> _SelectedPoints { get; } =  new List<TtPoint>();

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
                CreateMapPolygon(poly);
            }


            ((INotifyCollectionChanged)_Points).CollectionChanged += Points_CollectionChanged;
            ((INotifyCollectionChanged)_Polygons).CollectionChanged += Polygons_CollectionChanged;

            if (map.ActualHeight > 0)
            {
                IEnumerable<Location> locs = PolygonManagers.SelectMany(mpm => mpm.Points.Select(p => p.AdjLocation));
                if (locs.Any())
                    map.SetView(locs, new Thickness(10), 0);
            }

            foreach (TtPoint point in _Points)
            {
                point.PointPolygonChanged += Point_PolygonChanged;
            }
        }

        private void CreateMapPolygon(TtPolygon polygon)
        {
            ObservableCollection<TtPoint> ocPoints = new ObservableCollection<TtPoint>(_Points.Where(p => p.PolygonCN == polygon.CN));
            _PointsByPolys.Add(polygon.CN, ocPoints);

            TtMapPolygonManager mpm = new TtMapPolygonManager(_Map, polygon, ocPoints, _Manager.GetPolygonGraphicOption(polygon.CN));
            _PolygonManagers.Add(polygon.CN, mpm);
            PolygonManagers.Add(mpm);

            mpm.PointSelected += PointSelected;
        }

        private void PointSelected(TtMapPoint mapPoint, Boolean adjusted)
        {
            TtPoint point = mapPoint.Point;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                _SelectedPoints.Clear();
                IList<TtPoint> points = _PointsByPolys[point.PolygonCN];

                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (_LastPoint.Index < point.Index)
                    {
                        for (int i = point.Index; i < points.Count; i++)
                        {
                            _SelectedPoints.Add(points[i]);
                        }

                        for (int i = 0; i <= _LastPoint.Index; i++)
                        {
                            _SelectedPoints.Add(points[i]);
                        }
                    }
                    else
                    {
                        for (int i = _LastPoint.Index; i <= point.Index; i++)
                        {
                            _SelectedPoints.Add(points[i]);
                        }
                    }
                }
                else
                {
                    if (_LastPoint.Index < point.Index)
                    {
                        for (int i = _LastPoint.Index; i <= point.Index; i++)
                        {
                            _SelectedPoints.Add(points[i]);
                        }
                    }
                    else
                    {
                        for (int i = point.Index; i < points.Count; i++)
                        {
                            _SelectedPoints.Add(points[i]);
                        }

                        for (int i = 0; i <= _LastPoint.Index; i++)
                        {
                            _SelectedPoints.Add(points[i]);
                        }
                    }
                }
            }
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                _SelectedPoints.Add(point);
            }
            else
            {
                _SelectedPoints.Clear();
                _SelectedPoints.Add(point);
            }
            
            _LastPoint = point;
        }

        private void RemoveMapPolygon(TtPolygon polygon)
        {
            TtMapPolygonManager mpm = _PolygonManagers[polygon.CN];
            mpm.Detach();
            PolygonManagers.Remove(mpm);
            _PolygonManagers.Remove(polygon.CN);
            _PointsByPolys.Remove(polygon.CN);
        }


        private void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TtPolygon poly in e.NewItems)
                    {
                        CreateMapPolygon(poly);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPolygon poly in e.OldItems)
                    {
                        RemoveMapPolygon(poly);
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

        private void Point_PolygonChanged(TtPoint point, TtPolygon newPolygon, TtPolygon oldPolygon)
        {
            _PointsByPolys[oldPolygon.CN].Remove(point);

            ObservableCollection<TtPoint> points = _PointsByPolys[newPolygon.CN];
            if (point.Index < points.Count)
                points.Insert(point.Index, point);
            else
                points.Add(point);
        }
    }
}
