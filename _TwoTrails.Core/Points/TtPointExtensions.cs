using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoTrails.Core.Points
{
    public static class TtPointExtensions
    {
        public static bool IsGpsType(this TtPoint point)
        {
            return point.OpType == OpType.GPS || point.OpType == OpType.Take5 ||
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
            return point.OnBoundary && point.OpType != OpType.WayPoint;
        }

        public static bool IsNavPoint(this TtPoint point)
        {
            switch (point.OpType)
            {
                case OpType.GPS:
                case OpType.Take5:
                case OpType.Traverse:
                case OpType.Walk:
                    return true;
            }

            return false;
        }
        
        public static bool IsMiscPoint(this TtPoint point)
        {
            return point.OpType == OpType.SideShot && !point.OnBoundary;
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


        public static TtPoint ConvertQuondam(this QuondamPoint point)
        {
            TtPoint conversion = point.ParentPoint.DeepCopy();

            conversion.PID = point.PID;
            conversion.Index = point.Index;
            conversion.Polygon = point.Polygon;
            conversion.Group = point.Group;
            conversion.OnBoundary = point.OnBoundary;
            conversion.Comment = String.IsNullOrEmpty(point.Comment) ? conversion.Comment : point.Comment;
            conversion.TimeCreated = point.TimeCreated;
            conversion.ClearLinkedPoints();

            return conversion;
        }
    }
}
