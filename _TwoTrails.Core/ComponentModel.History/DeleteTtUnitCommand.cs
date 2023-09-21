
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtUnitCommand : ITtUnitCommand
    {
        private TtManager pointsManager;
        private DeleteTtPointsCommand dpc;

        public DeleteTtUnitCommand(TtUnit polygon, TtManager pointsManager) : base(polygon)
        {
            this.pointsManager = pointsManager;

            dpc = new DeleteTtPointsCommand(pointsManager.GetPoints(polygon.CN), pointsManager);
        }

        public override void Redo()
        {
            dpc.Redo();
            pointsManager.DeleteUnit(Unit);
        }

        public override void Undo()
        {
            pointsManager.AddUnit(Unit);
            dpc.Undo();
        }
    }
}
