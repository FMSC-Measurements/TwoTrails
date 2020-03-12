using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class RetraceCommand : ITtPointsCommand
    {
        private TtHistoryManager _Manager;
        private CreateQuondamsCommand _CreateQuondamsCommand;

        public RetraceCommand(IEnumerable<TtPoint> points, TtHistoryManager pointsManager, TtPolygon targetPoly, int insertIndex, QuondamBoundaryMode bndMode = QuondamBoundaryMode.Inherit) : base(points)
        {
            _Manager = pointsManager;
            _CreateQuondamsCommand = new CreateQuondamsCommand(points, pointsManager, targetPoly, insertIndex, bndMode);
        }

        public override void Redo()
        {
            _CreateQuondamsCommand.Redo();

            _Manager.BaseManager.AddAction(DataActionType.RetracePoints);
        }

        public override void Undo()
        {
            _Manager.BaseManager.RemoveLastAction();

            _CreateQuondamsCommand.Undo();
        }
    }
}
