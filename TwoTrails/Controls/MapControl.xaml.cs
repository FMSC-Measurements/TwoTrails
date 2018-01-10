using FMSC.GeoSpatial.UTM;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoTrails.Core;
using TwoTrails.Mapping;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl
    {
        public static readonly DependencyProperty ManagerProperty =
                DependencyProperty.Register(nameof(Manager), typeof(TtManager), typeof(MapControl));

        public TtManager Manager
        {
            get { return (TtManager)this.GetValue(ManagerProperty); }
            set { this.SetValue(ManagerProperty, value); }
        }

        public TtMapManager MapManager { get; private set; }

        public bool IsLatLon { get; private set; } = false;

        public MapControl()
        {
            InitializeComponent();
            map.CredentialsProvider = new ApplicationIdCredentialsProvider(APIKeys.BING_MAPS_API_KEY);

            this.Loaded += MapControl_Loaded;

            map.Mode = new AerialMode();

            map.MouseMove += Map_MouseMove;
        }

        public MapControl(TtManager manager) : this()
        {
            Manager = manager;
        }

        private void MapControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Manager != null)
            {
                if (MapManager == null)
                    MapManager = new TtMapManager(map, Manager);

                lvPolygons.ItemsSource = MapManager.PolygonManagers; 
            }
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            Location loc = map.ViewportPointToLocation(e.GetPosition(map));

            if (IsLatLon)
                tbLoc.Text = $"Lat: { loc.Latitude.ToString("F6") }  Lon: { loc.Longitude.ToString("F6") }";
            else
            {
                UTMCoords coords = UTMTools.ConvertLatLonSignedDecToUTM(loc.Latitude, loc.Longitude, Manager.DefaultMetadata.Zone);

                tbLoc.Text = $"[{ coords.Zone }]  X: { coords.X.ToString("F2") }  Y: { coords.Y.ToString("F2") }";
            }
        }
    }
}
