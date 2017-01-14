namespace TwoTrails.Core.ComponentModel.History
{
    public interface ITtCommand
    {
        bool RequireRefresh { get; }
        void Undo();
        void Redo();
    }
}
