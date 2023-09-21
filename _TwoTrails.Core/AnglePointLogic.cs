using FMSC.Core;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public static class AnglePointLogic
    {
        public const double MINIMUM_ANGLE = 35d;
        public const double MAXIMUM_ANGLE = 180 - MINIMUM_ANGLE;

        public const AnglePointResult MAX_ERROR =
            AnglePointResult.AngleSpecsNotMet | AnglePointResult.SegmentsTooShort |
            AnglePointResult.NotEnoughBoundaryPoints | AnglePointResult.NotEnoughCorners;



        public static bool Qualifies(ITtManager manager, String polyCN)
        {
            return Qualifies(manager.GetPoints(polyCN));
        }

        public static bool Qualifies(IEnumerable<TtPoint> points)
        {
            return VerifyGeometry(points) == AnglePointResult.Qualifies;
        }


        public static AnglePointResult VerifyGeometry(ITtManager manager, String polyCN)
        {
            return VerifyGeometry(manager.GetPoints(polyCN));
        }

        public static AnglePointResult VerifyGeometry(IEnumerable<TtPoint> points)
        {
            AnglePointResult result = AnglePointResult.Qualifies;

            List<TtPoint> bpoints = points.Where(p => p.IsBndPoint()).ToList();

            if (bpoints.Count > 2)
            {
                int targetZone = bpoints[0].Metadata.Zone;

                TtPoint lastPoint = bpoints[bpoints.Count - 1];
                TtPoint currPoint = bpoints[0];
                TtPoint nextPoint;

                if (currPoint.HasSameAdjLocation(lastPoint))
                {
                    for (int i = bpoints.Count - 2; i > 1; i --)
                    {
                        lastPoint = bpoints[i];
                        if (currPoint.HasSameAdjLocation(lastPoint))
                        {
                            bpoints.RemoveAt(i + 1);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (bpoints.Count < 3) return AnglePointResult.NotEnoughCorners;
                }
                else
                {
                    bpoints.Add(currPoint);
                }

                Func<TtPoint, TtPoint, UTMCoords, UTMCoords, bool> meetsDistReq = (TtPoint cp, TtPoint np, UTMCoords cc, UTMCoords nc) =>
                {
                    double minDist = (cp.Accuracy + np.Accuracy) / 2 * 10;
                    return MathEx.Distance(cc.X, cc.Y, nc.X, nc.Y) < minDist;
                };

                UTMCoords lastCoords = lastPoint.GetCoords(targetZone);
                UTMCoords currCoords = currPoint.GetCoords(targetZone);
                UTMCoords nextCoords;

                for (int i = 0; i < bpoints.Count - 1; i++)
                {
                    nextPoint = bpoints[i + 1];
                    nextCoords = nextPoint.GetCoords(targetZone);

                    double angle = MathEx.CalculateAngleBetweenPoints(
                        lastPoint.AdjX, lastPoint.AdjY,
                        currPoint.AdjX, currPoint.AdjY,
                        nextPoint.AdjX, nextPoint.AdjY);

                    angle = angle % 180;

                    if (angle < MINIMUM_ANGLE || angle > MAXIMUM_ANGLE)
                    {
                        result |= AnglePointResult.AngleSpecsNotMet;
                    }

                    if (meetsDistReq(lastPoint, currPoint, lastCoords, currCoords))
                    {
                        result |= AnglePointResult.SegmentsTooShort;
                    }

                    if (result == MAX_ERROR) break;

                    lastPoint = currPoint;
                    lastCoords = currCoords;

                    currPoint = nextPoint;
                    currCoords = nextCoords;
                }

                //check last and first point
                if (meetsDistReq(lastPoint, currPoint, lastCoords, currCoords))
                {
                    result |= AnglePointResult.SegmentsTooShort;
                }
            }
            else
            {
                result = AnglePointResult.NotEnoughBoundaryPoints;
            }

            return result;
        }

        public static IEnumerable<String> GetErrorMessages(this AnglePointResult result)
        {
            if (result.HasFlag(AnglePointResult.NotEnoughBoundaryPoints))
                yield return "There are not enough boundary points to create a geometry.";

            if (result.HasFlag(AnglePointResult.NotEnoughCorners))
                yield return "There are not enough unique points to create a geometry.";

            if (result.HasFlag(AnglePointResult.AngleSpecsNotMet))
                yield return "Not all edges meet the required angle specifications.";

            if (result.HasFlag(AnglePointResult.SegmentsTooShort))
                yield return "Not all segments meet the required distancce specifications";
        }
    }

    [Flags]
    public enum AnglePointResult
    {
        Qualifies               = 0,
        NotEnoughBoundaryPoints = 1 << 0,
        NotEnoughCorners        = 1 << 1,
        AngleSpecsNotMet          = 1 << 2,
        SegmentsTooShort        = 1 << 3
    }
}
