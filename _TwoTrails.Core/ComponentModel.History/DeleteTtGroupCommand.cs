namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtGroupCommand : ITtGroupCommand
    {
        public DeleteTtGroupCommand(TtManager manager, TtGroup group) : base(manager, group) { }

        public override void Redo()
        {
            Manager.DeleteGroup(Group);
        }

        public override void Undo()
        {
            Manager.AddGroup(Group);
        }

        protected override DataActionType GetActionType() => DataActionType.DeletedGroups;
        protected override string GetCommandInfoDescription() => $"Delete Group {Group.Name}";
    }
}
