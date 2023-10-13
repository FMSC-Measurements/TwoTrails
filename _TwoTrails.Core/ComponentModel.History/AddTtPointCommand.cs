using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPointCommand : ITtPointCommand
    {
        public AddTtPointCommand(TtManager manager, TtPoint point) : base(manager, point) { }

        public override void Redo()
        {
            Manager.AddPoint(Point);
        }

        public override void Undo()
        {
            Manager.DeletePoint(Point);
        }

        protected override DataActionType GetActionType() => DataActionType.InsertedPoints;
        protected override string GetCommandInfoDescription() => $"Add Point {Point.PID}";
    }
}
