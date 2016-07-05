﻿using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class AddTtPointsCommand : ITtPointsCommand
    {
        private ITtManager pointsManager;

        public AddTtPointsCommand(List<TtPoint> points, ITtManager pointsManager, bool autoCommit = true) : base(points)
        {
            this.pointsManager = pointsManager;

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.AddPoints(points);
        }

        public override void Undo()
        {
            pointsManager.DeletePoints(points);
        }
    }
}
