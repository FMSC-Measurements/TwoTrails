using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ReplaceTtPointCommand : ITtPointCommand
    {
        private readonly TtPoint _ReplacedPoint;

        public ReplaceTtPointCommand(TtManager manager, TtPoint point) : base(manager, point)
        {
            _ReplacedPoint = manager.GetPoint(point.CN);
        }

        public override void Redo()
        {
            Manager.ReplacePoint(Point);
        }

        public override void Undo()
        {
            Manager.ReplacePoint(_ReplacedPoint);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedPoints;
        protected override string GetCommandInfoDescription() => $"Repalce point {Point.PID} with {_ReplacedPoint.PID}";
    }
}
