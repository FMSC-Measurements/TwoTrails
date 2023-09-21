using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtUnitCommand : ITtUnitCommand
    {
        private TtManager Manager;

        public AddTtUnitCommand(TtUnit unit, TtManager manager) : base(unit)
        {
            this.Manager = manager;
        }

        public override void Redo()
        {
            Manager.AddUnit(Unit);
        }

        public override void Undo()
        {
            Manager.DeleteUnit(Unit);
        }
    }
}
