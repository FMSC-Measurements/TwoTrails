using CSUtil.ComponentModel;
using System.Collections.Generic;
using TwoTrails.Core;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace TwoTrails.Mapping
{
    public abstract class TtMapShape : NotifyPropertyChangedEx
    {
        public abstract bool Visible { get; set; }

        protected MapControl Map { get; }


        public TtMapShape(MapControl map, TtPolygon polygon, IEnumerable<BasicGeoposition> locations, PolygonGraphicOptions pgo)
        {
            this.Map = map;
        }

        public void UpdateLocations(IEnumerable<BasicGeoposition> locations)
        {
            UpdateShape(locations);
        }

        protected abstract void UpdateShape(IEnumerable<BasicGeoposition> locations);

        public abstract void Detach();
    }
}
