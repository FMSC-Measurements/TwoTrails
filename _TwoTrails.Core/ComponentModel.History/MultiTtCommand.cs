using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MultiTtCommand : ITtCommand
    {
        public bool RequireRefresh { get { return _Commands.Any(c => c.RequireRefresh); } }

        private List<ITtCommand> _Commands = new List<ITtCommand>();


        public MultiTtCommand(IEnumerable<ITtCommand> commands, bool autoCommit = true)
        {
            _Commands = new List<ITtCommand>(commands);

            if (autoCommit)
                Redo();
        }

        public void Redo()
        {
            foreach (ITtCommand command in _Commands)
                command.Redo();
        }

        public void Undo()
        {
            foreach (ITtCommand command in _Commands)
                command.Undo();
        }
    }
}
