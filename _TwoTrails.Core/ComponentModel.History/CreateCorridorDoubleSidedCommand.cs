using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateCorridorDoubleSidedCommand : ITtPointsCommand
    {
        private EditTtPointsCommand _EditPointsCommand;
        private CreateQuondamsCommand _CreateQuondamsCommand;

        private readonly TtPolygon _TargetPolygon;

        public CreateCorridorDoubleSidedCommand(IEnumerable<TtPoint> points, TtPolygon targetPolygon, TtManager pointsManager) : base(points)
        {
            IEnumerable<TtPoint> ssPoints = points.Where(p => p.OpType == OpType.SideShot);
            List<TtPoint> s2ssPoints = new List<TtPoint>();

            _TargetPolygon = targetPolygon;

            TtPoint lastPoint = points.Last();

            bool flip = false;
            foreach (TtPoint p in (lastPoint.OpType == OpType.SideShot) ? ssPoints.TakeWhile(p => p.CN != lastPoint.CN) : ssPoints)
            {
                if (flip)
                    s2ssPoints.Add(p);

                flip = !flip;
            }

            s2ssPoints.Reverse();
            
            _EditPointsCommand = new EditTtPointsCommand(s2ssPoints.Concat(points.Where(p => p.IsGpsType())), PointProperties.BOUNDARY, false);
            _CreateQuondamsCommand = new CreateQuondamsCommand(s2ssPoints, pointsManager, targetPolygon, int.MaxValue, QuondamBoundaryMode.On);
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
