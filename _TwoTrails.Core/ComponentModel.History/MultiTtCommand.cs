using System.Collections.Generic;
using System.Linq;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MultiTtCommand : ITtCommand
    {
        public bool RequireRefresh { get { return _Commands.Any(c => c.RequireRefresh); } }

        private List<ITtCommand> _Commands = new List<ITtCommand>();

        public int NumberOfCommands => _Commands.Count;

        public MultiTtCommand(IEnumerable<ITtCommand> commands)
        {
            _Commands = new List<ITtCommand>(commands);
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
