﻿namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtGroupCommand : ITtGroupCommand
    {
        private ITtManager pointsManager;

        public DeleteTtGroupCommand(TtGroup group, ITtManager pointsManager) : base(group)
        {
            this.pointsManager = pointsManager;
        }

        public override void Redo()
        {
            pointsManager.DeleteGroup(Group);
        }

        public override void Undo()
        {
            pointsManager.AddGroup(Group);
        }
    }
}
