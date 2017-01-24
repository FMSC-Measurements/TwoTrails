using System;

namespace TwoTrails.Core.Points
{
    public class SideShotPoint : TravPoint
    {
        public SideShotPoint() : base() { }

        public SideShotPoint(TtPoint point) : base(point) { }

        public SideShotPoint(SideShotPoint point) : base(point) { }

        public SideShotPoint(string cn, int index, int pid, DateTime time, string polycn, string metacn, string groupcn,
            string comment, bool onbnd, double adjx, double adjy, double adjz, double unadjx, double unadjy, double unadjz,
            double acc, string qlinks, double? fwd, double? bk, double sd, double sa)
            : base(cn, index, pid, time, polycn, metacn, groupcn, comment, onbnd, adjx, adjy, adjz, unadjx,
            unadjy, unadjz, acc, qlinks, fwd, bk, sd, sa)
        { }


        public override OpType OpType { get { return OpType.SideShot; } }

        public void Adjust(TtPoint point)
        {
            Calculate(point.UnAdjX, point.UnAdjY, point.UnAdjZ, false);
            Calculate(point.AdjX, point.AdjY, point.AdjZ, true);
            SetAccuracy(point.Accuracy);
        }
    }
}
