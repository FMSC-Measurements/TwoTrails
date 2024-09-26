using System.Reflection;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtGroupCommand<T> : ITtGroupCommand
    {
        private readonly T NewValue;
        private readonly T OldValue;
        private readonly PropertyInfo Property;

        public EditTtGroupCommand(TtManager manager, TtGroup group, PropertyInfo property, T newValue) : base(manager, group)
        {
            this.Property = property;
            this.NewValue = newValue;
            this.OldValue = (T)property.GetValue(group);
        }

        public override void Redo()
        {
            Property.SetValue(Group, NewValue);
        }

        public override void Undo()
        {
            Property.SetValue(Group, OldValue);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedGroups;
        protected override string GetCommandInfoDescription() => $"Edit {Property.Name} of group {Group.Name}";
    }
}
