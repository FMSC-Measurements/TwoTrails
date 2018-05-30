using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPointCommand : ITtPointCommand
    {
        private ITtManager pointsManager;

        public AddTtPointCommand(TtPoint point, ITtManager pointsManager) : base(point)
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
