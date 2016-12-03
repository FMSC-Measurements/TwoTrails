using FMSC.Core;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public static class TtUtils
    {

        public static UtmExtent GetExtents(IEnumerable<TtPoint> points, bool adjusted = true)
        {
            if (!points.Any())
                throw new ArgumentException("Points contains no points");

            int zone = points.First().Metadata.Zone;

            UtmExtent.Builder buider = new UtmExtent.Builder(zone);
            
            foreach (TtPoint p in points)
                buider.Include(GetCoords(p, zone, adjusted));

            return buider.Build();
        }


        public static UTMCoords GetCoords(TtPoint point, int targetZone, bool adjusted = true)
        {
            if (point.Metadata.Zone != targetZone)
            {
                GpsPoint gps = point as GpsPoint;

                if (gps != null && gps.HasLatLon)
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

        public static Point GetLatLon(TtPoint point, bool adjusted = true)
        {
            GpsPoint gps = point as GpsPoint;
            if (gps != null && gps.HasLatLon)
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


        public static Point GetFarthestCorner(double pX, double pY, double top, double bottom, double left, double right)
        {
            Point fp;

            double dist, temp;

            dist = MathEx.Distance(pX, pY, left, top);
            fp = new Point(left, top);

            temp = MathEx.Distance(pX, pY, right, top);

            if (temp > dist)
            {
                dist = temp;
                fp.X = right;
                fp.Y = top;
            }

            temp = MathEx.Distance(pX, pY, left, bottom);

            if (temp > dist)
            {
                dist = temp;
                fp.X = left;
                fp.Y = bottom;
            }

            temp = MathEx.Distance(pX, pY, right, bottom);

            if (temp > dist)
            {
                fp.X = right;
                fp.Y = bottom;
            }

            return fp;
        }


        public static TimeSpan GetPolyCreationPeriod(IEnumerable<TtPoint> points)
        {
            if (!points.Any())
                throw new Exception("No Points");

            DateTime start = points.First().TimeCreated;
            DateTime end = start;

            foreach (TtPoint p in points)
            {
                if (p.TimeCreated < start)
                    start = p.TimeCreated;

                if (p.TimeCreated > end)
                    end = p.TimeCreated;
            }

            return new TimeSpan((end - start).Ticks);
        }
    }
}
