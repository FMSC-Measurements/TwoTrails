namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPolygonCommand : ITtPolygonCommand
    {
        private readonly DeleteTtPointsCommand _DeletePointsCommand;

        public DeleteTtPolygonCommand(TtManager manager, TtPolygon polygon) : base(manager, polygon)
        {
            _DeletePointsCommand = new DeleteTtPointsCommand(manager, manager.GetPoints(polygon.CN));
        }

        public override void Redo()
        {
            _DeletePointsCommand.Redo();
            Manager.DeletePolygon(Polygon);
        }

        public override void Undo()
        {
            Manager.AddPolygon(Polygon);
            _DeletePointsCommand.Undo();
        }

        protected override int GetAffectedItemCount() => (_DeletePointsCommand != null ? _DeletePointsCommand.CommandInfo.AffectedItems : 0) + 1;
        protected override DataActionType GetActionType() => DataActionType.DeletedPolygons | _DeletePointsCommand.CommandInfo.ActionType;
        protected override string GetCommandInfoDescription() => $"Delete polygon {Polygon.Name} ({_DeletePointsCommand.CommandInfo.AffectedItems} points affected)";
    }
}
