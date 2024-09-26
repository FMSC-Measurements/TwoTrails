using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddDataActionCommand : ITtBaseCommand
    {
        private readonly DataActionType _DataActionType;
        private readonly string _Notes;
        private readonly Guid _ID = Guid.NewGuid();

        public AddDataActionCommand(TtManager manager, DataActionType dataAction, string notes = null) : base(manager)
        {
            _DataActionType = dataAction;
            _Notes = notes;
        }

        public override void Redo()
        {
            Manager.AddAction(_DataActionType, _Notes, _ID);
        }

        public override void Undo()
        {
            Manager.RemoveAction(_ID);
        }

        protected override DataActionType GetActionType() => DataActionType.None;

        protected override string GetCommandInfoDescription() => $"Add data action";
    }
}
