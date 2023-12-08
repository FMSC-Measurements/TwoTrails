namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtBaseCommand : ITtCommand
    {
        protected readonly TtManager Manager;

        private CommandInfo _commandInfo;
        public CommandInfo CommandInfo =>_commandInfo ??
            (_commandInfo = new CommandInfo(GetActionType(), GetCommandInfoDescription(), GetAffectedItemCount()));

        public bool RequireRefresh { get; protected set; } = true;


        public ITtBaseCommand(TtManager manager)
        {
            Manager = manager;
        }


        public abstract void Undo();
        public abstract void Redo();

        protected abstract DataActionType GetActionType();
        protected virtual int GetAffectedItemCount() => 0;
        protected abstract string GetCommandInfoDescription();
    }
}
