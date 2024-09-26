using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtMetadataCommand : ITtBaseCommand
    {
        protected TtMetadata Metadata;


        public ITtMetadataCommand(TtManager manager, TtMetadata metadata) : base(manager)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        protected override int GetAffectedItemCount() => 1;
        protected override String GetCommandInfoDescription() => "Metadata";
    }
}
