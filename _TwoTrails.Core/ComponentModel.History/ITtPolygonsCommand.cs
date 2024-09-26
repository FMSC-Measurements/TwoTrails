using System;
using System.Collections.Generic;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPolygonsCommand : ITtBaseCommand
    {
        protected List<TtPolygon> Polygons;

        public ITtPolygonsCommand(TtManager manager, IEnumerable<TtPolygon> polygons) : base(manager)
        {
            this.Polygons = new List<TtPolygon>(polygons) ?? throw new ArgumentNullException(nameof(polygons));
        }


        protected override int GetAffectedItemCount() => Polygons.Count;
        protected override String GetCommandInfoDescription() => $"Edit of {Polygons.Count} polygons";
    }
}
