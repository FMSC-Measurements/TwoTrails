using System.Collections.Generic;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public static class UnitAnalyzer
    {
        public static bool QualifiesForGER(ITtManager manager, TtPolygon polygon)
        {
            return QualifiesForGER(manager.GetPoints(polygon.CN));
        }

        public static bool QualifiesForGER(IEnumerable<TtPoint> points)
        {
            return !(IsAnIsland(points) || IsACorridor(points));
        }


        public static bool IsAnIsland(this ITtManager manager, string polyCN)
        {
            return IsAnIsland(manager.GetPoints(polyCN));
        }

        public static bool IsAnIsland(IEnumerable<TtPoint> points)
        {
            bool hasGps = false;
            int onBndSideShotCount = 0;

            foreach (TtPoint pt in points)
            {
                if (pt.OpType == OpType.SideShot)
                {
                    if (pt.OnBoundary)
                        onBndSideShotCount++;
                }
                else
                {
                    if (pt.OnBoundary)
                    {
                        return false; //Only SideShots can be On Boundary
                    }
                    else if (pt.IsGpsBndTypeAtBase() && !hasGps)
                    {
                        hasGps = true;
                    }
                    else
                    {
                        return false; //Only one off Boundary GPS can be in the unit besides SideShots
                    }
                }
            }

            if (hasGps && onBndSideShotCount > 2)
                return true;

            return false;
        }


        public static bool IsACorridor(this ITtManager manager, string polyCN)
        {
            return IsACorridor(manager.GetPoints(polyCN));
        }

        public static bool IsACorridor(IEnumerable<TtPoint> points)
        {
            int onBndGpsCount = 0, offBndSSCount = 0, qndmToOnBndSSCount = 0, bndCount = 0;

            foreach (TtPoint pt in points)
            {
                if (pt.OpType == OpType.Traverse)
                {
                    //No Traverse in a Corridor
                    return false;
                }

                if (pt.OnBoundary)
                {
                    bndCount++;

                    if (pt.IsGpsBndType())
                    {
                        onBndGpsCount++;
                    }
                    else if (pt is QuondamPoint qp && qp.ParentPoint.OpType == OpType.SideShot)
                    {
                        qndmToOnBndSSCount++;
                    }
                    else if (pt.OpType == OpType.SideShot)
                    {
                        //On Bounary Sideshot
                        return false;
                    }
                }
                else if (pt.OpType == OpType.SideShot)
                {
                    offBndSSCount++;
                }
            }

            if (bndCount > 2 && onBndGpsCount > 1 && qndmToOnBndSSCount > 0)
                return true;

            return false;
        }
    }
}
