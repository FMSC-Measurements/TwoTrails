using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointCommand : ITtPointCommand
    {
        private TtManager pointsManager;

        private List<Tuple<QuondamPoint, TtPoint>> _ConvertedPoints = null;

        public DeleteTtPointCommand(TtPoint point, TtManager pointsManager) : base(point)
        {
            this.pointsManager = pointsManager;

            if (point.HasQuondamLinks)
            {
                _ConvertedPoints = new List<Tuple<QuondamPoint, TtPoint>>();

                QuondamPoint child;
                foreach (string ccn in point.LinkedPoints)
                {
                    if (pointsManager.PointExists(ccn))
                    {
                        child = pointsManager.GetPoint(ccn) as QuondamPoint;
                        
                        _ConvertedPoints.Add(Tuple.Create(child, child.ConvertQuondam()));
                    }
                }
            }
        }

        public override void Redo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item2);
                    Point.RemoveLinkedPoint(tuple.Item1);
                }
            }

            pointsManager.DeletePoint(Point);
        }

        public override void Undo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item1);
                    Point.AddLinkedPoint(tuple.Item1);
                }
            }

            pointsManager.AddPoint(Point);
        }
    }
}
