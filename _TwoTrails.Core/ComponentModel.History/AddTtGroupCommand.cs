namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtGroupCommand : ITtGroupCommand
    {
        public AddTtGroupCommand(TtManager manager, TtGroup group) : base(manager, group) { }

        public override void Redo()
        {
            Manager.AddGroup(Group);
        }

        public override void Undo()
        {
            Manager.DeleteGroup(Group);
        }

        protected override DataActionType GetActionType() => DataActionType.InsertedGroups;
        protected override string GetCommandInfoDescription() => $"Add Group {Group.Name}";
    }
}
