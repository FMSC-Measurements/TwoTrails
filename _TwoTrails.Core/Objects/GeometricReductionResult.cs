using FMSC.Core;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class GeometricErrorReductionResult
    {
        public const double MINIMUM_ANGLE = 45d;
        public const double MAXIMUM_ANGLE = 90d;
        public const double MIN_DIST_MULTIPLIER = 5d;
        public const double MAX_DIST_MULTIPLIER = 10d;

        public AnglePointResult Flags { get; private set; }

        public TtPolygon Polygon { get; private set; }

        public double TotalError { get; private set; }
        public double AreaError => Polygon.Area > 0 ? TotalError / Polygon.Area * 100d : 0;

        public int ShortLegs { get; private set; }
        public int ShallowEdges { get; private set; }

        public double LegsPercent => Segments.Count > 0 ? (Segments.Count - ShortLegs) * 100d / Segments.Count : 0;
        public double EdgesPercent => Segments.Count > 0 ? (Segments.Count - ShallowEdges) * 100d / Segments.Count : 0;


        public List<GERSegment> Segments { get; private set; } = new List<GERSegment>();


        public GeometricErrorReductionResult(ITtManager manager, TtPolygon polygon)
        {
            Polygon = polygon;

            List<TtPoint> bpoints = manager.GetPoints(polygon.CN).Where(p => p.IsBndPoint()).ToList();

            if (bpoints.Count > 2)
            {
                int targetZone = bpoints[0].Metadata.Zone;

                TtPoint lastPoint = bpoints[bpoints.Count - 1];
                TtPoint currPoint = bpoints[0];
                TtPoint nextPoint;

                if (currPoint.HasSameAdjLocation(lastPoint))
                {
                    for (int i = bpoints.Count - 2; i > 1; i--)
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

                    if (bpoints.Count < 3)
                    {
                        Flags |= AnglePointResult.NotEnoughCorners;
                        return;
                    }
                }
                else
                {
                    bpoints.Add(currPoint);
                }

                UTMCoords lastCoords = lastPoint.GetCoords(targetZone);
                UTMCoords currCoords = currPoint.GetCoords(targetZone);
                UTMCoords nextCoords;

                double distSegA = MathEx.Distance(lastCoords.X, lastCoords.Y, currCoords.X, currCoords.Y);
                double distSegB;

                double accSegA = (lastPoint.Accuracy + currPoint.Accuracy) / 2d;
                double accSegB;

                double minDistSegA = accSegA * MIN_DIST_MULTIPLIER;
                double minDistSegB;
                double maxDistSegA = accSegA * MAX_DIST_MULTIPLIER;
                double maxDistSegB;

                double accDistSegA = accSegA * distSegA;
                double accDistSegB;

                double aeDistDivA = distSegA < minDistSegA ? 1 :
                        (distSegA >= maxDistSegA) ? 2 :
                            distSegA / minDistSegA;
                double aeDistDivB;

                for (int i = 0; i < bpoints.Count - 1; i++)
                {
                    nextPoint = bpoints[i + 1];
                    nextCoords = nextPoint.GetCoords(targetZone);

                    double angle = MathEx.CalculateAngleBetweenPoints(
                        lastCoords.X, lastCoords.Y,
                        currCoords.X, currCoords.Y,
                        nextCoords.X, nextCoords.Y);

                    angle = angle % 180;

                    distSegB = MathEx.Distance(currCoords.X, currCoords.Y, nextCoords.X, nextCoords.Y);

                    accSegB = (currPoint.Accuracy + nextPoint.Accuracy) / 2d;

                    minDistSegB = accSegB * MIN_DIST_MULTIPLIER;
                    maxDistSegB = accSegB * MAX_DIST_MULTIPLIER;

                    accDistSegB = accSegB * distSegB;

                    double segmentLength = (distSegA + distSegB) / 2;

                    bool shortLeg = distSegB < minDistSegB;
                    bool shallowEdge = angle < MINIMUM_ANGLE;

                    if (shortLeg)
                        ShortLegs++;

                    if (shallowEdge)
                        ShallowEdges++;

                    aeDistDivB = shortLeg ? 1 :
                            (distSegB >= maxDistSegB) ? 2 :
                                distSegB / minDistSegB;

                    double aeAngDiv = shallowEdge ? 1 :
                            (angle >= MAXIMUM_ANGLE) ? 2 :
                                angle / MINIMUM_ANGLE;

                    double aeA = accDistSegA / 2 / ((aeDistDivA + aeAngDiv) / 2);
                    double aeB = accDistSegB / 2 / ((aeDistDivB + aeAngDiv) / 2);

                    double segAreaError = aeA + aeB;

                    Segments.Add(new GERSegment(lastPoint, currPoint, nextPoint, segAreaError, segmentLength, shortLeg, shallowEdge));

                    TotalError += segAreaError;


                    lastPoint = currPoint;
                    lastCoords = currCoords;

                    currPoint = nextPoint;
                    currCoords = nextCoords;

                    distSegA = distSegB;
                    accSegA = accSegB;
                    minDistSegA = minDistSegB;
                    maxDistSegA = maxDistSegB;
                    accDistSegA = accDistSegB;
                    aeDistDivA = aeDistDivB;
                }
            }
            else
            {
                Flags |= AnglePointResult.NotEnoughBoundaryPoints;
            }
        }

        public class GERSegment
        {
            //public const double MINIMUM_ANGLE = 45d;
            //public const double MAXIMUM_ANGLE = 90d;
            //public const double MIN_DIST_MULTIPLIER = 5d;
            //public const double MAX_DIST_MULTIPLIER = 10d;

            public TtPoint Point1 { get; }
            public TtPoint Point2 { get; }
            public TtPoint Point3 { get; }

            public bool ShortLegs { get; }
            public bool ShallowEdge { get; }

            public double SegmentLength { get; }
            public double AreaError { get; }


            public GERSegment(TtPoint p1, TtPoint p2, TtPoint p3, double areaError, double segLength, bool shortLegs, bool shallowEdge)
            {
                Point1 = p1;
                Point2 = p2;
                Point3 = p3;

                AreaError = areaError;
                SegmentLength = segLength;
                ShortLegs = shortLegs;
                ShallowEdge = shallowEdge;

                //double distSegA = MathEx.Distance(c1.X, c1.Y, c2.X, c2.Y);
                //double distSegB = MathEx.Distance(c2.X, c2.Y, c3.X, c3.Y);

                //double accSegA = (p1.Accuracy + p2.Accuracy) / 2d;
                //double accSegB = (p2.Accuracy + p3.Accuracy) / 2d;

                //double minDistSegA = accSegA * MIN_DIST_MULTIPLIER;
                //double minDistSegB = accSegB * MIN_DIST_MULTIPLIER;
                //double maxDistSegA = accSegA * MAX_DIST_MULTIPLIER;
                //double maxDistSegB = accSegB * MAX_DIST_MULTIPLIER;

                //double accDistSegA = accSegA * distSegA;
                //double accDistSegB = accSegB * distSegB;

                //SegmentLength = (distSegA + distSegB) / 2;
                //ShortLegs = distSegA < minDistSegA || distSegB < minDistSegB;

                //double angle = MathEx.CalculateAngleBetweenPoints(
                //        c1.X, c1.Y,
                //        c2.X, c2.Y,
                //        c3.X, c3.Y);

                //ShallowEdge = angle < MINIMUM_ANGLE;

                //double aeDistDivA = distSegA < minDistSegA ? 1 : 
                //        (distSegA >= maxDistSegA) ? 2 :
                //            distSegA / minDistSegA;

                //double aeDistDivB = distSegB < minDistSegB ? 1 :
                //        (distSegB >= maxDistSegB) ? 2 :
                //            distSegB / minDistSegB;

                //double aeAngDiv = angle < MINIMUM_ANGLE ? 1 :
                //        (angle >= MAXIMUM_ANGLE) ? 2 :
                //            angle / MINIMUM_ANGLE;

                //double aeA = accDistSegA / 2 / ((aeDistDivA + aeAngDiv) / 2);
                //double aeB = accDistSegB / 2 / ((aeDistDivB + aeAngDiv) / 2);

                //AreaError = aeA + aeB;
            }
        }
    }
}
