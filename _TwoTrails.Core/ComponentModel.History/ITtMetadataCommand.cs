using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtMetadataCommand : ITtBaseCommand
    {
        public Type DataType => MetadataProperties.DataType;

        protected TtMetadata Metadata;


        public ITtMetadataCommand(TtMetadata metadata)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        protected override Type GetAffectedType() => MetadataProperties.DataType;
        protected override int GetAffectedItemCount() => 1;
        protected override String GetCommandInfoDescription() => "Metadata";
    }
}
