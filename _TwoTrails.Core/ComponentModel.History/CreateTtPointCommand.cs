using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateTtPointCommand : ITtPointCommand
    {
        private readonly AddTtPointCommand _AddTtPointCommand;
        private readonly AddDataActionCommand _AddDataActionCommand;

        public CreateTtPointCommand(TtManager manager, TtPoint point) : base(manager, point)
        {
            _AddTtPointCommand = new AddTtPointCommand(manager, point);
            _AddDataActionCommand = new AddDataActionCommand(manager, DataActionType.ManualPointCreation);
        }

        public override void Redo()
        {
            _AddTtPointCommand.Redo();
            _AddDataActionCommand.Redo();
        }

        public override void Undo()
        {
            _AddDataActionCommand.Undo();
            _AddTtPointCommand.Undo();
        }

        protected override DataActionType GetActionType() => _AddTtPointCommand.CommandInfo.ActionType | DataActionType.ManualPointCreation;
        protected override string GetCommandInfoDescription() => $"Create Point {Point.PID}";
    }
}
