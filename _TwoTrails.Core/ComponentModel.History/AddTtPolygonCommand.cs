namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPolygonCommand : ITtPolygonCommand
    {
        private TtManager Manager;

        public AddTtPolygonCommand(TtPolygon polygon, TtManager manager) : base(polygon)
        {
            this.Manager = manager;
        }

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
