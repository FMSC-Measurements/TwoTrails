using FMSC.GeoSpatial.UTM;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        //private List<TtPoint> _SelectedPoints { get; } =  new List<TtPoint>();

        public ObservableCollection<TtMapPolygonManager> PolygonManagers { get; } = new ObservableCollection<TtMapPolygonManager>();
        
        private IObservableTtManager _Manager;

        private bool CtrlKeyPressed, SettingVisibilities;


        public TtMapManager(Map map, IObservableTtManager manager)
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

            foreach (TtPoint point in _Points)
            {
                point.PolygonChanged += Point_PolygonChanged;
            }

            EventManager.RegisterClassHandler(typeof(Control), Control.KeyDownEvent, new KeyEventHandler((s, e) => {
                if (e.Key == Key.LeftCtrl)
                    CtrlKeyPressed = true;
            }), true);

            EventManager.RegisterClassHandler(typeof(Control), Control.KeyUpEvent, new KeyEventHandler((s, e) => {
                if (e.Key == Key.LeftCtrl)
                    CtrlKeyPressed = false;
            }), true);
        }


        private void CreateMapPolygon(TtPolygon polygon)
        {
            ObservableCollection<TtPoint> ocPoints = new ObservableCollection<TtPoint>(_Points.Where(p => p.PolygonCN == polygon.CN));
            _PointsByPolys.Add(polygon.CN, ocPoints);

            TtMapPolygonManager mpm = new TtMapPolygonManager(_Map, polygon, ocPoints, _Manager.GetPolygonGraphicOption(polygon.CN));
            if (polygon.Name.IndexOf("_plt", StringComparison.InvariantCultureIgnoreCase) > 0)
            {
                mpm.AdjBndVisible = false;
                mpm.AdjBndPointsVisible = false;
                mpm.WayPointsVisible = true;
            }

            _PolygonManagers.Add(polygon.CN, mpm);
            PolygonManagers.Add(mpm);

            //mpm.PointSelected += PointSelected;
            mpm.PropertyChanged += TtMapPolygonManager_PropertyChanged;
        }

        private void TtMapPolygonManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!SettingVisibilities && CtrlKeyPressed && e.PropertyName == nameof(TtMapPolygonManager.Visible) &&
                sender is TtMapPolygonManager mapPolyManager)
            {
                SettingVisibilities = true;

                foreach (TtMapPolygonManager m in PolygonManagers.Where(ttmpm => ttmpm != mapPolyManager))
                {
                    m.Visible = false;
                }

                mapPolyManager.Visible = true;

                SettingVisibilities = false;
            }
        }


        //private void PointSelected(TtMapPoint mapPoint, Boolean adjusted)
        //{
        //    TtPoint point = mapPoint.Point;

        //    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        //    {
        //        _SelectedPoints.Clear();
        //        IList<TtPoint> points = _PointsByPolys[point.PolygonCN];

        //        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        //        {
        //            if (_LastPoint.Index < point.Index)
        //            {
        //                for (int i = point.Index; i < points.Count; i++)
        //                {
        //                    _SelectedPoints.Add(points[i]);
        //                }

        //                for (int i = 0; i <= _LastPoint.Index; i++)
        //                {
        //                    _SelectedPoints.Add(points[i]);
        //                }
        //            }
        //            else
        //            {
        //                for (int i = _LastPoint.Index; i <= point.Index; i++)
        //                {
        //                    _SelectedPoints.Add(points[i]);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (_LastPoint.Index < point.Index)
        //            {
        //                for (int i = _LastPoint.Index; i <= point.Index; i++)
        //                {
        //                    _SelectedPoints.Add(points[i]);
        //                }
        //            }
        //            else
        //            {
        //                for (int i = point.Index; i < points.Count; i++)
        //                {
        //                    _SelectedPoints.Add(points[i]);
        //                }

        //                for (int i = 0; i <= _LastPoint.Index; i++)
        //                {
        //                    _SelectedPoints.Add(points[i]);
        //                }
        //            }
        //        }
        //    }
        //    else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        //    {
        //        _SelectedPoints.Add(point);
        //    }
        //    else
        //    {
        //        _SelectedPoints.Clear();
        //        _SelectedPoints.Add(point);
        //    }
            
        //    _LastPoint = point;
        //}

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
                        p.PolygonChanged += Point_PolygonChanged;
                        _PointsByPolys[p.PolygonCN].Insert(p.Index, p);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPoint p in e.OldItems)
                    {
                        p.PolygonChanged -= Point_PolygonChanged;
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
