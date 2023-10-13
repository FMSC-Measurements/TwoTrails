using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ResetTtPointsCommand : ITtPointsCommand
    {
        private readonly List<TtPoint> _ResetPoints = null;


        public ResetTtPointsCommand(TtManager manager, IEnumerable<TtPoint> points, bool keepIndexAndPoly = false) : base(manager, points.Where(p => manager.HasOriginalPoint(p.CN)))
        {
            _ResetPoints = Points.Select(pt => manager.GetOriginalPoint(pt.CN).DeepCopy()).ToList();

            if (keepIndexAndPoly)
            {
                foreach (TtPoint point in Points)
                {
                    TtPoint resetPoint = _ResetPoints.First(p => p.CN == point.CN);
                    resetPoint.LocationChangedEventEnabled = false;
                    resetPoint.Index = point.Index;
                    resetPoint.Polygon = point.Polygon;
                    resetPoint.LocationChangedEventEnabled = true;
                }
            }
        }

        public override void Redo()
        {
            if (_ResetPoints != null && _ResetPoints.Count > 0)
            {
                Manager.ReplacePoints(_ResetPoints);
            }
        }

        public override void Undo()
        {
            if (_ResetPoints != null && _ResetPoints.Count > 0)
            {
                Manager.ReplacePoints(Points);
            }
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Reset {Points.Count} points";
    }
}
