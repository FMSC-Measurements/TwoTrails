﻿using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class SideShotSegment : IPointSegment
    {
        private List<TtPoint> _SegmentPoints = new List<TtPoint>();

        public int Count { get { return _SegmentPoints.Count; } }

        public bool IsValid { get { return _SegmentPoints.Count > 1 && _SegmentPoints[0].OpType != OpType.SideShot; } }


        public SideShotSegment() { }


        public void Add(TtPoint point)
        {
            _SegmentPoints.Add(point);
        }

        public void Adjust()
        {
            TtPoint basePoint = _SegmentPoints[0];
            for (int i = 1; i < _SegmentPoints.Count; i++)
            {
                SideShotPoint ss = _SegmentPoints[i] as SideShotPoint;
                ss.Adjust(basePoint);
            }
        }
    }
}
