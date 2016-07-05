using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;

        private List<Tuple<QuondamPoint, TtPoint>> _ConvertedPoints = null;


        public DeleteTtPointsCommand(List<TtPoint> points, TtManager pointsManager, bool autoCommit = true) : base(points)
        {
            this.pointsManager = pointsManager;

            if (points.Any(p => p.HasQuondamLinks))
            {
                HashSet<string> deletedCNs = new HashSet<string>(points.Select(p => p.CN));
                _ConvertedPoints = new List<Tuple<QuondamPoint, TtPoint>>();

                foreach (TtPoint point in points.Where(p => p.HasQuondamLinks))
                {
                    QuondamPoint child;
                    foreach (string ccn in point.LinkedPoints)
                    {
                        if (!deletedCNs.Contains(ccn) && pointsManager.PointExists(ccn))
                        {
                            child = pointsManager.GetPoint(ccn) as QuondamPoint;

                            _ConvertedPoints.Add(Tuple.Create(child, child.ConvertQuondam()));
                        }
                    }
                } 
            }

            if (autoCommit)
                Redo();
        }

        public override void Redo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> pair in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(pair.Item1);
                }
            }

            pointsManager.DeletePoints(points);
        }

        public override void Undo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> pair in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(pair.Item2);
                }
            }

            pointsManager.AddPoints(points);
        }
    }
}
