using CSUtil.ComponentModel;
using FMSC.Core.Collections;
using Microsoft.Maps.MapControl.WPF;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Mapping
{
    public class MapPolygonManager : NotifyPropertyChangedEx
    {
        public ObservableConvertedCollection<MapPoint, TtPoint> Points { get; }

        private MapPolygon _AdjBnd, _UnAdjBnd;
        public MapPolygon AdjBoundary { get { return _AdjBnd; } private set { SetField(ref _AdjBnd, value); } }
        public MapPolygon UnAdjBoundary { get { return _UnAdjBnd; } private set { SetField(ref _UnAdjBnd, value); } }

        private MapPath _AdjNav, _UnAdjNav;
        public MapPath AdjNavigation { get { return _AdjNav; } private set { SetField(ref _AdjNav, value); } }
        public MapPath UnAdjNavigation { get { return _UnAdjNav; } private set { SetField(ref _UnAdjNav, value); } }

        private object locker = new object();

        #region Visibility
        private bool _Visible;
        public bool Visible
        {
            get { return _Visible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _Visible, value, () =>
                            {
                                foreach (MapPoint p in Points)
                                    p.Visible = _Visible;
                            }); 
                }
            }
        }


        private bool _AdjBndVisible;
        public bool AdjBndVisible
        {
            get { return _AdjBndVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _AdjBndVisible, value, () => { AdjBoundary.Visible = value; }); 
                }
            }
        }

        private bool _AdjBndPointsVisible;
        public bool AdjBndPointsVisible
        {
            get { return _AdjBndPointsVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _AdjBndPointsVisible, value, () =>
                            {
                                foreach (MapPoint p in Points)
                                    p.AdjBndVisible = _AdjBndPointsVisible;
                            }); 
                }
            }
        }


        private bool _UnAdjBndVisible;
        public bool UnAdjBndVisible
        {
            get { return _UnAdjBndVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _UnAdjBndVisible, value, () => { UnAdjBoundary.Visible = value; }); 
                }
            }
        }

        private bool _UnAdjBndPointsVisible;
        public bool UnAdjBndPointsVisible
        {
            get { return _UnAdjBndPointsVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _UnAdjBndPointsVisible, value, () =>
                            {
                                foreach (MapPoint p in Points)
                                    p.UnAdjBndVisible = _UnAdjBndPointsVisible;
                            }); 
                }
            }
        }


        private bool _AdjNavVisible;
        public bool AdjNavVisible
        {
            get { return _AdjNavVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _AdjNavVisible, value, () => { AdjNavigation.Visible = value; }); 
                }
            }
        }

        private bool _AdjNavPointsVisible;
        public bool AdjNavPointsVisible
        {
            get { return _AdjNavPointsVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _AdjNavPointsVisible, value, () =>
                            {
                                foreach (MapPoint p in Points)
                                    p.AdjNavVisible = _AdjNavPointsVisible;
                            }); 
                }
            }
        }


        private bool _UnAdjNavVisible;
        public bool UnAdjNavVisible
        {
            get { return _UnAdjNavVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _UnAdjNavVisible, value, () => { UnAdjNavigation.Visible = value; }); 
                }
            }
        }

        private bool _UnAdjNavPointsVisible;
        public bool UnAdjNavPointsVisible
        {
            get { return _UnAdjNavPointsVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _UnAdjNavPointsVisible, value, () =>
                            {
                                foreach (MapPoint p in Points)
                                    p.UnAdjNavVisible = _UnAdjNavPointsVisible;
                            }); 
                }
            }
        }



        private bool _AdjMiscPointsVisible;
        public bool AdjMiscPointsVisible
        {
            get { return _AdjMiscPointsVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _AdjMiscPointsVisible, value, () =>
                            {
                                foreach (MapPoint p in Points)
                                    p.AdjMiscVisible = _AdjMiscPointsVisible;
                            }); 
                }
            }
        }

        private bool _UnAdjMiscPointsVisible;
        public bool UnAdjMiscPointsVisible
        {
            get { return _UnAdjMiscPointsVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _UnAdjMiscPointsVisible, value, () =>
                            {
                                foreach (MapPoint p in Points)
                                    p.UnAdjMiscVisible = _UnAdjMiscPointsVisible;
                            }); 
                }
            }
        }

        
        private bool _WayPointsVisible;
        public bool WayPointsVisible
        {
            get { return _WayPointsVisible; }
            set
            {
                lock (locker)
                {
                    SetField(ref _WayPointsVisible, value, () =>
                    {
                        foreach (MapPoint p in Points)
                            p.WayPointVisible = _WayPointsVisible;
                    });
                }
            }
        }
        #endregion


        public MapPolygonManager(Map map, TtPolygon polygon, ObservableCollection<TtPoint> points) :
            this(map, polygon, points, true, true, true, false, false, false, false, false, false, false, false, false)
        { }

        public MapPolygonManager(Map map, TtPolygon polygon, ObservableCollection<TtPoint> points,
            bool vis, bool adjBndVis, bool adjBndPtsVis, bool unadjBndVis, bool unadjBndPtsVis,
            bool adjNavVis, bool adjNavPtsVis, bool unadjNavVis, bool unadjNavPtsVis,
            bool adjMiscPtsVis, bool unadjMiscPtsVis, bool wayPtsVis)
        {
            _Visible = vis;
            _AdjBndVisible = adjBndVis;
            _AdjBndPointsVisible = adjBndPtsVis;
            _UnAdjBndVisible = unadjBndVis;
            _UnAdjBndPointsVisible = unadjBndPtsVis;

            _AdjNavVisible = adjNavVis;
            _AdjNavPointsVisible = adjNavPtsVis;
            _UnAdjNavVisible = unadjNavVis;
            _UnAdjNavPointsVisible = unadjNavPtsVis;

            _AdjMiscPointsVisible = adjMiscPtsVis;
            _UnAdjMiscPointsVisible = unadjMiscPtsVis;

            _WayPointsVisible = wayPtsVis;


            polygon.PolygonChanged += Polygon_PolygonChanged;

            AdjBoundary = new MapPolygon(map, polygon, new List<Location>());
            UnAdjBoundary = new MapPolygon(map, polygon, new List<Location>());
            AdjNavigation = new MapPath(map, polygon, new List<Location>());
            UnAdjNavigation = new MapPath(map, polygon, new List<Location>());

            Points = new ObservableConvertedCollection<MapPoint, TtPoint>(
                points,
                p => new MapPoint(map, p, Visible, _AdjBndPointsVisible, _UnAdjBndPointsVisible,
                        _AdjNavPointsVisible, _UnAdjNavPointsVisible, _AdjMiscPointsVisible, _UnAdjMiscPointsVisible,
                        _WayPointsVisible)
                );

            ((INotifyCollectionChanged)Points).CollectionChanged += MapPolygonManager_CollectionChanged;
        }

        private void MapPolygonManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (locker)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (MapPoint p in e.OldItems)
                        p.Detach();
                }
            }
        }

        private void Polygon_PolygonChanged(TtPolygon polygon)
        {
            lock (locker)
            {
                List<Location> adjBndLocs = new List<Location>();
                List<Location> unadjBndLocs = new List<Location>();
                List<Location> adjNavLocs = new List<Location>();
                List<Location> unadjNavLocs = new List<Location>();

                foreach (MapPoint p in Points)
                {
                    if (p.IsBndPoint)
                    {
                        adjBndLocs.Add(p.AdjLocation);
                        unadjBndLocs.Add(p.UnAdjLocation);
                    }

                    if (p.IsNavPoint)
                    {
                        adjNavLocs.Add(p.AdjLocation);
                        unadjNavLocs.Add(p.UnAdjLocation);
                    }
                }

                AdjBoundary.UpdateShape(adjBndLocs);
                UnAdjBoundary.UpdateShape(unadjBndLocs);

                AdjNavigation.UpdateShape(adjNavLocs);
                UnAdjNavigation.UpdateShape(unadjNavLocs); 
            }
        }
        
        public void Detach()
        {
            AdjBoundary.Detach();
            UnAdjBoundary.Detach();

            AdjNavigation.Detach();
            UnAdjNavigation.Detach();

            foreach (MapPoint p in Points)
            {
                p.Detach();
            }
        }
    }
}
