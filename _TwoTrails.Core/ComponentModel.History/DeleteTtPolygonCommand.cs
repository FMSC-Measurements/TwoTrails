
namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPolygonCommand : ITtPolygonCommand
    {
        private TtManager pointsManager;
        private DeleteTtPointsCommand dpc;

        public DeleteTtPolygonCommand(TtPolygon polygon, TtManager pointsManager, bool autoCommit = true) : base(polygon)
        {
            this.pointsManager = pointsManager;

            dpc = new DeleteTtPointsCommand(pointsManager.GetPoints(polygon.CN), pointsManager, false);

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            dpc.Redo();
            pointsManager.DeletePolygon(polygon);
        }

        public override void Undo()
        {
            pointsManager.AddPolygon(polygon);
            dpc.Undo();
        }
    }
}
