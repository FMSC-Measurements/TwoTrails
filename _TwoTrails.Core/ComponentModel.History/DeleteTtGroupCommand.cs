namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtGroupCommand : ITtGroupCommand
    {
        private TtManager pointsManager;

        public DeleteTtGroupCommand(TtGroup group, TtManager pointsManager) : base(group)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.DeleteGroup(Group);
        }

        public override void Undo()
        {
            pointsManager.AddGroup(Group);
        }

        protected override string GetCommandInfoDescription() => $"Delete Group {Group.Name}";
    }
}
