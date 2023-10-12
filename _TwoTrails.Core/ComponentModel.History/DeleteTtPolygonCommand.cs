namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPolygonCommand : ITtPolygonCommand
    {
        private TtManager _Manager;
        private DeleteTtPointsCommand _DeletePointsCommand;

        public DeleteTtPolygonCommand(TtPolygon polygon, TtManager pointsManager) : base(polygon)
        {
            this._Manager = pointsManager;

            _DeletePointsCommand = new DeleteTtPointsCommand(pointsManager.GetPoints(polygon.CN), pointsManager);
        }

        public override void Redo()
        {
            _DeletePointsCommand.Redo();
            _Manager.DeletePolygon(Polygon);
        }

        public override void Undo()
        {
            _Manager.AddPolygon(Polygon);
            _DeletePointsCommand.Undo();
        }

        protected override DataActionType GetActionType() => DataActionType.DeletedPolygons | _DeletePointsCommand.CommandInfo.ActionType;
        protected override string GetCommandInfoDescription() => $"Delete polygon {Polygon.Name} ({_DeletePointsCommand.CommandInfo.AffectedItems} points affected)";
    }
}
