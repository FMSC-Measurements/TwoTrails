using FMSC.Core.ComponentModel;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;

namespace TwoTrails.Mapping
{
    public abstract class TtMapBaseModel : BaseModel
    {
        public abstract bool Visible { get; set; }

        protected Map Map { get; }

        protected PolygonGraphicOptions PGO { get; }


        public TtMapBaseModel(Map map, PolygonGraphicOptions pgo)
        {
            this.Map = map;
            this.PGO = pgo;
        }
    }
}
