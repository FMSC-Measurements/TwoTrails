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
    public abstract class MapShape
    {
        public abstract bool Visible { get; set; }

        protected Map Map { get; }


        public MapShape(Map map, TtPolygon polygon, IList<Location> points)
        {
            this.Map = map;
        }

        public void UpdateShape(IList<Location> points)
        {

        }

        public abstract void Detach();
    }
}
