//#define USE_VARIABLE_DIST_MULTIPLIER

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

#if USE_VARIABLE_DIST_MULTIPLIER
        public const double MIN_DIST_MULTIPLIER = 5d;
        public const double MAX_DIST_MULTIPLIER = 10d;
#else
        public const double MIN_DIST_MULTIPLIER = 10d;
#endif

        public const double MIN_DIVISOR = 1d;
        public const double MAX_DIVISOR = 2d;

        public AnglePointResult Flags { get; private set; }

        private double PolygonArea { get; set; }

        public double TotalGpsError { get; private set; }
        public double TotalError { get; private set; }
        public double GpsAreaError => PolygonArea > 0 ? TotalGpsError / PolygonArea * 100d : 0;
        public double AreaError => PolygonArea > 0 ? TotalError / PolygonArea * 100d : 0;

        public double TotalReduction => GpsAreaError > 0 ? (GpsAreaError - AreaError) : 0;


        public int LongLegs { get; private set; }
        public int SharpEdges { get; private set; }

        public double LongLegsPercent => Segments.Count > 0 ? LongLegs * 100d / Segments.Count : 0;
        public double SharpEdgesPercent => Segments.Count > 0 ? SharpEdges * 100d / Segments.Count : 0;


        public List<GERSegment> Segments { get; private set; } = new List<GERSegment>();



        public GeometricErrorReductionResult(ITtManager manager, TtPolygon polygon) :
            this(manager.GetPoints(polygon.CN).Where(p => p.OnBoundary).ToList(), polygon.Area)
        { }

        public GeometricErrorReductionResult(List<TtPoint> points, double polygonArea)
        {
            PolygonArea = polygonArea;

            if (points.Count > 2)
            {
                int targetZone = points[0].Metadata.Zone;

                TtPoint lastPoint = points[points.Count - 1];
                TtPoint currPoint = points[0];
                TtPoint nextPoint;

                if (currPoint.HasSameAdjLocation(lastPoint))
                {
                    for (int i = points.Count - 2; i > 1; i--)
                    {
                        lastPoint = points[i];
                        if (currPoint.HasSameAdjLocation(lastPoint))
                        {
                            points.RemoveAt(i + 1);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (points.Count < 3)
                    {
                        Flags |= AnglePointResult.NotEnoughCorners;
                        return;
                    }
                }
                else
                {
                    points.Add(currPoint);
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

                bool shortLegA = distSegA < minDistSegA;

                double accDistSegA = accSegA * distSegA;
                double accDistSegB;

#if USE_VARIABLE_DIST_MULTIPLIER
                double maxDistSegA = accSegA * MIN_DIST_MULTIPLIER;

                double maxDistSegB;

                double aeDistDivA = distSegA < minDistSegA ? MIN_DIVISOR :
                            (distSegA >= maxDistSegA) ? MAX_DIVISOR :
                                distSegA / minDistSegA;
                double aeDistDivB;
#endif

                for (int i = 0; i < points.Count - 1; i++)
                {
                    nextPoint = points[i + 1];
                    nextCoords = nextPoint.GetCoords(targetZone);

                    double angle = MathEx.CalculateAngleBetweenPoints(
                        lastCoords.X, lastCoords.Y,
                        currCoords.X, currCoords.Y,
                        nextCoords.X, nextCoords.Y);

                    angle = Math.Round(angle % 180, 8);
                    if (angle > MAXIMUM_ANGLE)
                        angle = MAXIMUM_ANGLE - (angle % MAXIMUM_ANGLE);

                    distSegB = MathEx.Distance(currCoords.X, currCoords.Y, nextCoords.X, nextCoords.Y);

                    accSegB = (currPoint.Accuracy + nextPoint.Accuracy) / 2d;

                    minDistSegB = accSegB * MIN_DIST_MULTIPLIER;

                    accDistSegB = accSegB * distSegB;

                    double segmentLength = (distSegA + distSegB) / 2d;

                    bool sharpEdge = angle >= MINIMUM_ANGLE;

                    bool shortLegB = distSegB < minDistSegB;

                    if (distSegB >= minDistSegB)
                        LongLegs++;

                    if (sharpEdge)
                        SharpEdges++;


#if USE_VARIABLE_DIST_MULTIPLIER
                    double aeAngDiv = sharpEdge ? ((angle >= MAXIMUM_ANGLE) ? MAX_DIVISOR : angle / MINIMUM_ANGLE) : MIN_DIVISOR;
                    maxDistSegB = accSegB * MAX_DIST_MULTIPLIER;

                    aeDistDivB = shortLegB ? MIN_DIVISOR :
                                (distSegB >= maxDistSegB) ? MAX_DIVISOR :
                                    distSegB / minDistSegB;

                    double aeA = accDistSegA / 2 / (sharpEdge ? ((aeDistDivA + aeAngDiv) / 2) : MIN_DIVISOR);
                    double aeB = accDistSegB / 2 / (sharpEdge ? ((aeDistDivB + aeAngDiv) / 2) : MIN_DIVISOR);
#else
                    double aeDiv = (sharpEdge && !shortLegA && !shortLegB) ?
                        ((angle >= MAXIMUM_ANGLE) ? MAX_DIVISOR : angle / MINIMUM_ANGLE) : MIN_DIVISOR;
                    double aeA = accDistSegA / 2 / aeDiv;
                    double aeB = accDistSegB / 2 / aeDiv;
#endif

                    double segAreaError = aeA + aeB;

                    Segments.Add(new GERSegment(lastPoint, currPoint, nextPoint, segAreaError, angle, segmentLength));

                    TotalError += segAreaError;
                    TotalGpsError += accDistSegB;


                    lastPoint = currPoint;
                    lastCoords = currCoords;

                    currPoint = nextPoint;
                    currCoords = nextCoords;

                    distSegA = distSegB;
                    //accSegA = accSegB;
                    minDistSegA = minDistSegB;
                    //maxDistSegA = maxDistSegB;
                    accDistSegA = accDistSegB;
                    shortLegA = shortLegB;

#if USE_VARIABLE_DIST_MULTIPLIER
                    aeDistDivA = aeDistDivB;
#endif
                }
            }
            else
            {
                Flags |= AnglePointResult.NotEnoughBoundaryPoints;
            }
        }

        public class GERSegment
        {
            public TtPoint Point1 { get; }
            public TtPoint Point2 { get; }
            public TtPoint Point3 { get; }

            public bool SharpEdge => Edge >= MINIMUM_ANGLE;
            public bool ShallowEdge => Edge < MINIMUM_ANGLE;

            public double AreaError { get; }
            public double Edge { get; }
            public double SegmentLength { get; }

            public GERSegment(TtPoint p1, TtPoint p2, TtPoint p3, double areaError, double edge, double segLength)
            {
                Point1 = p1;
                Point2 = p2;
                Point3 = p3;

                AreaError = areaError;
                Edge = edge;
                SegmentLength = segLength;
            }
        }
    }
}
