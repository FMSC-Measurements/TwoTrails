﻿using System;
using System.Collections.Generic;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ReplaceTtPointCommand : ITtPointCommand
    {
        private TtManager pointsManager;
        private TtPoint _ReplacedPoint;

        public ReplaceTtPointCommand(TtPoint point, TtManager pointsManager, bool autoCommit = true) : base(point)
        {
            this.pointsManager = pointsManager;
            
            _ReplacedPoint = pointsManager.GetPoint(point.CN);

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            pointsManager.ReplacePoint(Point);
        }

        public override void Undo()
        {
            pointsManager.ReplacePoint(_ReplacedPoint);
        }
    }
}