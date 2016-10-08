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
    public class TtMapPath : TtMapShape
    {
        private bool _Visible;
        public override bool Visible { get { return _Visible; } set { SetField(ref _Visible, value); } }

        public TtMapPath(Map map, TtPolygon polygon, LocationCollection locations, PolygonGraphicOptions pgo, bool adjusted) : base(map, polygon, locations, pgo)
        {

        }

        protected override void UpdateShape(LocationCollection locations)
        {

        }


        public override void Detach()
        {

        }
    }
}
