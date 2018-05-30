using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ResetTtPointsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;

        private List<TtPoint> _ResetPoints = null;


        public ResetTtPointsCommand(IEnumerable<TtPoint> points, TtManager pointsManager) : base(points.Where(p => pointsManager.HasOriginalPoint(p.CN)))
        {
            this.pointsManager = pointsManager;
            
            _ResetPoints = Points.Select(pt => pointsManager.GetOriginalPoint(pt.CN).DeepCopy()).ToList();
        }

        public override void Redo()
        {
            if (_ResetPoints != null && _ResetPoints.Count > 0)
            {
                pointsManager.ReplacePoints(_ResetPoints);
            }
        }

        public override void Undo()
        {
            if (_ResetPoints != null && _ResetPoints.Count > 0)
            {
                pointsManager.ReplacePoints(Points);
            }
        }
    }
}
