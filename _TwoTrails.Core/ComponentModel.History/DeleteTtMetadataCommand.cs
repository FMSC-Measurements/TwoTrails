namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtMetadataCommand : ITtMetadataCommand
    {
        public DeleteTtMetadataCommand(TtManager manager, TtMetadata metadata) : base(manager, metadata) { }

        public override void Redo()
        {
            Manager.DeleteMetadata(Metadata);
        }

        public override void Undo()
        {
            Manager.AddMetadata(Metadata);
        }

        protected override DataActionType GetActionType() => DataActionType.DeletedMetadata;
        protected override string GetCommandInfoDescription() => $"Delete Metadata {Metadata.Name}";
    }
}
