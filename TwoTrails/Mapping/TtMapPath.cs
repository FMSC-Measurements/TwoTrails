using System.Collections.Generic;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.Utils;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace TwoTrails.Mapping
{
    public class TtMapPath : TtMapShape
    {
        private TtPolygon _Polygon { get; }

        private MapPolyline MapPolyline { get; } = new MapPolyline();

        private bool _Visible;
        public override bool Visible
        {
            get { return _Visible; }
            set { SetField(ref _Visible, value, () => MapPolyline.Visible = _Visible); }
        }

        public bool IsEditing { get; set; }

        public TtMapPath(MapControl map, TtPolygon polygon, IEnumerable<BasicGeoposition> locations, PolygonGraphicOptions pgo, bool adjusted, bool visible) : base(map, polygon, locations, pgo)
        {
            _Polygon = polygon;
            _Visible = visible;

            MapPolyline.StrokeColor = TtUtils.GetColor(pgo.AdjBndColor);
            //MapPolyline.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
            MapPolyline.Visible = _Visible;
            //MapPolyline.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed;

            MapPolyline.StrokeThickness = adjusted ?
                pgo.AdjWidth : pgo.UnAdjWidth;

            MapPolyline.Path = new Geopath(locations);
            //MapPolyline.Locations = locations;

            pgo.ColorChanged += (PolygonGraphicOptions _pgo, GraphicCode code, int color) =>
            {
                switch (code)
                {
                    case GraphicCode.ADJNAV_COLOR:
                        MapPolyline.StrokeColor = TtUtils.GetColor(pgo.AdjNavColor);
                        //MapPolyline.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjNavColor));
                        break;
                    case GraphicCode.UNADJNAV_COLOR:
                        MapPolyline.StrokeColor = TtUtils.GetColor(pgo.UnAdjNavColor);
                        //MapPolyline.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.UnAdjNavColor));
                        break;
                }
            };

            map.Children.Add(MapPolyline);
        }

        protected override void UpdateShape(IEnumerable<BasicGeoposition> locations)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MapPolyline.Path = new Geopath(locations);
                //MapPolyline.Locations = locations;

                Map.UpdateLayout();
                //Map.Refresh();
            });
        }

        public override void Detach()
        {
            Map.Children.Remove(MapPolyline);
        }
    }
}
