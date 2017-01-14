namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtGroupCommand : ITtGroupCommand
    {
        private ITtManager pointsManager;

        public AddTtGroupCommand(TtGroup group, ITtManager pointsManager, bool autoCommit = true) : base(group)
        {
            this.pointsManager = pointsManager;

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.AddGroup(group);
        }

        public override void Undo()
        {
            pointsManager.DeleteGroup(group);
        }
    }
}
