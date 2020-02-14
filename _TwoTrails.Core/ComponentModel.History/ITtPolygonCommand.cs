using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPolygonCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        protected TtPolygon Polygon;

        public ITtPolygonCommand(TtPolygon polygon)
        {
            this.Polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
