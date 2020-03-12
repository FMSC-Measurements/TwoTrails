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

        private DataActionType _DataActionType;
        private string _Notes;


        public AddDataActionCommand(DataActionType dataAction, TtManager manager, string notes = null)
        {
            _Manager = manager;
            _DataActionType = dataAction;
            _Notes = notes;
        }

        public void Redo()
        {
            _Manager.AddAction(_DataActionType, _Notes);
        }

        public void Undo()
        {
            _Manager.RemoveLastAction();
        }
    }
}
