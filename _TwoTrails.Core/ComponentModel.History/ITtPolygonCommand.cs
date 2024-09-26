using System;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPolygonCommand : ITtBaseCommand
    {
        protected TtPolygon Polygon;

        public ITtPolygonCommand(TtManager manager, TtPolygon polygon) : base(manager)
        {
            this.Polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
        }


        protected override int GetAffectedItemCount() => 1;
        protected override String GetCommandInfoDescription() => "Polygon";
    }
}
