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
            this.Points = new List<TtPoint>(points);
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
