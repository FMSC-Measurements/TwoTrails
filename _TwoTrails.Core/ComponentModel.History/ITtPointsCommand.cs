using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPointsCommand : ITtCommand
    {
        public bool RequireRefresh { get; protected set; } = true;

        protected List<TtPoint> Points;

        public ITtPointsCommand(IEnumerable<TtPoint> points)
        {
            this.Points = new List<TtPoint>(points) ?? throw new ArgumentNullException(nameof(points));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
