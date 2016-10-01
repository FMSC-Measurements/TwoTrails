using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core.Points;
using System.Windows;
using FMSC.GeoSpatial.UTM;
using System.Diagnostics;
using System.Windows.Input;
using CSUtil.ComponentModel;
using System.Windows.Media;
using System.ComponentModel;

namespace TwoTrails.Mapping
{
    public delegate void MapPointEvent(MapPoint point);

    public class MapPoint : NotifyPropertyChangedEx
    {
        public event MapPointEvent LocationChanged;
        public event MapPointEvent AdjPointSelected;
        public event MapPointEvent UnAdjPointSelected;

        public Pushpin AdjPushpin { get; } = new Pushpin();
        public Pushpin UnAdjPushpin { get; } = new Pushpin();

        private Location _AdjLoc, _UnAdjLoc;
        public Location AdjLocation { get { return _AdjLoc; } private set { SetField(ref _AdjLoc, value); } }
        public Location UnAdjLocation { get { return _UnAdjLoc; } private set { SetField(ref _UnAdjLoc, value); } }

        private TtPoint _Point { get; }
        public bool IsBndPoint { get { return _Point.IsBndPoint(); } }
        public bool IsNavPoint { get; }
        public bool IsMiscPoint { get { return _Point.IsMiscPoint(); } }

        private Map _Map;


        public Color AdjColor { get; }
        public Color UnAdjColor { get; }

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


        #region Visibility
        private bool _Visible;
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


        private void UpdateVisibility()
        {
            lock (locker)
            {
                if (_Visible)
                {
                    AdjPushpin.Visibility =
                        ((AdjBndVisible && _Point.IsBndPoint()) ||
                        (AdjNavVisible && IsNavPoint) ||
                        (AdjMiscVisible && _Point.IsMiscPoint())) ?
                        Visibility.Visible : Visibility.Collapsed;

                    UnAdjPushpin.Visibility =
                        ((UnAdjBndVisible && _Point.IsBndPoint()) ||
                        (UnAdjNavVisible && IsNavPoint) ||
                        (UnAdjMiscVisible && _Point.IsMiscPoint())) ?
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


        public MapPoint(Map map, TtPoint point)
        {
            _Point = point;

            AdjPushpin.Visibility = Visibility.Collapsed;
            UnAdjPushpin.Visibility = Visibility.Collapsed;

            AdjPushpin.MouseLeftButtonDown += AdjPushpin_MouseLeftButtonDown;
            UnAdjPushpin.MouseLeftButtonDown += UnAdjPushpin_MouseLeftButtonDown;


            point.PropertyChanged += Point_PropertyChanged;
            point.LocationChanged += UpdateLocation;
            UpdateLocation(point);

            Attach(map);
        }

        private void Point_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TtPoint.OnBoundary))
            {
                UpdateVisibility();
            }
        }

        private void UpdateLocation(TtPoint point)
        {
            GpsPoint gps = point as GpsPoint;
            if (gps != null && gps.HasLatLon)
            {
                AdjLocation = new Location((double)gps.Longitude, (double)gps.Latitude);
                AdjPushpin.Location = AdjLocation;
                UnAdjPushpin.Location = UnAdjLocation = AdjLocation;
            }
            else
            {
                if (point.Metadata == null)
                {
                    Debug.WriteLine(String.Format("Point {0} has no Metadata. ({1})", point.PID, point.CN));
                }
                else
                {
                    Point adj = UTMTools.convertUTMtoLatLonSignedDecAsPoint(point.AdjX, point.AdjY, point.Metadata.Zone);
                    AdjLocation = new Location(adj.Y, adj.X);
                    AdjPushpin.Location = AdjLocation;

                    Point unadj = UTMTools.convertUTMtoLatLonSignedDecAsPoint(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);
                    UnAdjLocation = new Location(adj.Y, adj.X);
                    UnAdjPushpin.Location = UnAdjLocation;
                }
            }

            LocationChanged?.Invoke(this);
        }



        private void AdjPushpin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AdjPointSelected?.Invoke(this);
        }

        private void UnAdjPushpin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_Editing)
                UnAdjPointSelected?.Invoke(this);
        }



        public void Attach(Map map)
        {
            Detach();

            _Map = map;

            _Map.Children.Add(AdjPushpin);
            _Map.Children.Add(UnAdjPushpin);
        }

        public void Detach()
        {
            if (_Map != null)
            {
                _Map.Children.Remove(AdjPushpin);
                _Map.Children.Remove(UnAdjPushpin);
            }
        }
    }
}
