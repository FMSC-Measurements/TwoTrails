using FMSC.GeoSpatial.UTM;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private TtProject Project { get; set; }

        public IObservableTtManager Manager => Project.HistoryManager;
        public bool SortPolysByName => Project.Settings.SortPolysByName;

        public PolygonVisibilityControl PolygonVisibilityControl { get; private set; }
        public PolygonGraphicBrushOptions DefaultPolygonGraphicBrushOptions { get; private set; }


        public bool HasManager => Manager != null;

        public TtMapManager MapManager { get; private set; }

        public bool IsLatLon { get; private set; } = false;
        

        public MapControl()
        {
            this.Loaded += OnLoaded;

            InitializeComponent();

            map.Loaded += OnMapLoaded;
            map.MouseMove += OnMouseMove;

            map.CredentialsProvider = new ApplicationIdCredentialsProvider(APIKeys.BING_MAPS_API_KEY);
            map.Mode = new AerialMode();
        }

        public MapControl(TtProject project) : this()
        {
            Project = project;
        }


        private void OnLoaded(object sender, EventArgs e)
        {
            if (Manager != null && MapManager == null)
            {
                MapManager = new TtMapManager(this, map, Project);
                DefaultPolygonGraphicBrushOptions = new PolygonGraphicBrushOptions(null, Manager.GetDefaultPolygonGraphicOption());
                PolygonVisibilityControl = new PolygonVisibilityControl(MapManager.PolygonManagers, DefaultPolygonGraphicBrushOptions);
                DataContext = this;
            }

            this.Loaded -= OnLoaded;
        }


        private void OnMapLoaded(object sender, EventArgs e)
        {
            if (map.ActualHeight > 0 && MapManager != null)
            {
                ZoomToAllPolys(sender, null);

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
