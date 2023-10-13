namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtMetadataCommand : ITtMetadataCommand
    {
        public AddTtMetadataCommand(TtMetadata metadata, TtManager manager) : base(manager, metadata) { }

        public override void Redo()
        {
            Manager.AddMetadata(Metadata);
        }

        public override void Undo()
        {
            Manager.DeleteMetadata(Metadata);
        }

        protected override DataActionType GetActionType() => DataActionType.InsertedMetadata;
        protected override string GetCommandInfoDescription() => $"Add Metadata {Metadata.Name}";
    }
}
