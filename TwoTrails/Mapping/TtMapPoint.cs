﻿using FMSC.Core.ComponentModel;
using FMSC.GeoSpatial.UTM;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using Point = FMSC.Core.Point;

namespace TwoTrails.Mapping
{
    public delegate void MapPointEvent(TtMapPoint point);
    public delegate void MapPointSelectedEvent(TtMapPoint point, bool adjusted);

    public class TtMapPoint : TtMapBaseModel
    {
        public event MapPointEvent LocationChanged;
        public event MapPointSelectedEvent PointSelected;

        public Pushpin AdjPushpin { get; } = new Pushpin();
        public Pushpin UnAdjPushpin { get; } = new Pushpin();
        public Label AdjLabel { get; }
        public Label UnAdjLabel { get; }

        private Location _AdjLoc, _UnAdjLoc;
        public Location AdjLocation { get { return _AdjLoc; } private set { SetField(ref _AdjLoc, value); } }
        public Location UnAdjLocation { get { return _UnAdjLoc; } private set { SetField(ref _UnAdjLoc, value); } }

        public TtPoint Point { get; }
        public bool IsBndPoint { get { return Point.OnBoundary; } }
        public bool IsNavPoint { get; }
        public bool IsMiscPoint { get { return Point.IsMiscPoint(); } }
        public int Index { get { return Point.Index; } }
        
        private object locker = new object();
        private bool _detached = false;

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

        public override bool Visible
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
                        ((AdjBndVisible && Point.OnBoundary) ||
                        (AdjNavVisible && IsNavPoint) ||
                        (AdjMiscVisible && Point.IsMiscPoint())) ?
                        Visibility.Visible : Visibility.Collapsed;

                    UnAdjPushpin.Visibility =
                        ((UnAdjBndVisible && Point.OnBoundary) ||
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


        public TtMapPoint(Map map, TtPoint point, PolygonGraphicOptions pgo) : base(map, pgo)
        {
            Point = point;

            AdjPushpin.Visibility = Visibility.Collapsed;
            UnAdjPushpin.Visibility = Visibility.Collapsed;

            AdjPushpin.MouseLeftButtonDown += AdjPushpin_MouseLeftButtonDown;
            UnAdjPushpin.MouseLeftButtonDown += UnAdjPushpin_MouseLeftButtonDown;

            AdjPushpin.ToolTipOpening += LoadAdjToolTip;
            UnAdjPushpin.ToolTipOpening += LoadUnAdjToolTip;

            AdjLabel = new Label()
            {
                FontSize = 6,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(-5, 0, -5, 0),
                Foreground = new SolidColorBrush(Colors.White),
                Content = point.PID
            };

            UnAdjLabel = new Label()
            {
                FontSize = 6,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(-5, 0, -5, 0),
                Foreground = new SolidColorBrush(Colors.White),
                Content = point.PID
            };

            AdjPushpin.Content = AdjLabel;
            UnAdjPushpin.Content = UnAdjLabel;

            AdjPushpin.ToolTip = String.Empty;
            UnAdjPushpin.ToolTip = String.Empty;

            ToolTipService.SetShowDuration(AdjPushpin, 60000);
            ToolTipService.SetShowDuration(UnAdjPushpin, 60000);


            AdjColor = MediaTools.GetColor(PGO.AdjPtsColor);
            UnAdjColor = MediaTools.GetColor(PGO.UnAdjPtsColor);
            WayPointColor = MediaTools.GetColor(PGO.WayPtsColor);

            IsNavPoint = Point.IsNavPoint();

            PGO.ColorChanged += PGO_ColorChanged;

            Point.PropertyChanged += Point_PropertyChanged;
            if (Point is QuondamPoint qp)
                qp.ParentPoint.PropertyChanged += Point_PropertyChanged;

            Point.LocationChanged += UpdateLocation;
            UpdateLocation(point);
            
            Map.Children.Add(UnAdjPushpin);
            Map.Children.Add(AdjPushpin);
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

                AdjLabel.Content = Point.PID;
                UnAdjLabel.Content = Point.PID;
            }
        }

        private void PGO_ColorChanged(PolygonGraphicOptions _pgo, GraphicCode code, int color)
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

        private void LoadUnAdjToolTip(object sender, ToolTipEventArgs e)
        {
            if (!(UnAdjPushpin.ToolTip is PointInfoBox))
                UnAdjPushpin.ToolTip = new PointInfoBox(this, false);
        }

        private void LoadAdjToolTip(object sender, ToolTipEventArgs e)
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


        public override string ToString()
        {
            return $"Point {Point.PID}";
        }

        public override void Detach()
        {
            if (!_detached)
            {
                AdjPushpin.MouseLeftButtonDown -= AdjPushpin_MouseLeftButtonDown;
                UnAdjPushpin.MouseLeftButtonDown -= UnAdjPushpin_MouseLeftButtonDown;

                AdjPushpin.ToolTipOpening += LoadAdjToolTip;
                UnAdjPushpin.ToolTipOpening += LoadUnAdjToolTip;

                PGO.ColorChanged -= PGO_ColorChanged;

                Point.PropertyChanged -= Point_PropertyChanged;
                if (Point is QuondamPoint qp && qp.ParentPoint != null)
                    qp.ParentPoint.PropertyChanged -= Point_PropertyChanged;

                Point.LocationChanged -= UpdateLocation;

                //Map.Dispatcher.Invoke(() =>
                //{
                    Map.Children.Remove(UnAdjPushpin);
                    Map.Children.Remove(AdjPushpin);
                //});

                _detached = true;
            }
        }
    }
}
