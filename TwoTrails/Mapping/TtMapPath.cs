using Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Media;
using TwoTrails.Core;

namespace TwoTrails.Mapping
{
    public class TtMapPath : TtMapShape
    {
        private TtPolygon _Polygon { get; }

        private MapPolyline _MapPolyline { get; } = new MapPolyline();

        private PolygonGraphicOptions _PGO { get; }

        private bool _Visible;
        public override bool Visible
        {
            get { return _Visible; }
            set { SetField(ref _Visible, value, () => _MapPolyline.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed); }
        }

        public bool IsEditing { get; set; }

        public TtMapPath(Map map, TtPolygon polygon, LocationCollection locations, PolygonGraphicOptions pgo, bool adjusted, bool visible) : base(map, polygon, locations, pgo)
        {
            _Polygon = polygon;
            _Visible = visible;
            _PGO = pgo;

            _MapPolyline.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
            _MapPolyline.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed;

            _MapPolyline.StrokeThickness = adjusted ?
                pgo.AdjWidth : pgo.UnAdjWidth;

            _MapPolyline.Locations = locations;

            _PGO.ColorChanged += OnColorChanged;

            map.Children.Add(_MapPolyline);
        }

        private void OnColorChanged(PolygonGraphicOptions pgo, GraphicCode code, int color)
        {
            switch (code)
            {
                case GraphicCode.ADJNAV_COLOR:
                    _MapPolyline.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjNavColor));
                    break;
                case GraphicCode.UNADJNAV_COLOR:
                    _MapPolyline.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.UnAdjNavColor));
                    break;
            }
        }

        protected override void UpdateShape(LocationCollection locations)
        {
            Application.Current.Dispatcher.Invoke(() => {
                _MapPolyline.Locations = locations;

                Map.Refresh();
            });
        }

        public override void Detach()
        {
            _PGO.ColorChanged -= OnColorChanged;
            Map.Children.Remove(_MapPolyline);
        }
    }
}
