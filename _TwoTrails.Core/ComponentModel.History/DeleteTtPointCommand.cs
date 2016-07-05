using System;
using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointCommand : ITtPointCommand
    {
        private TtManager pointsManager;

        private List<Tuple<QuondamPoint, TtPoint>> _ConvertedPoints = null;


        public DeleteTtPointCommand(TtPoint point, TtManager pointsManager, bool autoCommit = true) : base(point)
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

            pointsManager.DeletePoint(point);
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

            pointsManager.AddPoint(point);
        }
    }
}
