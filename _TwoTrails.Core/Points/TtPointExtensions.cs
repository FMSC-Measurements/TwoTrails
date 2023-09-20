using FMSC.Core;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TwoTrails.Core.Points
{
    public static class TtPointExtensions
    {
        public static bool IsGpsType(this TtPoint point)
        {
            return point.OpType == OpType.Take5 || point.OpType == OpType.GPS ||
                point.OpType == OpType.Walk || point.OpType == OpType.WayPoint;
        }

        public static bool IsTravType(this TtPoint point)
        {
            return point.OpType == OpType.Traverse || point.OpType == OpType.SideShot;
        }

        public static bool IsGpsAtBase(this TtPoint point)
        {
            return IsGpsType(point) || (point.OpType == OpType.Quondam && ((QuondamPoint)point).ParentPoint.IsGpsType());
        }

        public static bool IsBndPoint(this TtPoint point)
        {
            return point.OnBoundary;
        }

        public static bool IsNavPoint(this TtPoint point)
        {
            return point.OpType == OpType.Take5 || point.OpType == OpType.GPS ||
                point.OpType == OpType.Traverse || point.OpType == OpType.Walk;
        }
        
        public static bool IsMiscPoint(this TtPoint point)
        {
            return (point.OpType == OpType.SideShot || point.OpType == OpType.Quondam) && !point.OnBoundary;
        }

        public static bool IsWayPointAtBase(this TtPoint point)
        {
            return point.OpType == OpType.WayPoint || (point is QuondamPoint qp && qp.ParentPoint.OpType == OpType.WayPoint);
        }

        public static bool IsManualAccType(this TtPoint point)
        {
            return IsGpsType(point) || point.OpType == OpType.Quondam;
        }

        public static bool HasSameUnAdjLocation(this TtPoint point, TtPoint otherPoint)
        {
            return point.UnAdjX == otherPoint.UnAdjX &&
                point.UnAdjY == otherPoint.UnAdjY &&
                point.UnAdjY == otherPoint.UnAdjY;
        }

        public static bool HasSameAdjLocation(this TtPoint point, TtPoint otherPoint)
        {
            return point.AdjX == otherPoint.AdjX &&
                point.AdjY == otherPoint.AdjY &&
                point.AdjY == otherPoint.AdjY;
        }
        

        public static TtPoint DeepCopy(this TtPoint point)
        {
            switch (point.OpType)
            {
                case OpType.GPS: return new GpsPoint(point);
                case OpType.Take5: return new Take5Point(point);
                case OpType.Traverse: return new TravPoint(point);
                case OpType.SideShot: return new SideShotPoint(point);
                case OpType.Quondam: return new QuondamPoint(point);
                case OpType.Walk: return new WalkPoint(point);
                case OpType.WayPoint: return new WayPoint(point);
            }

            return null;
        }

        public static IEnumerable<TtPoint> DeepCopy(this IEnumerable<TtPoint> points)
        {
            return points.Select(p => p.DeepCopy());
        }


        public static TtPoint CreatePoint(this OpType opType)
        {
            switch (opType)
            {
                case OpType.GPS: return new GpsPoint();
                case OpType.Take5: return new Take5Point();
                case OpType.Traverse: return new TravPoint();
                case OpType.SideShot: return new SideShotPoint();
                case OpType.Quondam: return new QuondamPoint();
                case OpType.Walk: return new WalkPoint();
                case OpType.WayPoint: return new WayPoint();
            }

            return null;
        }


        public static GpsPoint ConvertQuondam(this QuondamPoint point)
        {
            GpsPoint conversion = point.IsGpsAtBase() ? (GpsPoint)point.ParentPoint.DeepCopy() : new GpsPoint(point.ParentPoint);

            conversion.CN = point.CN;
            conversion.Index = point.Index;
            conversion.Polygon = point.Polygon;
            conversion.PolygonCN = point.PolygonCN;
            conversion.Group = point.Group;
            conversion.GroupCN = point.GroupCN;
            conversion.OnBoundary = point.OnBoundary;

            if (point.ManualAccuracy != null)
                conversion.ManualAccuracy = point.ManualAccuracy;

            if (!string.IsNullOrWhiteSpace(point.Comment))
            {
                conversion.Comment = (!string.IsNullOrEmpty(conversion.Comment)) ?
                    $"{point.Comment} | {conversion.Comment}" :
                    point.Comment;
            }

            conversion.ClearLinkedPoints();

            return conversion;
        }


        public static UTMCoords GetCoords(this TtPoint point, int targetZone, bool adjusted = true)
        {
            if (point.Metadata.Zone != targetZone)
            {
                if (point is GpsPoint gps && gps.HasLatLon)
                {
                    return UTMTools.ConvertLatLonSignedDecToUTM((double)gps.Latitude, (double)gps.Longitude, targetZone);
                }
                else //Use reverse location calculation
                {
                    Position pos;

                    if (adjusted)
                        pos = UTMTools.ConvertUTMtoLatLonSignedDec(point.AdjX, point.AdjY, point.Metadata.Zone);
                    else
                        pos = UTMTools.ConvertUTMtoLatLonSignedDec(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);

                    return UTMTools.ConvertLatLonToUTM(pos, targetZone);
                }
            }
            else
            {
                if (adjusted)
                    return new UTMCoords(point.AdjX, point.AdjY, targetZone);
                else
                    return new UTMCoords(point.UnAdjX, point.UnAdjY, targetZone);
            }
        }

        public static Point GetLatLon(this TtPoint point, bool adjusted = true)
        {
            if (point is GpsPoint gps && gps.HasLatLon)
            {
                return new Point((double)gps.Longitude, (double)gps.Latitude);
            }
            else
            {
                if (point.Metadata == null)
                    throw new Exception("Missing Metadata");

                return adjusted ?
                    UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.AdjX, point.AdjY, point.Metadata.Zone) :
                    UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);
            }
        }

    }
}
