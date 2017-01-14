namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtGroupCommand : ITtGroupCommand
    {
        private ITtManager pointsManager;

        public DeleteTtGroupCommand(TtGroup group, ITtManager pointsManager, bool autoCommit = true) : base(group)
        {
            this.pointsManager = pointsManager;

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.DeleteGroup(group);
        }

        public override void Undo()
        {
            pointsManager.AddGroup(group);
        }
    }
}
