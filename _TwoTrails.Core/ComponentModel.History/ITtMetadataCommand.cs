﻿using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtMetadataCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        protected TtMetadata metadata;

        public ITtMetadataCommand(TtMetadata metadata)
        {
            this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
