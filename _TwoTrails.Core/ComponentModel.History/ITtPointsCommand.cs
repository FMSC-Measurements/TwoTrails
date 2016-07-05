using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPointsCommand : ITtCommand
    {
        protected List<TtPoint> points;

        public ITtPointsCommand(List<TtPoint> points)
        {
            this.points = points;
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
