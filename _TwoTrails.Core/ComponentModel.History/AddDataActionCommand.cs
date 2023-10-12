using System;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddDataActionCommand : ITtBaseCommand
    {
        private TtManager _Manager;

        private readonly DataActionType _DataActionType;
        private readonly string _Notes;
        private readonly Guid _ID = Guid.NewGuid();


        public AddDataActionCommand(DataActionType dataAction, TtManager manager, string notes = null)
        {
            _Manager = manager;
            _DataActionType = dataAction;
            _Notes = notes;
        }

        public override void Redo()
        {
            _Manager.AddAction(_DataActionType, _Notes, _ID);
        }

        public override void Undo()
        {
            _Manager.RemoveAction(_ID);
        }

        protected override DataActionType GetActionType() => DataActionType.None;

        protected override string GetCommandInfoDescription() => $"Add data action";
    }
}
