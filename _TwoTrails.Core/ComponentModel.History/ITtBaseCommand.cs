using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtBaseCommand : ITtCommand
    {
        private CommandInfo _commandInfo;
        public CommandInfo CommandInfo =>_commandInfo ??
            (_commandInfo = new CommandInfo(GetAffectedType(), GetCommandInfoDescription(), GetAffectedItemCount()));

        public bool RequireRefresh { get; protected set; } = true;

        public abstract void Undo();
        public abstract void Redo();

        protected abstract Type GetAffectedType();
        protected virtual int GetAffectedItemCount() => 0;
        protected abstract string GetCommandInfoDescription();
    }
}
