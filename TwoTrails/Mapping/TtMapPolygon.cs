using Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Media;
using TwoTrails.Core;

namespace TwoTrails.Mapping
{
    public class TtMapPolygon : TtMapShape
    {
        private TtPolygon _Polygon { get; }

        private MapPolygon _MapPolygon { get; } = new MapPolygon();

        private bool _detached;


        private bool _Visible;
        public override bool Visible
        {
            get { return _Visible; }
            set { SetField(ref _Visible, value, () => _MapPolygon.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed); }
        }

        public bool IsEditing { get; set; }

        public TtMapPolygon(Map map, TtPolygon polygon, LocationCollection locations, PolygonGraphicOptions pgo, bool adjusted, bool visible) : base(map, polygon, locations, pgo)
        {
            _Polygon = polygon;
            _Visible = visible;

            _MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
            _MapPolygon.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed;

            _MapPolygon.StrokeThickness = adjusted ?
                pgo.AdjWidth : pgo.UnAdjWidth;

            _MapPolygon.Locations = locations;

            PGO.ColorChanged += OnColorChanged;

            map.Children.Add(_MapPolygon);
        }

        private void OnColorChanged(PolygonGraphicOptions pgo, GraphicCode code, int color)
        {
            switch (code)
            {
                case GraphicCode.ADJBND_COLOR:
                    _MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
                    break;
                case GraphicCode.UNADJBND_COLOR:
                    _MapPolygon.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.UnAdjBndColor));
                    break;
            }
        }

        protected override void UpdateShape(LocationCollection locations)
        {
            Application.Current.Dispatcher.Invoke(() => {
                _MapPolygon.Locations = locations;

                Map.Refresh();
            });
        }

        public override void Detach()
        {
            if (_detached)
            {
                PGO.ColorChanged -= OnColorChanged;
                Map.Children.Remove(_MapPolygon);
                _detached = true;
            }
        }
    }
}
