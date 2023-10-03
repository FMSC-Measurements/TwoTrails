using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public interface ITtCommand
    {
        CommandInfo CommandInfo { get; }
        bool RequireRefresh { get; }
        void Undo();
        void Redo();
    }
}
