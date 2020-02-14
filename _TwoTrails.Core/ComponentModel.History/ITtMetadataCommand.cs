using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtMetadataCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        public Type DataType => MetadataProperties.DataType;

        protected TtMetadata Metadata;

        public ITtMetadataCommand(TtMetadata metadata)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
