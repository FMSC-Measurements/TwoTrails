namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPolygonCommand : ITtPolygonCommand
    {
        public AddTtPolygonCommand(TtManager manager, TtPolygon polygon) : base(manager, polygon) { }

        public override void Redo()
        {
            Manager.AddPolygon(Polygon);
        }

        public override void Undo()
        {
            Manager.DeletePolygon(Polygon);
        }

        protected override DataActionType GetActionType() => DataActionType.InsertedPolygons;
        protected override string GetCommandInfoDescription() => $"Add Polygon {Polygon.Name}";
    }
}
