using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ResetTtPointCommand : ITtPointCommand
    {
        private readonly TtPoint _ResetPoint;

        public ResetTtPointCommand(TtManager manager, TtPoint point, bool keepIndexAndPoly = false) : base(manager, point)
        {
            _ResetPoint = manager.GetOriginalPoint(point.CN).DeepCopy();

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
            Manager.ReplacePoint(_ResetPoint);
        }

        public override void Undo()
        {
            Manager.ReplacePoint(Point);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Reset point {Point}";
    }
}
