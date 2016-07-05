﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtMetadataCommand : ITtCommand
    {
        protected TtMetadata metadata;

        public ITtMetadataCommand(TtMetadata metadata)
        {
            this.metadata = metadata;
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
