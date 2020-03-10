using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateCorridorDoubleSidedCommand : ITtPointsCommand
    {
        private EditTtPointsCommand editPoints;
        private CreateQuondamsCommand createQuondams;

        public CreateCorridorDoubleSidedCommand(IEnumerable<TtPoint> points, TtPolygon targetPolygon, ITtManager pointsManager) : base(points)
        {
            IEnumerable<TtPoint> ssPoints = points.Where(p => p.OpType == OpType.SideShot);
            List<TtPoint> s2ssPoints = new List<TtPoint>();

            TtPoint lastPoint = points.Last();

            bool flip = false;
            foreach (TtPoint p in (lastPoint.OpType == OpType.SideShot) ? ssPoints.TakeWhile(p => p.CN != lastPoint.CN) : ssPoints)
            {
                if (flip)
                    s2ssPoints.Add(p);

                flip = !flip;
            }

            s2ssPoints.Reverse();
            
            editPoints = new EditTtPointsCommand(s2ssPoints.Concat(points.Where(p => p.IsGpsType())), PointProperties.BOUNDARY, false);
            createQuondams = new CreateQuondamsCommand(s2ssPoints, pointsManager, targetPolygon, int.MaxValue, QuondamBoundaryMode.On);
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
