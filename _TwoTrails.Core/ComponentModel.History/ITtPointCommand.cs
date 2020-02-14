using System;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPointCommand : ITtCommand
    {
        public bool RequireRefresh { get; protected set; } = true;

        public Type DataType => Point.GetType();

        protected TtPoint Point;

        public ITtPointCommand(TtPoint point)
        {
            this.Point = point ?? throw new ArgumentNullException(nameof(point));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
