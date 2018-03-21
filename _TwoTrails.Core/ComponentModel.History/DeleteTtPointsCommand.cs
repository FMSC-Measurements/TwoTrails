using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;

        private List<Tuple<TtPoint, QuondamPoint, TtPoint>> _ConvertedPoints = null;


        public DeleteTtPointsCommand(IEnumerable<TtPoint> points, TtManager pointsManager) : base(points)
        {
            this.pointsManager = pointsManager;

            if (points.Any(p => p.HasQuondamLinks))
            {
                HashSet<string> deletedCNs = new HashSet<string>(points.Select(p => p.CN));
                _ConvertedPoints = new List<Tuple<TtPoint, QuondamPoint, TtPoint>>();

                foreach (TtPoint point in points.Where(p => p.HasQuondamLinks))
                {
                    QuondamPoint child;
                    foreach (string ccn in point.LinkedPoints)
                    {
                        if (!deletedCNs.Contains(ccn) && pointsManager.PointExists(ccn))
                        {
                            child = pointsManager.GetPoint(ccn) as QuondamPoint;

                            _ConvertedPoints.Add(Tuple.Create(point, child, child.ConvertQuondam()));
                        }
                    }
                } 
            }
        }

        public override void Redo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<TtPoint, QuondamPoint, TtPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item3);
                    tuple.Item1.RemoveLinkedPoint(tuple.Item2);
                }
            }

            pointsManager.DeletePoints(Points);
        }

        public override void Undo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<TtPoint, QuondamPoint, TtPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item2);
                    tuple.Item1.AddLinkedPoint(tuple.Item2);
                }
            }

            pointsManager.AddPoints(Points);
        }
    }
}
