using FMSC.Core;
using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core
{
    public static class UnitAnalyzer
    {
        public static UnitAreaType GetUnitAreaType(ITtManager manager, TtPolygon polygon) => GetUnitAreaType(manager.GetPoints(polygon.CN));

        public static UnitAreaType GetUnitAreaType(IEnumerable<TtPoint> points)
        {
            if (IsAnIsland(points)) return UnitAreaType.Island;
            if (IsACorridor(points)) return UnitAreaType.Corridor;

            return UnitAreaType.General;
        }


        public static bool QualifiesForGER(ITtManager manager, TtPolygon polygon) => QualifiesForGER(manager.GetPoints(polygon.CN));

        public static bool QualifiesForGER(IEnumerable<TtPoint> points)
        {
            return !(IsAnIsland(points) || IsACorridor(points));
        }


        public static bool IsAnIsland(this ITtManager manager, string polyCN) => IsAnIsland(manager.GetPoints(polyCN));

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


        public static bool IsACorridor(this ITtManager manager, string polyCN) => IsACorridor(manager.GetPoints(polyCN));

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


        public static bool HasTies(this ITtManager manager, string polyCN, bool advancedDetection = false) => HasTies(manager.GetPoints(polyCN), advancedDetection);

        public static bool HasTies(IEnumerable<TtPoint> points, bool advancedDetection = false)
        {
            return HasTies(points.OnBndPoints().SyncPointsToZone(), advancedDetection);
        }

        public static bool HasTies(IEnumerable<Point> points, bool advancedDetection = false)
        {
            List<Point> pts = points.RemoveSequencialDuplicates().ToList();

            if (pts.Count > 3)
            {
                if (advancedDetection)
                {
                    Point p1, p2;

                    for (int i = 0; i < pts.Count - 2; i++)
                    {
                        p1 = pts[i];
                        p2 = pts[i + 1];

                        for (int j = 0; j + 1 < i; j++)
                        {
                            if (MathEx.LineSegmentsIntersect(p1, p2, pts[j], pts[j + 1])) return true;
                        }

                        for (int j = i + 2; j + 1 < pts.Count; j++)
                        {
                            if (MathEx.LineSegmentsIntersect(p1, p2, pts[j], pts[j + 1])) return true;
                        }
                    }

                    p1 = pts[0];
                    p2 = pts[pts.Count - 1];

                    for (int i = 1; i < pts.Count - 2; i++)
                    {
                        if (MathEx.LineSegmentsIntersect(p1, p2, pts[i], pts[i + 1])) return true;
                    }
                }
                else
                {
                    for (int i = 0; i < pts.Count - 3; i++)
                    {
                        if (MathEx.LineSegmentsIntersect(pts[i], pts[i + 1], pts[i + 2], pts[i + 3])) return true;
                    }

                    if (MathEx.LineSegmentsIntersect(pts[pts.Count - 3], pts[pts.Count - 2], pts[pts.Count - 1], pts[0])) return true;
                    if (MathEx.LineSegmentsIntersect(pts[pts.Count - 2], pts[pts.Count - 1], pts[0], pts[1])) return true;
                    if (MathEx.LineSegmentsIntersect(pts[pts.Count - 1], pts[0], pts[1], pts[2])) return true;
                }
            }

            return false;
        }
    }
}
