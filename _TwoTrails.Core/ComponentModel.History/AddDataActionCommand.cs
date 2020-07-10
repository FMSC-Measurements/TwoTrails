using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddDataActionCommand : ITtCommand
    {
        private TtManager _Manager;
        public Type DataType => null;
        public bool RequireRefresh => false;

        private readonly DataActionType _DataActionType;
        private readonly string _Notes;
        private readonly Guid _ID = Guid.NewGuid();


        public AddDataActionCommand(DataActionType dataAction, TtManager manager, string notes = null)
        {
            _Manager = manager;
            _DataActionType = dataAction;
            _Notes = notes;
        }

        public void Redo()
        {
            _Manager.AddAction(_DataActionType, _Notes, _ID);
        }

        public void Undo()
        {
            _Manager.RemoveAction(_ID);
        }
    }
}
