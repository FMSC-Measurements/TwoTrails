namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtGroupCommand : ITtGroupCommand
    {
        private TtManager pointsManager;

        public AddTtGroupCommand(TtGroup group, TtManager pointsManager) : base(group)
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

        protected override DataActionType GetActionType() => DataActionType.InsertedGroups;
        protected override string GetCommandInfoDescription() => $"Add Group {Group.Name}";
    }
}
