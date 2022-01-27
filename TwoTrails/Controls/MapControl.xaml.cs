using FMSC.Core.Utilities;
using FMSC.GeoSpatial.UTM;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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


        private readonly KeyEventHandler KeyDownHandler, KeyUpHandler;
        public bool CtrlKeyPressed { get; private set; }

        public bool HasManager => Manager != null;

        public TtMapManager MapManager { get; private set; }

        public bool IsLatLon { get; private set; } = false;
        

        public MapControl()
        {
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

            InitializeComponent();

            map.Loaded += OnMapLoaded;
            map.MouseMove += OnMouseMove;

            map.CredentialsProvider = new ApplicationIdCredentialsProvider(APIKeys.BING_MAPS_API_KEY);
            map.Mode = new AerialMode();

            KeyDownHandler = new KeyEventHandler(OnKeyDown);
            KeyUpHandler = new KeyEventHandler(OnKeyUp);
        }

        public MapControl(TtProject project) : this()
        {
            Project = project;
        }


        private void OnLoaded(object sender, EventArgs e)
        {
            if (Manager != null && MapManager == null)
            {
                MapManager = new TtMapManager(this, map, Manager);
                DefaultPolygonGraphicBrushOptions = new PolygonGraphicBrushOptions(null, Manager.GetDefaultPolygonGraphicOption());
                PolygonVisibilityControl = new PolygonVisibilityControl(MapManager.PolygonManagers, DefaultPolygonGraphicBrushOptions);
                DataContext = this;

                AddHandler(MapControl.KeyDownEvent, KeyDownHandler);
                AddHandler(MapControl.KeyUpEvent, KeyUpHandler);
            }

            this.Loaded -= OnLoaded;

            SortPolys();

            Project.Settings.PropertyChanged += (s, pce) =>
            {
                if (pce.PropertyName == nameof(TtSettings.SortPolysByName)) SortPolys();
            };
        }

        private void OnUnloaded(object sender, EventArgs e)
        {
            RemoveHandler(MapControl.KeyDownEvent, KeyDownHandler);
            RemoveHandler(MapControl.KeyUpEvent, KeyUpHandler);

            this.Unloaded -= OnUnloaded;
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


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                CtrlKeyPressed = true;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                CtrlKeyPressed = false;
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

        private void SortPolys()
        {
            lvPolygons.Items.SortDescriptions.Clear();
            lvPolygons.Items.SortDescriptions.Add(new SortDescription($"Polygon.{(Project.Settings.SortPolysByName ? "Name" : "TimeCreated")}", ListSortDirection.Ascending));
        }
    }
}
