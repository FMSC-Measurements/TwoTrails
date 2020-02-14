namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPolygonCommand : ITtPolygonCommand
    {
        private ITtManager Manager;

        public AddTtPolygonCommand(TtPolygon polygon, ITtManager manager) : base(polygon)
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
    }
}
