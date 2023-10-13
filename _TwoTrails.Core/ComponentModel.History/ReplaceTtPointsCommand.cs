using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ReplaceTtPointsCommand : ITtPointsCommand
    {
        private List<TtPoint> _ReplacedPoints = null;


        public ReplaceTtPointsCommand(TtManager manager, IEnumerable<TtPoint> points) : base(manager, points)
        {
            _ReplacedPoints = Points.Select(pt => manager.GetPoint(pt.CN)).ToList();
        }

        public override void Redo()
        {
            if (_ReplacedPoints != null && _ReplacedPoints.Count > 0)
            {
                Manager.ReplacePoints(Points);
            }
        }

        public override void Undo()
        {
            if (_ReplacedPoints != null && _ReplacedPoints.Count > 0)
            {
                Manager.ReplacePoints(_ReplacedPoints);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Repalce {Points.Count} points";
    }
}
