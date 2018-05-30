using System;

namespace TwoTrails.Core.Points
{
    public class WalkPoint : GpsPoint
    {
        public WalkPoint() : base() { }

        public WalkPoint(TtPoint point) : base(point) { }

        public WalkPoint(WalkPoint point) : base(point) { }

        public WalkPoint(string cn, int index, int pid, DateTime time, string polycn, string metacn, string groupcn,
            string comment, bool onbnd, double adjx, double adjy, double adjz, double unadjx, double unadjy, double unadjz,
            double acc, string qlinks, double? lat, double? lon, double? elev, double? manacc, double? rmser, DataDictionary extended = null)
            : base(cn, index, pid, time, polycn, metacn, groupcn, comment, onbnd, adjx, adjy, adjz, unadjx,
            unadjy, unadjz, acc, qlinks, lat, lon, elev, manacc, rmser, extended)
        { }

        public override OpType OpType { get { return OpType.Walk; } }
    }
}
