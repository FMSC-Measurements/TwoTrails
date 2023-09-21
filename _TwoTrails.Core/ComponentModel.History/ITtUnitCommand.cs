using System;
using TwoTrails.Core.Units;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtUnitCommand : ITtCommand
    {
        public bool RequireRefresh { get; } = false;

        public Type DataType => UnitProperties.DataType;

        protected TtUnit Unit;

        public ITtUnitCommand(TtUnit unit)
        {
            this.Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
