using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtGroupCommand : ITtBaseCommand
    {
        protected TtGroup Group;

        public ITtGroupCommand(TtManager manager, TtGroup group) : base(manager)
        {
            this.Group = group ?? throw new ArgumentNullException(nameof(group));
        }

        protected override int GetAffectedItemCount() => 1;
        protected override String GetCommandInfoDescription() => "Group";
    }
}
