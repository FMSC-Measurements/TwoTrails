﻿using CSUtil.ComponentModel;
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
    public class TtMapPolygonManager : NotifyPropertyChangedEx
    {
        public ObservableConvertedCollection<TtMapPoint, TtPoint> Points { get; }

        private TtMapPolygon _AdjBnd, _UnAdjBnd;
        public TtMapPolygon AdjBoundary { get { return _AdjBnd; } private set { SetField(ref _AdjBnd, value); } }
        public TtMapPolygon UnAdjBoundary { get { return _UnAdjBnd; } private set { SetField(ref _UnAdjBnd, value); } }

        private TtMapPath _AdjNav, _UnAdjNav;
        public TtMapPath AdjNavigation { get { return _AdjNav; } private set { SetField(ref _AdjNav, value); } }
        public TtMapPath UnAdjNavigation { get { return _UnAdjNav; } private set { SetField(ref _UnAdjNav, value); } }

        private object locker = new object();

        public TtPolygon Polygon { get; }

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
                        foreach (TtMapPoint p in Points)
                            p.Visible = _Visible;

                        if (_Visible)
                        {
                            AdjBoundary.Visible = _AdjBndVisible;
                            UnAdjBoundary.Visible = _UnAdjBndVisible;
                            AdjNavigation.Visible = _AdjNavVisible;
                            UnAdjNavigation.Visible = _UnAdjNavVisible;
                        }
                        else
                        {
                            AdjBoundary.Visible = false;
                            UnAdjBoundary.Visible = false;
                            AdjNavigation.Visible = false;
                            UnAdjNavigation.Visible = false;
                        }
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
                    SetField(ref _AdjBndVisible, value, () => { AdjBoundary.Visible = value && _Visible; }); 
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
                                foreach (TtMapPoint p in Points)
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
                                foreach (TtMapPoint p in Points)
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
                                foreach (TtMapPoint p in Points)
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
                                foreach (TtMapPoint p in Points)
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
                                foreach (TtMapPoint p in Points)
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
                                foreach (TtMapPoint p in Points)
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
                        foreach (TtMapPoint p in Points)
                            p.WayPointVisible = _WayPointsVisible;
                    });
                }
            }
        }
        #endregion


        #region Color
        private PolygonGraphicOptions Graphics { get; }
        #endregion


        public TtMapPolygonManager(Map map, TtPolygon polygon, ObservableCollection<TtPoint> points, PolygonGraphicOptions pgo) :
            this(map, polygon, points, pgo, true, true, true, false, false, false, false, false, false, false, false, false)
        { }

        public TtMapPolygonManager(Map map, TtPolygon polygon, ObservableCollection<TtPoint> points, PolygonGraphicOptions pgo,
            bool vis, bool adjBndVis, bool adjBndPtsVis, bool unadjBndVis, bool unadjBndPtsVis,
            bool adjNavVis, bool adjNavPtsVis, bool unadjNavVis, bool unadjNavPtsVis,
            bool adjMiscPtsVis, bool unadjMiscPtsVis, bool wayPtsVis)
        {
            Polygon = polygon;
            Graphics = pgo;

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


            polygon.PolygonChanged += UpdatePolygonShape;

            AdjBoundary = new TtMapPolygon(map, polygon, new LocationCollection(), Graphics, true, _AdjBndVisible);
            UnAdjBoundary = new TtMapPolygon(map, polygon, new LocationCollection(), Graphics, false, _UnAdjBndVisible);
            AdjNavigation = new TtMapPath(map, polygon, new LocationCollection(), Graphics, true, _AdjNavVisible);
            UnAdjNavigation = new TtMapPath(map, polygon, new LocationCollection(), Graphics, false, _UnAdjNavVisible);

            Points = new ObservableConvertedCollection<TtMapPoint, TtPoint>(
                points,
                p => new TtMapPoint(map, p, Graphics, Visible, _AdjBndPointsVisible, _UnAdjBndPointsVisible,
                        _AdjNavPointsVisible, _UnAdjNavPointsVisible, _AdjMiscPointsVisible, _UnAdjMiscPointsVisible,
                        _WayPointsVisible)
                );

            UpdatePolygonShape(polygon);

            ((INotifyCollectionChanged)Points).CollectionChanged += MapPolygonManager_CollectionChanged;
        }

        private void MapPolygonManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (locker)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (TtMapPoint p in e.OldItems)
                        p.Detach();
                }
            }
        }

        private void UpdatePolygonShape(TtPolygon polygon)
        {
            lock (locker)
            {
                LocationCollection adjBndLocs = new LocationCollection();
                LocationCollection unadjBndLocs = new LocationCollection();
                LocationCollection adjNavLocs = new LocationCollection();
                LocationCollection unadjNavLocs = new LocationCollection();

                foreach (TtMapPoint p in Points)
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

                AdjBoundary.UpdateLocations(adjBndLocs);
                UnAdjBoundary.UpdateLocations(unadjBndLocs);

                AdjNavigation.UpdateLocations(adjNavLocs);
                UnAdjNavigation.UpdateLocations(unadjNavLocs); 
            }
        }
        
        public void Detach()
        {
            AdjBoundary.Detach();
            UnAdjBoundary.Detach();

            AdjNavigation.Detach();
            UnAdjNavigation.Detach();

            foreach (TtMapPoint p in Points)
            {
                p.Detach();
            }
        }
    }
}
