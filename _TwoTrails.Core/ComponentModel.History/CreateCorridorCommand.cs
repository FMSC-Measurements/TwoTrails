using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateCorridorCommand : ITtPointsCommand
    {
        private readonly EditTtPointsCommand _EditPointsCommand;
        private readonly CreateQuondamsCommand _CreateQuondamsCommand;

        private readonly TtPolygon _TargetPolygon;

        public CreateCorridorCommand(IEnumerable<TtPoint> points, TtPolygon targetPolygon, TtManager pointsManager) : base(points)
        {
            IEnumerable<TtPoint> ssPoints = points.Where(p => p.OpType == OpType.SideShot);
            _TargetPolygon = targetPolygon;

            TtPoint lastPoint = points.Last();
            if (lastPoint.OpType == OpType.SideShot)
                ssPoints = ssPoints.TakeWhile(p => p.CN != lastPoint.CN);
            
            _EditPointsCommand = new EditTtPointsCommand(ssPoints, PointProperties.BOUNDARY, false);
            _CreateQuondamsCommand = new CreateQuondamsCommand(ssPoints.Reverse(), pointsManager, targetPolygon, int.MaxValue, QuondamBoundaryMode.On);
        }

        public override void Redo()
        {
            _EditPointsCommand.Redo();
            _CreateQuondamsCommand.Redo();
        }

        public override void Undo()
        {
            _CreateQuondamsCommand.Undo();
            _EditPointsCommand.Undo();
        }

        protected override int GetAffectedItemCount() => _CreateQuondamsCommand.CommandInfo.AffectedItems + _EditPointsCommand.CommandInfo.AffectedItems;
        protected override string GetCommandInfoDescription() => $"Create Corridor in unit {_TargetPolygon.Name}";
    }
}
