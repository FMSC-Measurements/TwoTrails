using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class RetraceCommand : ITtPointsCommand
    {
        private readonly TtPolygon _Polygon;
        private readonly CreateQuondamsCommand _CreateQuondamsCommand;
        private readonly AddDataActionCommand _AddDataActionCommand;

        public RetraceCommand(TtManager manager, IEnumerable<TtPoint> points, TtPolygon targetPoly, int insertIndex, QuondamBoundaryMode bndMode = QuondamBoundaryMode.Inherit) : base(manager, points)
        {
            _Polygon = targetPoly;
            _CreateQuondamsCommand = new CreateQuondamsCommand(manager, points, targetPoly, insertIndex, bndMode);
            _AddDataActionCommand = new AddDataActionCommand(manager, DataActionType.RetracePoints);
        }

        public override void Redo()
        {
            _CreateQuondamsCommand.Redo();
            _AddDataActionCommand.Redo();
        }

        public override void Undo()
        {
            _AddDataActionCommand.Undo();
            _CreateQuondamsCommand.Undo();
        }

        protected override DataActionType GetActionType() => _CreateQuondamsCommand.CommandInfo.ActionType | DataActionType.RetracePoints;
        protected override string GetCommandInfoDescription() => $"Retrace {Points.Count} points to unit {_Polygon.Name}";
    }
}
