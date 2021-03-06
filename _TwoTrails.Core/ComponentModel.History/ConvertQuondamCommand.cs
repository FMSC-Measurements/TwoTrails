﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ConvertQuondamCommand : ITtPointCommand
    {
        private TtManager pointsManager;
        private List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private TtPoint _ConvertedPoint;

        public ConvertQuondamCommand(QuondamPoint point, TtManager pointsManager) : base(point)
        {
            this.pointsManager = pointsManager;

            _ConvertedPoint = point.ConvertQuondam();

            if (point.IsGpsAtBase())
            {
                _AddNmea.AddRange(pointsManager.GetNmeaBursts(point.ParentPointCN).Select(n => new TtNmeaBurst(n, _ConvertedPoint.CN)));
            }
        }

        public override void Redo()
        {
            pointsManager.ReplacePoint(_ConvertedPoint);

            if (_AddNmea.Count > 0)
            {
                pointsManager.AddNmeaBursts(_AddNmea);
            }
        }

        public override void Undo()
        {
            pointsManager.ReplacePoint(Point);

            if (_AddNmea.Count > 0)
            {
                pointsManager.RestoreNmeaBurts(Point.CN);
            }
        }
    }
}
