namespace TwoTrails.Core.ComponentModel.History
{
    public abstract class ITtGroupCommand : ITtCommand
    {
        protected TtGroup group;

        public ITtGroupCommand(TtGroup group)
        {
            this.group = group;
        }

        public abstract void Redo();

        public abstract void Undo();
    }
}
