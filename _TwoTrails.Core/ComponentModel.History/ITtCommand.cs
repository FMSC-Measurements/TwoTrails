namespace TwoTrails.Core.ComponentModel.History
{
    public interface ITtCommand
    {
        void Undo();
        void Redo();
    }
}
