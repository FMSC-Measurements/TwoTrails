using System;
using System.Collections.Generic;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ResetTtPointCommand : ITtPointCommand
    {
        private TtManager pointsManager;
        private TtPoint _ResetPoint;

        public ResetTtPointCommand(TtPoint point, TtManager pointsManager, bool autoCommit = true) : base(point)
        {
            this.pointsManager = pointsManager;

            _ResetPoint = pointsManager.GetOriginalPoint(point.CN).DeepCopy();

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.ReplacePoint(_ResetPoint);
        }

        public override void Undo()
        {
            pointsManager.ReplacePoint(Point);
        }
    }
}
