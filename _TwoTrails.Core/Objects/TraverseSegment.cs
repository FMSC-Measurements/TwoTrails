using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class TraverseSegment : IPointSegment
    {
        private List<TtPoint> _PolyPoints;
        private List<TtPoint> _SegmentPoints = new List<TtPoint>();

        public int Count { get { return _SegmentPoints.Count; } }

        public bool IsValid
        {
            get
            {
                return _SegmentPoints.Count > 2 &&
                    _SegmentPoints[0].IsGpsAtBase() &&
                    _SegmentPoints[_SegmentPoints.Count - 1].IsGpsAtBase();
            }
        }


        public TraverseSegment(IList<TtPoint> polyPoints)
        {
            _PolyPoints = new List<TtPoint>(polyPoints);
        }


        public void Add(TtPoint point)
        {
            _SegmentPoints.Add(point);
        }

        public void Adjust()
        {
            if (_SegmentPoints.Count > 2)
            {
                double lastX, lastY, lastZ, currX, currY, currZ, deltaX, deltaY, deltaZ,
                    deltaDist, legLen = 0, adjPerim, deltaXCorr, deltaYCorr, deltaZCorr,
                    adjX, adjY, adjZ;

                int size = _SegmentPoints.Count;

                CalculateTraverseUnAdjSegment();

                adjPerim = CalculateSegmentAdjustmentPerimeter();

                TtPoint last = _SegmentPoints[size - 1];

                lastX = last.UnAdjX;
                lastY = last.UnAdjY;
                lastZ = last.UnAdjZ;

                TtPoint curr = _SegmentPoints[size - 2];

                deltaX = lastX - curr.UnAdjX;
                deltaY = lastY - curr.UnAdjY;
                deltaZ = lastZ - curr.UnAdjZ;

                deltaDist = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

                deltaXCorr = deltaX / adjPerim;
                deltaYCorr = deltaY / adjPerim;
                deltaZCorr = deltaZ / adjPerim;

                curr = _SegmentPoints[0];

                double acc = curr.Accuracy;
                double accInc = (last.Accuracy - acc) / (_SegmentPoints.Count - 1);

                lastX = curr.UnAdjX;
                lastY = curr.UnAdjY;
                lastZ = curr.UnAdjZ;

                for (int i = 1; i < size - 1; i++)
                {
                    curr = _SegmentPoints[i];
                    acc += accInc;

                    currX = curr.UnAdjX;
                    currY = curr.UnAdjY;
                    currZ = curr.UnAdjZ;

                    legLen += MathEx.Distance(currX, currY, lastX, lastY);

                    adjX = legLen * deltaXCorr + currX;
                    adjY = legLen * deltaYCorr + currY;
                    adjZ = legLen * deltaZCorr + currZ;

                    curr.SetAdjLocation(adjX, adjY, adjZ);
                    curr.SetAccuracy(acc);

                    lastX = currX;
                    lastY = currY;
                    lastZ = currZ;
                }

                //Adjust the sideshots in the traverse
                last = _SegmentPoints[0];
                for (int i = _SegmentPoints[1].Index; i < _PolyPoints.Count; i++)
                {
                    curr = _PolyPoints[i];

                    if (curr.OpType == OpType.SideShot)
                    {
                        ((SideShotPoint)curr).Adjust(last);
                    }
                    else if (curr.OpType == OpType.Traverse)
                    {
                        last = curr;
                    }
                    else
                        break;
                }
            }
        }


        private void CalculateTraverseUnAdjSegment()
        {
            TtPoint last = _SegmentPoints.First();

            foreach (TtPoint point in _SegmentPoints)
            {
                if (point.OpType == OpType.Traverse)
                {
                    ((TravPoint)point).Calculate(last.UnAdjX, last.UnAdjY, last.UnAdjZ, false);
                }

                last = point;
            }
        }

        private double CalculateSegmentAdjustmentPerimeter()
        {
            TtPoint curr = _SegmentPoints[0];

            double adjPerim = 0, currX, currY,
                lastX = curr.UnAdjX,
                lastY = curr.UnAdjY;

            for (int i = 1; i < _SegmentPoints.Count; i++)
            {
                curr = _SegmentPoints[i];

                if (curr.OpType == OpType.Traverse)
                {
                    currX = curr.UnAdjX;
                    currY = curr.UnAdjY;

                    adjPerim += MathEx.Distance(currX, currY, lastX, lastY);

                    lastX = currX;
                    lastY = currY;
                }
            }

            return adjPerim;
        }
    }
}
