using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class CreateTtPointCommand : ITtPointCommand
    {
        private AddTtPointCommand _AddTtPointCommand;
        private AddDataActionCommand _AddDataActionCommand;

        public CreateTtPointCommand(TtPoint point, TtManager pointsManager) : base(point)
        {
            _AddTtPointCommand = new AddTtPointCommand(point, pointsManager);
            _AddDataActionCommand = new AddDataActionCommand(DataActionType.ManualPointCreation, pointsManager);
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

        protected override string GetCommandInfoDescription() => $"Create Point {Point.PID}";
    }
}
