using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointCommand : ITtPointCommand
    {
        private TtManager pointsManager;

        private List<Tuple<QuondamPoint, GpsPoint>> _ConvertedPoints = null;
        private List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private TtPoint _QpParentPoint = null;

        public DeleteTtPointCommand(TtPoint point, TtManager pointsManager) : base(point)
        {
            this.pointsManager = pointsManager;

            if (point.HasQuondamLinks)
            {
                _ConvertedPoints = new List<Tuple<QuondamPoint, GpsPoint>>();

                QuondamPoint child;
                foreach (string ccn in point.LinkedPoints)
                {
                    if (pointsManager.PointExists(ccn))
                    {
                        child = pointsManager.GetPoint(ccn) as QuondamPoint;
                        
                        _ConvertedPoints.Add(Tuple.Create(child, child.ConvertQuondam()));

                        if (point.IsGpsType())
                        {
                            _AddNmea.AddRange(pointsManager.GetNmeaBursts(point.CN).Select(n => new TtNmeaBurst(n, child.CN)));
                        }
                    }
                }
            }

            if (point.OpType == OpType.Quondam && point is QuondamPoint qp)
            {
                _QpParentPoint = qp.ParentPoint;
            }
        }

        public override void Redo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<QuondamPoint, GpsPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item2);
                    Point.RemoveLinkedPoint(tuple.Item1);
                }

                pointsManager.AddNmeaBursts(_AddNmea);
            }

            if (_QpParentPoint != null)
            {
                _QpParentPoint.RemoveLinkedPoint(Point as QuondamPoint);
            }

            pointsManager.DeletePoint(Point);

            if (Point.IsGpsType())
            {
                pointsManager.DeleteNmeaBursts(Point.CN);
            }
        }

        public override void Undo()
        {
            if (Point.IsGpsType())
            {
                pointsManager.RestoreNmeaBurts(Point.CN);
            }

            pointsManager.AddPoint(Point);

            if (_QpParentPoint != null)
            {
                _QpParentPoint.AddLinkedPoint(Point as QuondamPoint);
            }

            if (_ConvertedPoints != null)
            {
                foreach (Tuple<QuondamPoint, GpsPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item1);
                    Point.AddLinkedPoint(tuple.Item1);
                    pointsManager.DeleteNmeaBursts(tuple.Item2.CN);
                }
            }
        }

        protected override string GetCommandInfoDescription() => $"Delete Point {Point.PID}";
    }
}
