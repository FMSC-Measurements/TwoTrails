using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPolygonCommand : ITtPolygonCommand
    {
        private ITtManager pointsManager;

        public AddTtPolygonCommand(TtPolygon polygon, ITtManager pointsManager) : base(polygon)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.AddPolygon(polygon);
        }

        public override void Undo()
        {
            pointsManager.DeletePolygon(polygon);
        }
    }
}
