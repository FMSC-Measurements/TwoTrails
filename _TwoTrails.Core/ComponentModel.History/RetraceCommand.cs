using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class RetraceCommand : ITtPointsCommand
    {
        private readonly TtManager _Manager;
        private readonly TtPolygon _Polygon;
        private readonly CreateQuondamsCommand _CreateQuondamsCommand;
        private readonly Guid _ID = Guid.NewGuid();

        public RetraceCommand(IEnumerable<TtPoint> points, TtManager pointsManager, TtPolygon targetPoly, int insertIndex, QuondamBoundaryMode bndMode = QuondamBoundaryMode.Inherit) : base(points)
        {
            _Manager = pointsManager;
            _CreateQuondamsCommand = new CreateQuondamsCommand(points, pointsManager, targetPoly, insertIndex, bndMode);
        }

        public override void Redo()
        {
            _CreateQuondamsCommand.Redo();

            _Manager.AddAction(DataActionType.RetracePoints, null, _ID);
        }

        public override void Undo()
        {
            _CreateQuondamsCommand.Undo();

            _Manager.RemoveAction(_ID);
        }

        protected override string GetCommandInfoDescription() => $"Retrace {Points.Count} points to unit {_Polygon.Name}";
    }
}
