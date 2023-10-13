using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointsCommand : ITtPointsCommand
    {
        private readonly List<Tuple<TtPoint, QuondamPoint, GpsPoint>> _ConvertedPoints = null;
        private readonly List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private readonly List<Tuple<QuondamPoint, TtPoint>> _QpParentPoints = null;


        public DeleteTtPointsCommand(TtManager manager, IEnumerable<TtPoint> points) : base(manager, points)
        {
            if (points.Any(p => p.HasQuondamLinks))
            {
                HashSet<string> deletedCNs = new HashSet<string>(points.Select(p => p.CN));
                _ConvertedPoints = new List<Tuple<TtPoint, QuondamPoint, GpsPoint>>();

                foreach (TtPoint point in points.Where(p => p.HasQuondamLinks))
                {
                    QuondamPoint child;
                    foreach (string ccn in point.LinkedPoints)
                    {
                        if (!deletedCNs.Contains(ccn) && manager.PointExists(ccn))
                        {
                            child = manager.GetPoint(ccn) as QuondamPoint;

                            _ConvertedPoints.Add(Tuple.Create(point, child, child.ConvertQuondam()));

                            if (point.IsGpsType())
                            {
                                _AddNmea.AddRange(manager.GetNmeaBursts(point.CN).Select(n => new TtNmeaBurst(n, child.CN)));
                            }
                        }
                    }
                } 
            }

            if (points.Any(p => p.OpType == OpType.Quondam))
            {
                _QpParentPoints = new List<Tuple<QuondamPoint, TtPoint>>();

                foreach (QuondamPoint qp in points.Where(p => p.OpType == OpType.Quondam))
                {
                    _QpParentPoints.Add(Tuple.Create(qp, qp.ParentPoint));
                }
            }
        }

        public override void Redo()
        {
            if (_ConvertedPoints != null)
            {
                foreach (Tuple<TtPoint, QuondamPoint, GpsPoint> tuple in _ConvertedPoints)
                {
                    Manager.ReplacePoint(tuple.Item3);
                    tuple.Item1.RemoveLinkedPoint(tuple.Item2);
                }
                
                Manager.AddNmeaBursts(_AddNmea);
            }

            if (_QpParentPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> qvp in _QpParentPoints)
                {
                    qvp.Item2.RemoveLinkedPoint(qvp.Item1);
                }
            }

            Manager.DeletePoints(Points);

            foreach (TtPoint point in Points.Where(p => p.IsGpsType()))
            {
                Manager.DeleteNmeaBursts(point.CN);
            }
        }

        public override void Undo()
        {
            foreach (TtPoint point in Points.Where(p => p.IsGpsType()))
            {
                Manager.RestoreNmeaBurts(point.CN);
            }

            Manager.AddPoints(Points);

            if (_QpParentPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> qvp in _QpParentPoints)
                {
                    qvp.Item2.AddLinkedPoint(qvp.Item1);
                }
            }

            if (_ConvertedPoints != null)
            {
                foreach (Tuple<TtPoint, QuondamPoint, GpsPoint> tuple in _ConvertedPoints)
                {
                    Manager.ReplacePoint(tuple.Item2);
                    tuple.Item1.AddLinkedPoint(tuple.Item2);

                    Manager.DeleteNmeaBursts(tuple.Item3.CN);
                }
            }
        }


        protected override int GetAffectedItemCount() => _ConvertedPoints != null ? _ConvertedPoints.Count : 0 + Points.Count;
        protected override DataActionType GetActionType() => DataActionType.DeletedPoints | (_AddNmea.Count > 0 ? DataActionType.InsertedNmea : DataActionType.None);
        protected override string GetCommandInfoDescription() => $"Delete {Points.Count} points";
    }
}
