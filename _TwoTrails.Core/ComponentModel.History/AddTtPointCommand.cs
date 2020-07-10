using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPointCommand : ITtPointCommand
    {
        private TtManager pointsManager;

        public AddTtPointCommand(TtPoint point, TtManager pointsManager) : base(point)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.AddPoint(Point);
        }

        public override void Undo()
        {
            pointsManager.DeletePoint(Point);
        }
    }
}
