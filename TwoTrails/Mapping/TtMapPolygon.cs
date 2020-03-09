using System.Collections.Generic;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.Utils;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace TwoTrails.Mapping
{
    public class TtMapPolygon : TtMapShape
    {
        private TtPolygon _Polygon { get; }

        private MapPolygon MapPolygon { get; } = new MapPolygon();

        private bool _Visible;
        public override bool Visible
        {
            get { return _Visible; }
            set { SetField(ref _Visible, value, () => MapPolygon.Visible = _Visible); }
        }

        public bool IsEditing { get; set; }

        public TtMapPolygon(MapControl map, TtPolygon polygon, IEnumerable<BasicGeoposition> locations, PolygonGraphicOptions pgo, bool adjusted, bool visible) : base(map, polygon, locations, pgo)
        {
            _Polygon = polygon;
            _Visible = visible;

            MapPolygon.StrokeColor = TtUtils.GetColor(pgo.AdjBndColor);
            //MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
            MapPolygon.Visible = _Visible;
            //MapPolygon.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed;

            MapPolygon.StrokeThickness = adjusted ?
                pgo.AdjWidth : pgo.UnAdjWidth;

            MapPolygon.Path = new Geopath(locations);
            //MapPolygon.Locations = locations;

            pgo.ColorChanged += (PolygonGraphicOptions _pgo, GraphicCode code, int color) =>
            {
                switch (code)
                {
                    case GraphicCode.ADJBND_COLOR:
                        MapPolygon.StrokeColor = TtUtils.GetColor(pgo.AdjBndColor);
                        //MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
                        break;
                    case GraphicCode.UNADJBND_COLOR:
                        MapPolygon.StrokeColor = TtUtils.GetColor(pgo.UnAdjBndColor);
                        //MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.UnAdjBndColor));
                        break;
                }
            };

            map.Children.Add(MapPolygon);
        }

        protected override void UpdateShape(IEnumerable<BasicGeoposition> locations)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MapPolygon.Path = new Geopath(locations);
                //MapPolygon.Locations = locations;

                Map.UpdateLayout();
                //Map.Refresh();
            });
        }

        public override void Detach()
        {
            Map.Children.Remove(MapPolygon);
        }
    }
}
