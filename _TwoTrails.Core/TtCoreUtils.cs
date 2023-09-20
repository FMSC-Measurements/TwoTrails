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

        public static bool IsPolygonAnIsland(this ITtManager manager, string polyCN)
        {
            return IsPolygonAnIsland(manager.GetPoints(polyCN));
        }

        public static bool IsPolygonAnIsland(IEnumerable<TtPoint> points)
        {
            if (points.HasAtLeast(3, pt => pt.OnBoundary))
            {
                bool hasGps = false;
                int sideShotCount = 0;

                foreach (TtPoint pt in points)
                {
                    if (!hasGps && !pt.OnBoundary)
                    {
                        if (pt.IsGpsAtBase())
                            hasGps = true;
                    }
                    else if (hasGps && pt.OnBoundary)
                    {
                        if (pt.OpType == OpType.SideShot)
                            sideShotCount++;
                        else
                            break;
                    }
                    else
                        break;
                }

                if (sideShotCount > 2)
                    return true;
            }

            return false;
        }

 

        public static Tuple<double, double, double> CalculateAreaPerimeterAndOnBoundTrail(this ITtManager manager, string polyCN)
        {
            return TtCoreUtils.CalculateAreaPerimeterAndOnBoundTrail(manager.GetPoints(polyCN).Where(p => p.IsBndPoint()).ToList());
        }

        public static Tuple<double, double, double> CalculateAreaPerimeterAndOnBoundTrail(List<TtPoint> points)
        {
            if (points.Count > 2)
            {
                double perim = 0, linePerim, area = 0;

                TtPoint p1 = points[0], fBndPt = null, lBndPt;
                TtPoint p2;

                lBndPt = p1;

                for (int i = 0; i < points.Count - 1; i++)
                {
                    p1 = points[i];
                    p2 = points[i + 1];

                    if (fBndPt == null)
                        fBndPt = p1;

                    lBndPt = p2;

                    perim += MathEx.Distance(p1.AdjX, p1.AdjY, p2.AdjX, p2.AdjY);
                    area += (p2.AdjX - p1.AdjX) * (p2.AdjY + p1.AdjY);
                }

                linePerim = perim;

                if (!fBndPt.HasSameAdjLocation(lBndPt))
                {
                    perim += MathEx.Distance(fBndPt.AdjX, fBndPt.AdjY, lBndPt.AdjX, lBndPt.AdjY);
                    area += (fBndPt.AdjX - lBndPt.AdjX) * (fBndPt.AdjY + lBndPt.AdjY);
                }

                return Tuple.Create(Math.Abs(area) / 2, perim, linePerim);
            }
            else
            {
                return Tuple.Create(0d, 0d, 0d);
            }
        }
    }
}
