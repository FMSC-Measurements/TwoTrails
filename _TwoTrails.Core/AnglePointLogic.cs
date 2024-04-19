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
        public const double MINIMUM_ANGLE = 45d;
        public const double MAXIMUM_ANGLE = 180 - MINIMUM_ANGLE;
        public const double MIN_DIST_MULTIPLIER = 10d;

        public const AnglePointResult MAX_ERROR =
            AnglePointResult.AngleSpecsNotMet | AnglePointResult.SegmentsTooShort |
            AnglePointResult.NotEnoughBoundaryPoints | AnglePointResult.NotEnoughCorners;



        public static bool Qualifies(ITtManager manager, TtPolygon poly)
        {
            return Qualifies(manager.GetPoints(poly.CN));
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

            List<TtPoint> bpoints = points.OnBndPointsList();

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

                UTMCoords lastCoords = lastPoint.GetCoords(targetZone);
                UTMCoords currCoords = currPoint.GetCoords(targetZone);
                UTMCoords nextCoords;

                for (int i = 0; i < bpoints.Count - 1; i++)
                {
                    nextPoint = bpoints[i + 1];
                    nextCoords = nextPoint.GetCoords(targetZone);

                    double angle = MathEx.CalculateAngleBetweenPoints(
                        lastCoords.X, lastCoords.Y,
                        currCoords.X, currCoords.Y,
                        nextCoords.X, nextCoords.Y);

                    angle = angle % 180;

                    if (angle < MINIMUM_ANGLE || angle > MAXIMUM_ANGLE)
                    {
                        result |= AnglePointResult.AngleSpecsNotMet;
                    }

                    if (CheckSegmentMeetsMinimumDistance(lastCoords, currCoords, (currPoint.Accuracy + lastPoint.Accuracy) / 2d))
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
                if (CheckSegmentMeetsMinimumDistance(lastCoords, currCoords, (currPoint.Accuracy + lastPoint.Accuracy) / 2d))
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

        public static bool CheckSegmentMeetsMinimumDistance(UTMCoords cc, UTMCoords nc, double accuracy) =>
            MathEx.Distance(cc.X, cc.Y, nc.X, nc.Y) < (accuracy * MIN_DIST_MULTIPLIER);



        public static IEnumerable<String> GetErrorMessages(this AnglePointResult result)
        {
            if (result.HasFlag(AnglePointResult.NotEnoughBoundaryPoints))
                yield return "There are not enough boundary points to create a geometry.";

            if (result.HasFlag(AnglePointResult.NotEnoughCorners))
                yield return "There are not enough unique points to create a geometry.";

            if (result.HasFlag(AnglePointResult.AngleSpecsNotMet))
                yield return "Not all edges meet the required angle specifications.";

            if (result.HasFlag(AnglePointResult.SegmentsTooShort))
                yield return "Not all segments meet the required distancce specifications.";
        }
    }


    [Flags]
    public enum AnglePointResult
    {
        Qualifies               = 0,
        NotEnoughBoundaryPoints = 1 << 0,
        NotEnoughCorners        = 1 << 1,
        AngleSpecsNotMet        = 1 << 2,
        SegmentsTooShort        = 1 << 3
    }
}
