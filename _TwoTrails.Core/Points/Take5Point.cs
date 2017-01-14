using System;

namespace TwoTrails.Core.Points
{
    public class Take5Point : GpsPoint
    {
        public Take5Point() : base() { }

        public Take5Point(TtPoint point) : base(point) { }

        public Take5Point(GpsPoint point) : base(point) { }

        public Take5Point(string cn, int index, int pid, DateTime time, string polycn, string metacn, string groupcn,
            string comment, bool onbnd, double adjx, double adjy, double adjz, double unadjx, double unadjy, double unadjz,
            double acc, string qlinks, double? lat, double? lon, double? elev, double? manacc, double? rmser)
            : base(cn, index, pid, time, polycn, metacn, groupcn, comment, onbnd, adjx, adjy, adjz, unadjx,
            unadjy, unadjz, acc, qlinks, lat, lon, elev, manacc, rmser)
        { }

        public override OpType OpType { get { return OpType.Take5; } }
    }
}
