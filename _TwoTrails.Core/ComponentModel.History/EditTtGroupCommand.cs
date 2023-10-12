using System.Reflection;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtGroupCommand<T> : ITtGroupCommand
    {
        private T NewValue;
        private T OldValue;
        private PropertyInfo Property;

        public EditTtGroupCommand(TtGroup group, PropertyInfo property, T newValue) : base(group)
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
