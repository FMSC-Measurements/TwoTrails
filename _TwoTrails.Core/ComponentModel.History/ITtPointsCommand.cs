using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtPointsCommand : ITtBaseCommand
    {
        protected List<TtPoint> Points;

        public ITtPointsCommand(IEnumerable<TtPoint> points)
        {
            this.Points = new List<TtPoint>(points) ?? throw new ArgumentNullException(nameof(points));
        }


        protected override int GetAffectedItemCount() => Points.Count;
        protected override String GetCommandInfoDescription() => $"Edit of {Points.Count} points";
    }
}
