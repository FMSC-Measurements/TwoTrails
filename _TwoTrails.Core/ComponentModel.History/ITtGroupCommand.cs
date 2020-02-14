using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtGroupCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        public Type DataType => GroupProperties.DataType;

        protected TtGroup Group;

        public ITtGroupCommand(TtGroup group)
        {
            this.Group = group ?? throw new ArgumentNullException(nameof(group));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
