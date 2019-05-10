using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ResetTtPointCommand : ITtPointCommand
    {
        private TtManager pointsManager;
        private TtPoint _ResetPoint;

        public ResetTtPointCommand(TtPoint point, TtManager pointsManager, bool keepIndexAndPoly = false) : base(point)
        {
            this.pointsManager = pointsManager;

            _ResetPoint = pointsManager.GetOriginalPoint(point.CN).DeepCopy();

            if (keepIndexAndPoly)
            {
                _ResetPoint.LocationChangedEventEnabled = false;
                _ResetPoint.Index = point.Index;
                _ResetPoint.Polygon = point.Polygon;
                _ResetPoint.LocationChangedEventEnabled = true;
            }
        }

        public override void Redo()
        {
            pointsManager.ReplacePoint(_ResetPoint);
        }

        public override void Undo()
        {
            pointsManager.ReplacePoint(Point);
        }
    }
}
