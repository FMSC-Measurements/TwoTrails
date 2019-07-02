using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TwoTrails.Core.Interfaces
{
    public interface IFileImportModel
    {
        ICommand SetupImportCommand { get; }
    }
}
