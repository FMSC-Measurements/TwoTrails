using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPolygonCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        public Type DataType => PolygonProperties.DataType;

        protected TtUnit Polygon;

        public ITtPolygonCommand(TtUnit polygon)
        {
            this.Polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
