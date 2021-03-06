﻿namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtMetadataCommand : ITtMetadataCommand
    {
        private ITtManager pointsManager;

        public AddTtMetadataCommand(TtMetadata metadata, ITtManager pointsManager) : base(metadata)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.AddMetadata(Metadata);
        }

        public override void Undo()
        {
            pointsManager.DeleteMetadata(Metadata);
        }
    }
}
