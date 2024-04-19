using FMSC.Core;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public static class TtCoreUtils
    {
        public static TtPoint GetPointByType(OpType op)
        {
            switch (op)
            {
                case OpType.GPS: return new GpsPoint();
                case OpType.Take5: return new Take5Point();
                case OpType.Traverse: return new TravPoint();
                case OpType.SideShot: return new SideShotPoint();
                case OpType.Quondam: return new QuondamPoint();
                case OpType.Walk: return new WalkPoint();
                case OpType.WayPoint: return new WayPoint();
            }

            throw new Exception("Unknown OpType");
        }

        public static DateTime ParseTime(String value)
        {
            if (!DateTime.TryParseExact(value, Consts.DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
            {
                if (!DateTime.TryParse(value, out time))
                {
                    time = DateTime.Now;
                }
            }

            return time;
        }


        public static void ChangeGpsZone(GpsPoint point, int zone, int oldZone)
        {
            UTMCoords coords;

            if (point.HasLatLon)
            {
                coords = UTMTools.ConvertLatLonSignedDecToUTM((double)point.Latitude, (double)point.Longitude, zone);
            }
            else
            {
                coords = UTMTools.ShiftZones(point.UnAdjX, point.UnAdjY, zone, oldZone);
            }

            point.SetUnAdjLocation(coords.X, coords.Y, point.UnAdjZ);
        }



        public static bool IsPolygonValid(this ITtManager manager, string polyCN)
        {
            if (manager.PolygonExists(polyCN))
                return manager.GetPoints(polyCN).HasAtLeast(3, pt => pt.OnBoundary);
            throw new Exception("Polygon Not Found");
        }



        public static APStats CalculateBoundaryAreaPerimeterAndTrail(this ITtManager manager, string polyCN)
        {
            return CalculateAreaPerimeterAndTrail(manager.GetPoints(polyCN).OnBndPoints());
        }

        public static APStats CalculateBoundaryAreaPerimeterAndTrail(IEnumerable<TtPoint> points)
        {
            return CalculateAreaPerimeterAndTrail(points.OnBndPoints());
        }

        public static APStats CalculateAreaPerimeterAndTrail(IEnumerable<TtPoint> points)
        {
            return CalculateAreaPerimeterAndTrail(points.SyncPointsToZone().ToList());
        }
        
        public static APStats CalculateAreaPerimeterAndTrail(List<Point> points)
        {
            if (points.Count > 2)
            {
                double perim = 0, linePerim, area = 0;

                Point p1 = points[0], fBndPt = points[0], lBndPt;
                Point p2;

                lBndPt = p1;

                for (int i = 0; i < points.Count - 1; i++)
                {
                    p1 = points[i];
                    p2 = points[i + 1];

                    lBndPt = p2;

                    perim += MathEx.Distance(p1.X, p1.Y, p2.X, p2.Y);
                    area += (p2.X - p1.X) * (p2.Y + p1.Y);
                }

                linePerim = perim;

                if (fBndPt != lBndPt)
                {
                    perim += MathEx.Distance(fBndPt.X, fBndPt.Y, lBndPt.X, lBndPt.Y);
                    area += (fBndPt.X - lBndPt.X) * (fBndPt.Y + lBndPt.Y);
                }

                return new APStats(Math.Abs(area) / 2, perim, linePerim);
            }
            else
            {
                return new APStats();
            }
        }
    }
}
