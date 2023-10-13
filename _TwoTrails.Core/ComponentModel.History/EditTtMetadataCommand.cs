using System.Reflection;

namespace TwoTrails.Core.ComponentModel.History
{
    public class EditTtMetadataCommand<T> : ITtMetadataCommand
    {
        private readonly T NewValue;
        private readonly T OldValue;
        private readonly PropertyInfo Property;

        public EditTtMetadataCommand(TtManager manager, TtMetadata metadata, PropertyInfo property, T newValue) : base(manager, metadata)
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
