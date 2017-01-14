using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Points;

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
            set { SetField(ref _Visible, value, () => MapPolyline.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed); }
        }

        public bool IsEditing { get; set; }

        public TtMapPath(Map map, TtPolygon polygon, LocationCollection locations, PolygonGraphicOptions pgo, bool adjusted, bool visible) : base(map, polygon, locations, pgo)
        {
            _Polygon = polygon;
            _Visible = visible;

            MapPolyline.Stroke = new SolidColorBrush(pgo.AdjBndColor);
            MapPolyline.Visibility = _Visible ? Visibility.Visible : Visibility.Collapsed;

            MapPolyline.StrokeThickness = adjusted ?
                pgo.AdjWidth : pgo.UnAdjWidth;

            MapPolyline.Locations = locations;

            pgo.ColorChanged += (PolygonGraphicOptions _pgo, GraphicCode code, Color color) =>
            {
                switch (code)
                {
                    case GraphicCode.ADJNAV_COLOR:
                        MapPolyline.Stroke = new SolidColorBrush(pgo.AdjNavColor);
                        break;
                    case GraphicCode.UNADJNAV_COLOR:
                        MapPolyline.Stroke = new SolidColorBrush(pgo.UnAdjNavColor);
                        break;
                }
            };

            map.Children.Add(MapPolyline);
        }

        protected override void UpdateShape(LocationCollection locations)
        {
            Application.Current.Dispatcher.Invoke(() => {
                MapPolyline.Locations = locations;

                Map.Refresh();
            });
        }

        public override void Detach()
        {
            Map.Children.Remove(MapPolyline);
        }
    }
}
