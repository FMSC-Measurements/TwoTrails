using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPointsCommand : ITtPointsCommand
    {
        public AddTtPointsCommand(TtManager manager, IEnumerable<TtPoint> points) : base(manager, points) { }

        public override void Redo()
        {
            Manager.AddPoints(Points);
        }

        public override void Undo()
        {
            Manager.DeletePoints(Points);
        }

        protected override DataActionType GetActionType() => DataActionType.InsertedPoints;
        protected override string GetCommandInfoDescription() => $"Add {Points.Count} points";
    }
}
