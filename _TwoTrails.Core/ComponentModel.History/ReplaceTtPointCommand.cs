using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ReplaceTtPointCommand : ITtPointCommand
    {
        private TtManager _Manager;
        private TtPoint _ReplacedPoint;

        public ReplaceTtPointCommand(TtPoint point, TtManager pointsManager) : base(point)
        {
            this._Manager = pointsManager;
            
            _ReplacedPoint = pointsManager.GetPoint(point.CN);
        }

        public override void Redo()
        {
            _Manager.ReplacePoint(Point);
        }

        public override void Undo()
        {
            _Manager.ReplacePoint(_ReplacedPoint);
        }

        protected override string GetCommandInfoDescription() => $"Repalce point {Point.PID} with {_ReplacedPoint.PID}";
    }
}
