using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class ReplaceTtPointsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;

        private List<TtPoint> _ReplacedPoints = null;


        public ReplaceTtPointsCommand(IEnumerable<TtPoint> points, TtManager pointsManager) : base(points)
        {
            this.pointsManager = pointsManager;
            
            _ReplacedPoints = Points.Select(pt => pointsManager.GetPoint(pt.CN)).ToList();
        }

        public override void Redo()
        {
            if (_ReplacedPoints != null && _ReplacedPoints.Count > 0)
            {
                pointsManager.ReplacePoints(Points);
            }
        }

        public override void Undo()
        {
            if (_ReplacedPoints != null && _ReplacedPoints.Count > 0)
            {
                pointsManager.ReplacePoints(_ReplacedPoints);
            }
        }
    }
}
