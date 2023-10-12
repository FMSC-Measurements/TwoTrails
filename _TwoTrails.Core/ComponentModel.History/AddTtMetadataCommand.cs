﻿namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtMetadataCommand : ITtMetadataCommand
    {
        private TtManager pointsManager;

        public AddTtMetadataCommand(TtMetadata metadata, TtManager pointsManager) : base(metadata)
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

        protected override DataActionType GetActionType() => DataActionType.InsertedMetadata;
        protected override string GetCommandInfoDescription() => $"Add Metadata {Metadata.Name}";
    }
}
