using System.Linq;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPolygonCommand : ITtPolygonCommand
    {
        private readonly DeleteTtPointsCommand _DeletePointsCommand;
        private readonly EditTtPolygonsCommand _EditPolygonsCommand;

        public DeleteTtPolygonCommand(TtManager manager, TtPolygon polygon) : base(manager, polygon)
        {
            _DeletePointsCommand = new DeleteTtPointsCommand(manager, manager.GetPoints(polygon.CN));
            _EditPolygonsCommand = new EditTtPolygonsCommand(manager, manager.Polygons.Where(p => p.ParentUnitCN == polygon.CN), PolygonProperties.PARENT_UNIT_CN, null);
        }

        public override void Redo()
        {
            _EditPolygonsCommand.Redo();
            _DeletePointsCommand.Redo();
            Manager.DeletePolygon(Polygon);
        }

        public override void Undo()
        {
            Manager.AddPolygon(Polygon);
            _DeletePointsCommand.Undo();
            _EditPolygonsCommand.Undo();
        }

        protected override int GetAffectedItemCount() => 
            (_DeletePointsCommand != null ? _DeletePointsCommand.CommandInfo.AffectedItems : 0) +
            (_EditPolygonsCommand != null ? _EditPolygonsCommand.CommandInfo.AffectedItems : 0) + 1;
        protected override DataActionType GetActionType() => DataActionType.DeletedPolygons | _DeletePointsCommand.CommandInfo.ActionType;
        protected override string GetCommandInfoDescription() =>
            $"Delete polygon {Polygon.Name} ({_DeletePointsCommand.CommandInfo.AffectedItems} points" + 
            $"{(_EditPolygonsCommand.CommandInfo.AffectedItems > 0 ? $" and {_EditPolygonsCommand.CommandInfo.AffectedItems} exclusions" : "")} affected)";
    }
}
