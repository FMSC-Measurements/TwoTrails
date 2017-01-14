using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPolygonCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        protected TtPolygon polygon;

        public ITtPolygonCommand(TtPolygon polygon)
        {
            this.polygon = polygon;
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
