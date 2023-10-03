using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPointsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;

        public AddTtPointsCommand(IEnumerable<TtPoint> points, TtManager pointsManager) : base(points)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.AddPoints(Points);
        }

        public override void Undo()
        {
            pointsManager.DeletePoints(Points);
        }

        protected override string GetCommandInfoDescription() => $"Add {Points.Count} points";
    }
}
