using Microsoft.Maps.MapControl.WPF;
using System;
using TwoTrails.Core.Points;
using System.Windows;
using FMSC.GeoSpatial.UTM;
using System.Diagnostics;
using System.Windows.Input;
using CSUtil.ComponentModel;
using System.Windows.Media;
using System.ComponentModel;
using TwoTrails.Core;
using System.Windows.Controls;
using Point = FMSC.Core.Point;

namespace TwoTrails.Mapping
{
    public delegate void MapPointEvent(TtMapPoint point);
    public delegate void MapPointSelectedEvent(TtMapPoint point, bool adjusted);

    public class TtMapPoint : NotifyPropertyChangedEx, IDisposable
    {
        public event MapPointEvent LocationChanged;
        public event MapPointSelectedEvent PointSelected;

        public Pushpin AdjPushpin { get; } = new Pushpin();
        public Pushpin UnAdjPushpin { get; } = new Pushpin();

        private Location _AdjLoc, _UnAdjLoc;
        public Location AdjLocation { get { return _AdjLoc; } private set { SetField(ref _AdjLoc, value); } }
        public Location UnAdjLocation { get { return _UnAdjLoc; } private set { SetField(ref _UnAdjLoc, value); } }

        public TtPoint Point { get; }
        public bool IsBndPoint { get { return Point.IsBndPoint(); } }
        public bool IsNavPoint { get; }
        public bool IsMiscPoint { get { return Point.IsMiscPoint(); } }
        public int Index { get { return Point.Index; } }

        private Map _Map;
        
        private object locker = new object();

        private bool _Editing;
        public bool Editing
        {
            get { return _Editing; }
            set
            {
                lock (locker)
                {
                    if (_Editing != value)
                    {
                        _Editing = value;

                        UnAdjPushpin.IsEnabled = !_Editing;
                        UnAdjPushpin.Background = new SolidColorBrush(UnAdjColor)
                        {
                            Opacity = _Editing ? 0.7d : 1
                        };
                    }
                }
            }
        }


        #region Color
        private Color _AdjColor;
        public Color AdjColor
        {
            get { return _AdjColor; }
            set { SetField(ref _AdjColor, value, UpdateColor); }
        }


        private Color _UnAdjColor;
        public Color UnAdjColor
        {
            get { return _UnAdjColor; }
            set { SetField(ref _UnAdjColor, value, UpdateColor); }
        }


        private Color _WayPointColor;
        public Color WayPointColor
        {
            get { return _WayPointColor; }
            set { SetField(ref _WayPointColor, value, UpdateColor); }
        }

        private void UpdateColor()
        {
            if (Point.IsWayPointAtBase())
            {
                UnAdjPushpin.Background = new SolidColorBrush(WayPointColor)
                {
                    Opacity = _Editing ? 0.7d : 1
                };
            }
            else
            {
                AdjPushpin.Background = new SolidColorBrush(AdjColor);
                UnAdjPushpin.Background = new SolidColorBrush(UnAdjColor)
                {
                    Opacity = _Editing ? 0.7d : 1
                };
            }
        }
        #endregion


        #region Visibility
        private bool _Visible = true;
        public bool Visible
        {
            get { return _Visible; }
            set { SetField(ref _Visible, value, UpdateVisibility); }
        }


        private bool _AdjBndVisible;
        public bool AdjBndVisible
        {
            get { return _AdjBndVisible; }
            set { SetField(ref _AdjBndVisible, value, UpdateVisibility); }
        }

        private bool _UnAdjBndVisible;
        public bool UnAdjBndVisible
        {
            get { return _UnAdjBndVisible; }
            set { SetField(ref _UnAdjBndVisible, value, UpdateVisibility); }
        }


        private bool _AdjNavVisible;
        public bool AdjNavVisible
        {
            get { return _AdjNavVisible; }
            set { SetField(ref _AdjNavVisible, value, UpdateVisibility); }
        }

        private bool _UnAdjNavVisible;
        public bool UnAdjNavVisible
        {
            get { return _UnAdjNavVisible; }
            set { SetField(ref _UnAdjNavVisible, value, UpdateVisibility); }
        }



        private bool _AdjMiscVisible;
        public bool AdjMiscVisible
        {
            get { return _AdjMiscVisible; }
            set { SetField(ref _AdjMiscVisible, value, UpdateVisibility); }
        }

        private bool _UnAdjMiscVisible;
        public bool UnAdjMiscVisible
        {
            get { return _UnAdjMiscVisible; }
            set { SetField(ref _UnAdjMiscVisible, value, UpdateVisibility); }
        }


        private bool _WayPointVisible;
        public bool WayPointVisible
        {
            get { return _WayPointVisible; }
            set { SetField(ref _WayPointVisible, value, UpdateVisibility); }
        }

        private void UpdateVisibility()
        {
            lock (locker)
            {
                if (_Visible)
                {
                    AdjPushpin.Visibility =
                        ((AdjBndVisible && Point.IsBndPoint()) ||
                        (AdjNavVisible && IsNavPoint) ||
                        (AdjMiscVisible && Point.IsMiscPoint())) ?
                        Visibility.Visible : Visibility.Collapsed;

                    UnAdjPushpin.Visibility =
                        ((UnAdjBndVisible && Point.IsBndPoint()) ||
                        (UnAdjNavVisible && IsNavPoint) ||
                        (UnAdjMiscVisible && Point.IsMiscPoint())) ||
                        (WayPointVisible && Point.IsWayPointAtBase()) ?
                        Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    AdjPushpin.Visibility = Visibility.Collapsed;
                    UnAdjPushpin.Visibility = Visibility.Collapsed;
                } 
            }
        }
        #endregion


        public TtMapPoint(Map map, TtPoint point, PolygonGraphicOptions pgo)
        {
            Point = point;

            AdjPushpin.Visibility = Visibility.Collapsed;
            UnAdjPushpin.Visibility = Visibility.Collapsed;

            AdjPushpin.MouseLeftButtonDown += AdjPushpin_MouseLeftButtonDown;
            UnAdjPushpin.MouseLeftButtonDown += UnAdjPushpin_MouseLeftButtonDown;

            AdjPushpin.ToolTipOpening += LoadAdjToolTip;
            UnAdjPushpin.ToolTipOpening += LoadUnAdjToolTip;

            AdjPushpin.ToolTip = String.Empty;
            UnAdjPushpin.ToolTip = String.Empty;

            ToolTipService.SetShowDuration(AdjPushpin, 60000);
            ToolTipService.SetShowDuration(UnAdjPushpin, 60000);


            AdjColor = MediaTools.GetColor(pgo.AdjPtsColor);
            UnAdjColor = MediaTools.GetColor(pgo.UnAdjPtsColor);
            WayPointColor = MediaTools.GetColor(pgo.WayPtsColor);

            IsNavPoint = point.IsNavPoint();

            pgo.ColorChanged += (PolygonGraphicOptions _pgo, GraphicCode code, int color) =>
            {
                switch (code)
                {
                    case GraphicCode.ADJPTS_COLOR:
                        AdjColor = MediaTools.GetColor(color);
                        break;
                    case GraphicCode.UNADJPTS_COLOR:
                        UnAdjColor = MediaTools.GetColor(color);
                        break;
                    case GraphicCode.WAYPTS_COLOR:
                        WayPointColor = MediaTools.GetColor(color);
                        break;
                    default:
                        break;
                }
            };

            point.PropertyChanged += Point_PropertyChanged;
            if (point is QuondamPoint qp)
                qp.ParentPoint.PropertyChanged += Point_PropertyChanged;

            point.LocationChanged += UpdateLocation;
            UpdateLocation(point);

            _Map = map;

            _Map.Children.Add(UnAdjPushpin);
            _Map.Children.Add(AdjPushpin);
        }

        public TtMapPoint(Map map, TtPoint point, PolygonGraphicOptions pgo, bool visible,
            bool adjBndVis, bool unadjBndVis, bool adjNavVis,
            bool unadjNavVis, bool adjMiscVis, bool unadjMiscVis,
            bool wayVis) : this(map, point, pgo)
        {
            _Visible = visible;
            _AdjBndVisible = adjBndVis;
            _UnAdjBndVisible = unadjBndVis;
            _AdjNavVisible = adjNavVis;
            _UnAdjNavVisible = unadjNavVis;
            _AdjMiscVisible = adjMiscVis;
            _UnAdjNavVisible = unadjMiscVis;
            _WayPointVisible = wayVis;

            UpdateVisibility();
        }


        private void Point_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TtPoint.OnBoundary))
            {
                UpdateVisibility();
            }
            else if (e.PropertyName == nameof(TtPoint.PID))
            {
                UnAdjPushpin.ToolTip = new PointInfoBox(this, false);
                AdjPushpin.ToolTip = new PointInfoBox(this, true);
            }
        }

        private void UpdateLocation(TtPoint point)
        {
            if (point is GpsPoint gps)
            {
                if (gps.HasLatLon)
                {
                    AdjPushpin.Location = AdjLocation = new Location((double)gps.Latitude, (double)gps.Longitude);
                    UnAdjPushpin.Location = UnAdjLocation = new Location((double)gps.Latitude, (double)gps.Longitude);

                    LocationChanged?.Invoke(this);
                }
                else if (point.Metadata != null)
                {
                    Point loc = UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.AdjX, point.AdjY, point.Metadata.Zone);
                    AdjPushpin.Location = AdjLocation = new Location(loc.Y, loc.X);
                    UnAdjPushpin.Location = UnAdjLocation = new Location(loc.Y, loc.X);

                    LocationChanged?.Invoke(this);
                }
                else
                {
                    Trace.WriteLine($"Point {point.PID} has no Metadata. ({point.CN})");
                }
            }
            else
            {
                if (point.Metadata != null)
                {
                    Point adj = UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.AdjX, point.AdjY, point.Metadata.Zone);
                    AdjPushpin.Location = AdjLocation = new Location(adj.Y, adj.X);

                    Point unadj = UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);
                    UnAdjPushpin.Location = UnAdjLocation = new Location(unadj.Y, unadj.X);

                    LocationChanged?.Invoke(this);
                }
                else
                {
                    Trace.WriteLine($"Point {point.PID} has no Metadata. ({point.CN})");
                }
            }
        }

        private void LoadUnAdjToolTip(Object sender, ToolTipEventArgs e)
        {
            if (!(UnAdjPushpin.ToolTip is PointInfoBox))
                UnAdjPushpin.ToolTip = new PointInfoBox(this, false);
        }

        private void LoadAdjToolTip(Object sender, ToolTipEventArgs e)
        {
            if (!(AdjPushpin.ToolTip is PointInfoBox))
                AdjPushpin.ToolTip = new PointInfoBox(this, true);
        }

        private void AdjPushpin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PointSelected?.Invoke(this, true);
        }

        private void UnAdjPushpin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_Editing)
                PointSelected?.Invoke(this, false);
        }

        
        public void Detach()
        {
            if (_Map != null)
            {
                _Map.Children.Remove(AdjPushpin);
                _Map.Children.Remove(UnAdjPushpin);
            }
        }

        public override string ToString()
        {
            return $"Point {Point.PID}";
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposed)
        {
            AdjPushpin.MouseLeftButtonDown -= AdjPushpin_MouseLeftButtonDown;
            UnAdjPushpin.MouseLeftButtonDown -= UnAdjPushpin_MouseLeftButtonDown;

            AdjPushpin.ToolTipOpening -= LoadAdjToolTip;
            UnAdjPushpin.ToolTipOpening -= LoadUnAdjToolTip;
        }
    }
}
