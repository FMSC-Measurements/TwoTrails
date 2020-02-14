
namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPolygonCommand : ITtPolygonCommand
    {
        private TtManager pointsManager;
        private DeleteTtPointsCommand dpc;

        public DeleteTtPolygonCommand(TtPolygon polygon, TtManager pointsManager) : base(polygon)
        {
            this.pointsManager = pointsManager;

            dpc = new DeleteTtPointsCommand(pointsManager.GetPoints(polygon.CN), pointsManager);
        }

        public override void Redo()
        {
            dpc.Redo();
            pointsManager.DeletePolygon(Polygon);
        }

        public override void Undo()
        {
            pointsManager.AddPolygon(Polygon);
            dpc.Undo();
        }
    }
}
