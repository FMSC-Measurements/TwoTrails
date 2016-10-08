using CSUtil.ComponentModel;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Mapping
{
    public abstract class TtMapShape : NotifyPropertyChangedEx
    {
        public abstract bool Visible { get; set; }

        protected Map Map { get; }


        public TtMapShape(Map map, TtPolygon polygon, LocationCollection locations, PolygonGraphicOptions pgo)
        {
            this.Map = map;
        }

        public void UpdateLocations(LocationCollection locations)
        {
            UpdateShape(locations);
        }

        protected abstract void UpdateShape(LocationCollection locations);

        public abstract void Detach();
    }
}
