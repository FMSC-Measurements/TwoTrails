using Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Media;
using TwoTrails.Core;

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
            set { SetField(ref _Visible, value, () => MapPolygon.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed); }
        }

        public bool IsEditing { get; set; }

        public TtMapPolygon(Map map, TtPolygon polygon, LocationCollection locations, PolygonGraphicOptions pgo, bool adjusted, bool visible) : base(map, polygon, locations, pgo)
        {
            _Polygon = polygon;
            _Visible = visible;

            MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
            MapPolygon.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed;

            MapPolygon.StrokeThickness = adjusted ?
                pgo.AdjWidth : pgo.UnAdjWidth;

            MapPolygon.Locations = locations;

            pgo.ColorChanged += (PolygonGraphicOptions _pgo, GraphicCode code, int color) =>
            {
                switch (code)
                {
                    case GraphicCode.ADJBND_COLOR:
                        MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
                        break;
                    case GraphicCode.UNADJBND_COLOR:
                        MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.UnAdjBndColor));
                        break;
                }
            };

            map.Children.Add(MapPolygon);
        }

        protected override void UpdateShape(LocationCollection locations)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MapPolygon.Locations = locations;

                Map.Refresh();
            });
        }

        public override void Detach()
        {
            Map.Children.Remove(MapPolygon);
        }
    }
}
