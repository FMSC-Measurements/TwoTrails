using FMSC.Core.Utilities;
using FMSC.GeoSpatial.UTM;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Mapping;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        public static readonly DependencyProperty ManagerProperty =
                DependencyProperty.Register(nameof(Manager), typeof(IObservableTtManager), typeof(MapControl));
        
        public IObservableTtManager Manager
        {
            get { return (IObservableTtManager)this.GetValue(ManagerProperty); }
            set
            {
                this.SetValue(ManagerProperty, value);
                HasManager = value != null;
            }
        }

        public PolygonVisibilityControl PolygonVisibilityControl { get; set; }

        public bool HasManager { get; set; }

        public TtMapManager MapManager { get; private set; }

        public bool IsLatLon { get; private set; } = false;
        

        public MapControl()
        {
            InitializeComponent();
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(APIKeys.BING_MAPS_API_KEY);
            
            map.Mode = new AerialMode();

            map.Loaded += OnMapLoaded;
            map.MouseMove += OnMouseMove;

            this.Loaded += OnLoaded;
            this.Unloaded += (s, e) => {
                map.MouseMove -= OnMouseMove;

                if (PolygonVisibilityControl != null)
                    PolygonVisibilityControl.Dispose();

                if (MapManager != null)
                    MapManager.Dispose();
            };
        }


        public MapControl(IObservableTtManager manager) : this()
        {
            Manager = manager;
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            if (Manager != null && MapManager == null)
            {
                MapManager = new TtMapManager(map, Manager);
                PolygonVisibilityControl = new PolygonVisibilityControl(MapManager.PolygonManagers, new PolygonGraphicBrushOptions(null, Manager.GetDefaultPolygonGraphicOption()));
                DataContext = this;
            }

            this.Loaded -= OnLoaded;
        }

        private void OnMapLoaded(object sender, EventArgs e)
        {
            if (map.ActualHeight > 0 && MapManager != null)
            {
                IEnumerable<Location> locs = MapManager.PolygonManagers.SelectMany(mpm => mpm.Points.Select(p => p.AdjLocation));
                if (locs.Any())
                    map.SetView(locs, new Thickness(30), 0);

                map.Loaded -= OnMapLoaded;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Location loc = map.ViewportPointToLocation(e.GetPosition(map));

            if (IsLatLon)
                tbLoc.Text = $"Lat: { loc.Latitude:F6}  Lon: { loc.Longitude:F6}";
            else
            {
                UTMCoords coords = UTMTools.ConvertLatLonSignedDecToUTM(loc.Latitude, loc.Longitude,
                    HasManager ? Manager.DefaultMetadata.Zone : 0);

                tbLoc.Text = $"[{ coords.Zone }]  X: { coords.X:F2}  Y: { coords.Y:F2}";
            }
        }


        private void ColapseAllPolyControl(object sender, MouseButtonEventArgs e)
        {
            gridAllPolyCtrls.Visibility = gridAllPolyCtrls.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ZoomToAllPolys(object sender, RoutedEventArgs e)
        {
            IEnumerable<Location> locs = MapManager.PolygonManagers.SelectMany(mpm => mpm.Points.Select(p => p.AdjLocation));
            if (locs.Any())
                map.SetView(locs, new Thickness(30), 0);
        }
    }
}
