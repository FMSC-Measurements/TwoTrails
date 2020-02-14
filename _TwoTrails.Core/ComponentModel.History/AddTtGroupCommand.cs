namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtGroupCommand : ITtGroupCommand
    {
        private ITtManager pointsManager;

        public AddTtGroupCommand(TtGroup group, ITtManager pointsManager) : base(group)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.AddGroup(Group);
        }

        public override void Undo()
        {
            pointsManager.DeleteGroup(Group);
        }
    }
}
