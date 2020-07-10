using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateCorridorCommand : ITtPointsCommand
    {
        private EditTtPointsCommand editPoints;
        private CreateQuondamsCommand createQuondams;

        public CreateCorridorCommand(IEnumerable<TtPoint> points, TtPolygon targetPolygon, TtManager pointsManager) : base(points)
        {
            IEnumerable<TtPoint> ssPoints = points.Where(p => p.OpType == OpType.SideShot);

            TtPoint lastPoint = points.Last();
            if (lastPoint.OpType == OpType.SideShot)
                ssPoints = ssPoints.TakeWhile(p => p.CN != lastPoint.CN);
            
            editPoints = new EditTtPointsCommand(ssPoints, PointProperties.BOUNDARY, false);
            createQuondams = new CreateQuondamsCommand(ssPoints.Reverse(), pointsManager, targetPolygon, int.MaxValue, QuondamBoundaryMode.On);
        }

        public override void Redo()
        {
            editPoints.Redo();
            createQuondams.Redo();
        }

        public override void Undo()
        {
            createQuondams.Undo();
            editPoints.Undo();
        }
    }
}
