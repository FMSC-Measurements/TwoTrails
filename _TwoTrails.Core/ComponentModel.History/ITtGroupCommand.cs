using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtGroupCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        protected TtGroup group;

        public ITtGroupCommand(TtGroup group)
        {
            this.group = group;
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
