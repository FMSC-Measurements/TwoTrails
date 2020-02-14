using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public interface ITtCommand
    {
        Type DataType { get; }
        bool RequireRefresh { get; }
        void Undo();
        void Redo();
    }
}
