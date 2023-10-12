using System.Reflection;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtMetadataCommand<T> : ITtMetadataCommand
    {
        private T NewValue;
        private T OldValue;
        private PropertyInfo Property;

        public EditTtMetadataCommand(TtMetadata metadata, PropertyInfo property, T newValue) : base(metadata)
        {
            this.Property = property;
            this.NewValue = newValue;
            this.OldValue = (T)property.GetValue(metadata);
        }

        public override void Redo()
        {
            Property.SetValue(Metadata, NewValue);
        }

        public override void Undo()
        {
            Property.SetValue(Metadata, OldValue);
        }

        protected override DataActionType GetActionType() => DataActionType.ModifiedMetadata;
        protected override string GetCommandInfoDescription() => $"Edit {Property.Name} of metadata {Metadata.Name}";
    }
}
