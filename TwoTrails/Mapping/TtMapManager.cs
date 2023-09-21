using FMSC.Core.Windows.ComponentModel.Commands;
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
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Mapping
{
    public class TtMapManager
    {
        private Dictionary<string, ObservableCollection<TtPoint>> _PointsByPolys = new Dictionary<string, ObservableCollection<TtPoint>>();
        private Dictionary<string, TtMapPolygonManager> _PolygonManagersMap = new Dictionary<string, TtMapPolygonManager>();
        private Map _Map;
        private MapControl _MapControl;

        //private TtPoint _LastPoint;
        //private List<TtPoint> _SelectedPoints { get; } =  new List<TtPoint>();

        public ObservableCollection<TtMapPolygonManager> PolygonManagers { get; } = new ObservableCollection<TtMapPolygonManager>();
        
        
        private IObservableTtManager _Manager;

        private bool SettingVisibilities;

        public ICommand ZoomToPolygonCommand { get; }


        public TtMapManager(MapControl mapControl, Map map, IObservableTtManager manager)
        {
            _MapControl = mapControl;
            _Map = map;
            _Manager = manager;

            foreach (TtUnit poly in _Manager.Units)
            {
                CreateMapPolygon(poly);
            }
            
            ((INotifyCollectionChanged)_Manager.Points).CollectionChanged += Points_CollectionChanged;
            ((INotifyCollectionChanged)_Manager.Units).CollectionChanged += Polygons_CollectionChanged;

            foreach (TtPoint point in _Manager.Points)
            {
                point.UnitChanged += Point_PolygonChanged;
            }

            ZoomToPolygonCommand = new RelayCommand(x =>
            {
                if (x is TtUnit poly)
                {
                    ZoomToPolygon(poly);
                }
            });
        }


        private void CreateMapPolygon(TtUnit unit)
        {
            ObservableCollection<TtPoint> ocPoints = new ObservableCollection<TtPoint>(_Manager.Points.Where(p => p.UnitCN == unit.CN));
            _PointsByPolys.Add(unit.CN, ocPoints);

            TtMapPolygonManager mpm = new TtMapPolygonManager(_Map, unit, ocPoints, _Manager.GetUnitGraphicOption(unit.CN));
            if (unit.Name.IndexOf("_plt", StringComparison.InvariantCultureIgnoreCase) > 0 || 
                (ocPoints.Count > 0 && ocPoints.All(p => p.OpType == OpType.WayPoint)))
            {
                mpm.AdjBndVisible = false;
                mpm.AdjBndPointsVisible = false;
                mpm.WayPointsVisible = true;
            }
            else if (ocPoints.Count > 0 && ocPoints.All(p => p.IsMiscPoint()))
            {
                mpm.AdjBndVisible = false;
                mpm.AdjBndPointsVisible = false;
                mpm.AdjMiscPointsVisible = true;
            }

            _PolygonManagersMap.Add(unit.CN, mpm);
            PolygonManagers.Add(mpm);

            mpm.PropertyChanged += TtMapPolygonManager_PropertyChanged;
        }

        private void TtMapPolygonManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!SettingVisibilities && _MapControl.CtrlKeyPressed && e.PropertyName == nameof(TtMapPolygonManager.Visible) &&
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

        private void RemoveMapPolygon(TtUnit polygon)
        {
            TtMapPolygonManager mpm = _PolygonManagersMap[polygon.CN];

            mpm.PropertyChanged -= TtMapPolygonManager_PropertyChanged;

            PolygonManagers.Remove(mpm);
            _PolygonManagersMap.Remove(polygon.CN);
            _PointsByPolys.Remove(polygon.CN);

            mpm.Detach();
        }


        private void Polygons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TtUnit poly in e.NewItems)
                    {
                        CreateMapPolygon(poly);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtUnit poly in e.OldItems)
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
                        p.UnitChanged += Point_PolygonChanged;
                        _PointsByPolys[p.UnitCN].Insert(p.Index, p);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (TtPoint p in e.OldItems)
                    {
                        p.UnitChanged -= Point_PolygonChanged;
                        _PointsByPolys[p.UnitCN].Remove(p);
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

        private void Point_PolygonChanged(TtPoint point, TtUnit newPolygon, TtUnit oldPolygon)
        {
            _PointsByPolys[oldPolygon.CN].Remove(point);

            ObservableCollection<TtPoint> points = _PointsByPolys[newPolygon.CN];
            if (point.Index < points.Count)
                points.Insert(point.Index, point);
            else
                points.Add(point);
        }

        public void ZoomToPolygon(TtUnit polygon)
        {
            if (_PolygonManagersMap.ContainsKey(polygon.CN))
            {
                _PolygonManagersMap[polygon.CN].ZoomToUnit();
            }
        }
    }
}
