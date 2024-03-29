﻿using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core.ComponentModel.History
{
    public class DeleteTtPointsCommand : ITtPointsCommand
    {
        private TtManager pointsManager;

        private List<Tuple<TtPoint, QuondamPoint, TtPoint>> _ConvertedPoints = null;
        private List<TtNmeaBurst> _AddNmea = new List<TtNmeaBurst>();
        private List<Tuple<QuondamPoint, TtPoint>> _QpParentPoints = null;


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

                            if (point.IsGpsType())
                            {
                                _AddNmea.AddRange(pointsManager.GetNmeaBursts(point.CN).Select(n => new TtNmeaBurst(n, child.CN)));
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
                foreach (Tuple<TtPoint, QuondamPoint, TtPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item3);
                    tuple.Item1.RemoveLinkedPoint(tuple.Item2);
                }
                
                pointsManager.AddNmeaBursts(_AddNmea);
            }

            if (_QpParentPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> qvp in _QpParentPoints)
                {
                    qvp.Item2.RemoveLinkedPoint(qvp.Item1);
                }
            }

            pointsManager.DeletePoints(Points);

            foreach (TtPoint point in Points.Where(p => p.IsGpsType()))
            {
                pointsManager.DeleteNmeaBursts(point.CN);
            }
        }

        public override void Undo()
        {
            foreach (TtPoint point in Points.Where(p => p.IsGpsType()))
            {
                pointsManager.RestoreNmeaBurts(point.CN);
            }

            pointsManager.AddPoints(Points);

            if (_QpParentPoints != null)
            {
                foreach (Tuple<QuondamPoint, TtPoint> qvp in _QpParentPoints)
                {
                    qvp.Item2.AddLinkedPoint(qvp.Item1);
                }
            }

            if (_ConvertedPoints != null)
            {
                foreach (Tuple<TtPoint, QuondamPoint, TtPoint> tuple in _ConvertedPoints)
                {
                    pointsManager.ReplacePoint(tuple.Item2);
                    tuple.Item1.AddLinkedPoint(tuple.Item2);

                    pointsManager.DeleteNmeaBursts(tuple.Item3.CN);
                }
            }
        }
    }
}
