using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Mapping
{
    public class MapPath : MapShape
    {
        private bool _Visible;
        public override bool Visible { get { return _Visible; } set { _Visible = value; } }

        public MapPath(Map map, TtPolygon polygon, IList<Location> points) : base(map, polygon, points)
        {

        }

        public override void Detach()
        {

        }
    }
}
