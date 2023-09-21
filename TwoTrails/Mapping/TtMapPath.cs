using Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Units;

namespace TwoTrails.Mapping
{
    public class TtMapPath : TtMapShape
    {
        private TtUnit _Unit { get; }

        private MapPolyline _MapPolyline { get; } = new MapPolyline();

        private bool _detached;


        private bool _Visible;
        public override bool Visible
        {
            get { return _Visible; }
            set { SetField(ref _Visible, value, () => _MapPolyline.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed); }
        }

        public bool IsEditing { get; set; }

        public TtMapPath(Map map, TtUnit unit, LocationCollection locations, UnitGraphicOptions pgo, bool adjusted, bool visible) : base(map, unit, locations, pgo)
        {
            _Unit = unit;
            _Visible = visible;

            _MapPolyline.Stroke = new SolidColorBrush(MediaTools.GetColor(pgo.AdjBndColor));
            _MapPolyline.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed;

            _MapPolyline.StrokeThickness = adjusted ?
                pgo.AdjWidth : pgo.UnAdjWidth;

            _MapPolyline.Locations = locations;

            PGO.ColorChanged += OnColorChanged;

            map.Children.Add(_MapPolyline);
        }

        private void OnColorChanged(UnitGraphicOptions pgo, GraphicCode code, int color)
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
            if (_detached)
            {
                PGO.ColorChanged -= OnColorChanged;
                Map.Children.Remove(_MapPolyline);
                _detached = true;
            }
        }
    }
}
