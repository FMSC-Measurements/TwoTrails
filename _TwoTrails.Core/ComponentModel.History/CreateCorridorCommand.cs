using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateCorridorCommand : ITtPointsCommand
    {
        private EditTtPointsCommand editPoints;
        private CreateQuondamsCommand createQuondams;

        public CreateCorridorCommand(IEnumerable<TtPoint> points, TtPolygon targetPolygon, ITtManager pointsManager, bool autoCommit = true) : base(points)
        {
            IEnumerable<TtPoint> ssPoints = points.Where(p => p.OpType == OpType.SideShot);

            TtPoint lastPoint = points.Last();
            if (lastPoint.OpType == OpType.SideShot)
                ssPoints = ssPoints.TakeWhile(p => p.CN != lastPoint.CN);
                

            editPoints = new EditTtPointsCommand(ssPoints, PointProperties.BOUNDARY, false, false);
            createQuondams = new CreateQuondamsCommand(ssPoints.Reverse(), pointsManager, targetPolygon, int.MaxValue, true, false);
            
            if (autoCommit)
                Redo();
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
