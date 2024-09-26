using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateCorridorCommand : ITtPointsCommand
    {
        private readonly TtPolygon _TargetPolygon;

        private readonly EditTtPointsCommand _EditPointsCommand;
        private readonly CreateQuondamsCommand _CreateQuondamsCommand;


        public CreateCorridorCommand(TtManager manager, IEnumerable<TtPoint> points, TtPolygon targetPolygon) : base(manager, points)
        {
            IEnumerable<TtPoint> ssPoints = points.Where(p => p.OpType == OpType.SideShot);
            _TargetPolygon = targetPolygon;

            TtPoint lastPoint = points.Last();
            if (lastPoint.OpType == OpType.SideShot)
                ssPoints = ssPoints.TakeWhile(p => p.CN != lastPoint.CN);
            
            _EditPointsCommand = new EditTtPointsCommand(manager, ssPoints, PointProperties.BOUNDARY, false);
            _CreateQuondamsCommand = new CreateQuondamsCommand(manager, ssPoints.Reverse(), targetPolygon, int.MaxValue, QuondamBoundaryMode.On);
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
        protected override DataActionType GetActionType() => _EditPointsCommand.CommandInfo.ActionType | _CreateQuondamsCommand.CommandInfo.ActionType;
        protected override string GetCommandInfoDescription() => $"Create Corridor in unit {_TargetPolygon.Name}";
    }
}
