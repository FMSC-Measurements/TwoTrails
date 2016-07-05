using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtMetadataCommand : ITtMetadataCommand
    {
        private ITtManager pointsManager;

        public AddTtMetadataCommand(TtMetadata metadata, ITtManager pointsManager, bool autoCommit = true) : base(metadata)
        {
            this.pointsManager = pointsManager;

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.AddMetadata(metadata);
        }

        public override void Undo()
        {
            pointsManager.DeleteMetadata(metadata);
        }
    }
}
