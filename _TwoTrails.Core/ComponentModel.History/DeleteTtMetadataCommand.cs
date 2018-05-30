namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtMetadataCommand : ITtMetadataCommand
    {
        private ITtManager pointsManager;

        public DeleteTtMetadataCommand(TtMetadata metadata, ITtManager pointsManager) : base(metadata)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.DeleteMetadata(metadata);
        }

        public override void Undo()
        {
            pointsManager.AddMetadata(metadata);
        }
    }
}
