namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtMetadataCommand : ITtMetadataCommand
    {
        private TtManager pointsManager;

        public DeleteTtMetadataCommand(TtMetadata metadata, TtManager pointsManager) : base(metadata)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.DeleteMetadata(Metadata);
        }

        public override void Undo()
        {
            pointsManager.AddMetadata(Metadata);
        }
    }
}
