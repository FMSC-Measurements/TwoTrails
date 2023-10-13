using System;
using System.Collections.Generic;
using System.Linq;

namespace TwoTrails.Core.ComponentModel.History
{
    public class MultiTtCommand : ITtBaseCommand
    {
        private List<ITtCommand> _Commands = new List<ITtCommand>();

        public int NumberOfCommands => _Commands.Count;

        public string Description { get; }

        public MultiTtCommand(TtManager manager, IEnumerable<ITtCommand> commands, string descripton = null) : base(manager)
        {
            _Commands = new List<ITtCommand>(commands);
            Description = descripton;
        }

        public override void Redo()
        {
            foreach (ITtCommand command in _Commands)
                command.Redo();
        }

        public override void Undo()
        {
            foreach (ITtCommand command in _Commands)
                command.Undo();
        }


        protected override int GetAffectedItemCount() => _Commands.Select(c => c.CommandInfo.AffectedItems).Count();
        protected override DataActionType GetActionType()
        {
            DataActionType action = DataActionType.None;
            foreach (ITtCommand command in _Commands)
                action |= command.CommandInfo.ActionType;
            return action;
        }
        protected override string GetCommandInfoDescription() => Description ?? $"Multi Command made of {_Commands} commands.";
    }
}
