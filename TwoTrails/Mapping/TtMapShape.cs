using FMSC.Core.ComponentModel;
using Microsoft.Maps.MapControl.WPF;
using TwoTrails.Core;

namespace TwoTrails.Mapping
{
    public abstract class TtMapShape : TtMapBaseModel
    {
        public TtMapShape(Map map, TtPolygon polygon, LocationCollection locations, PolygonGraphicOptions pgo) : base(map, pgo)
        {
        }

        public void UpdateLocations(LocationCollection locations)
        {
            UpdateShape(locations);
        }

        protected abstract void UpdateShape(LocationCollection locations);
    }
}
