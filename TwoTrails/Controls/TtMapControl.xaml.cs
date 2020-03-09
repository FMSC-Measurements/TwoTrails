using FMSC.GeoSpatial.UTM;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Mapping;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for TtMapControl.xaml
    /// </summary>
    public partial class TtMapControl : UserControl
    {
        public static readonly DependencyProperty ManagerProperty =
                DependencyProperty.Register(nameof(Manager), typeof(IObservableTtManager), typeof(TtMapControl));
        
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
        

        public TtMapControl()
        {
            InitializeComponent();
            //map.CredentialsProvider = new ApplicationIdCredentialsProvider(APIKeys.BING_MAPS_API_KEY);

            map.MapServiceToken = APIKeys.BING_MAPS_API_KEY;

            //map.Mode = new AerialMode();

            map.Loaded += async (s, e) =>
            {
                if (map.ActualHeight > 0 && MapManager != null)
                {
                    IEnumerable<BasicGeoposition> locs = MapManager.PolygonManagers.SelectMany(mpm => mpm.Points.Select(p => p.AdjLocation));
                    if (locs.Any())
                    {
                        MapScene scene = MapScene.CreateFromLocations(locs.Select(bgp => new Geopoint(bgp)));
                        _ = (s as MapControl).TrySetSceneAsync(scene);

                        //map.SetView(locs, new Thickness(30), 0);
                    }
                }
            };

            map.MouseMove += (s, e) =>
            {
                //Location loc = map.ViewportPointToLocation(e.GetPosition(map));

                //if (IsLatLon)
                //    tbLoc.Text = $"Lat: { loc.Latitude.ToString("F6") }  Lon: { loc.Longitude.ToString("F6") }";
                //else
                //{
                //    UTMCoords coords = UTMTools.ConvertLatLonSignedDecToUTM(loc.Latitude, loc.Longitude,
                //        HasManager ? Manager.DefaultMetadata.Zone : 0);

                //    tbLoc.Text = $"[{ coords.Zone }]  X: { coords.X.ToString("F2") }  Y: { coords.Y.ToString("F2") }";
                //}
            };

            this.Loaded += (s, e) =>
            {
                if (Manager != null && MapManager == null)
                {
                    MapManager = new TtMapManager(s as MapControl, Manager);
                    PolygonVisibilityControl = new PolygonVisibilityControl(MapManager.PolygonManagers, new PolygonGraphicBrushOptions(null, Manager.GetDefaultPolygonGraphicOption()));
                    DataContext = this;
                }
            };
        }

        public TtMapControl(IObservableTtManager manager) : this()
        {
            Manager = manager;
        }


        private void ColapseAllPolyControl(object sender, MouseButtonEventArgs e)
        {
            gridAllPolyCtrls.Visibility = gridAllPolyCtrls.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ZoomToAllPolys(object sender, RoutedEventArgs e)
        {
            //IEnumerable<Location> locs = MapManager.PolygonManagers.SelectMany(mpm => mpm.Points.Select(p => p.AdjLocation));
            //if (locs.Any())
            //    map.SetView(locs, new Thickness(30), 0);
        }
    }
}
