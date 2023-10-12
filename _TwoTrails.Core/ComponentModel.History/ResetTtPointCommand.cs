using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ResetTtPointCommand : ITtPointCommand
    {
        private TtManager _Manager;
        private TtPoint _ResetPoint;

        public ResetTtPointCommand(TtPoint point, TtManager pointsManager, bool keepIndexAndPoly = false) : base(point)
        {
            this._Manager = pointsManager;

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
            _Manager.ReplacePoint(_ResetPoint);
        }

        public override void Undo()
        {
            _Manager.ReplacePoint(Point);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Reset point {Point}";
    }
}
