using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointCommand : ITtPointCommand
    {
        private List<Tuple<QuondamPoint, GpsPoint>> _ConvertedPoints = null;
        private List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private TtPoint _QpParentPoint = null;

        public DeleteTtPointCommand(TtManager manager, TtPoint point) : base(manager, point)
        {
            if (point.HasQuondamLinks)
            {
                _ConvertedPoints = new List<Tuple<QuondamPoint, GpsPoint>>();

                QuondamPoint child;
                foreach (string ccn in point.LinkedPoints)
                {
                    if (manager.PointExists(ccn))
                    {
                        child = manager.GetPoint(ccn) as QuondamPoint;
                        
                        _ConvertedPoints.Add(Tuple.Create(child, child.ConvertQuondam()));

                        if (point.IsGpsType())
                        {
                            _AddNmea.AddRange(manager.GetNmeaBursts(point.CN).Select(n => new TtNmeaBurst(n, child.CN)));
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
                    Manager.ReplacePoint(tuple.Item2);
                    Point.RemoveLinkedPoint(tuple.Item1);
                }

                Manager.AddNmeaBursts(_AddNmea);
            }

            if (_QpParentPoint != null)
            {
                _QpParentPoint.RemoveLinkedPoint(Point as QuondamPoint);
            }

            Manager.DeletePoint(Point);

            if (Point.IsGpsType())
            {
                Manager.DeleteNmeaBursts(Point.CN);
            }
        }

        public override void Undo()
        {
            if (Point.IsGpsType())
            {
                Manager.RestoreNmeaBurts(Point.CN);
            }

            Manager.AddPoint(Point);

            if (_QpParentPoint != null)
            {
                _QpParentPoint.AddLinkedPoint(Point as QuondamPoint);
            }

            if (_ConvertedPoints != null)
            {
                foreach (Tuple<QuondamPoint, GpsPoint> tuple in _ConvertedPoints)
                {
                    Manager.ReplacePoint(tuple.Item1);
                    Point.AddLinkedPoint(tuple.Item1);
                    Manager.DeleteNmeaBursts(tuple.Item2.CN);
                }
            }
        }


        protected override int GetAffectedItemCount() => _ConvertedPoints.Count + 1;
        protected override DataActionType GetActionType() => DataActionType.DeletedPoints;
        protected override string GetCommandInfoDescription() => $"Delete Point {Point.PID}";
    }
}
