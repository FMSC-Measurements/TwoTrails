using FMSC.Core.ComponentModel;
using Microsoft.Maps.MapControl.WPF;
using TwoTrails.Core;

namespace TwoTrails.Mapping
{
    public abstract class TtMapBaseModel : BaseModel
    {
        public abstract bool Visible { get; set; }

        protected Map Map { get; }

        protected UnitGraphicOptions PGO { get; }


        public TtMapBaseModel(Map map, UnitGraphicOptions pgo)
        {
            this.Map = map;
            this.PGO = pgo;
        }

        public abstract void Detach();
    }
}
