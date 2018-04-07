using FMSC.Core;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public static class TtUtils
    {
        public static bool? CheckForUpdate()
        {
            try
            {
                string res = new WebClient().DownloadString(Consts.URL_TWOTRAILS_UPDATE);

                if (res != null)
                {
                    string[] tokens = res.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length > 0)
                    {
                        if (new Version(tokens[0].Trim()) > Assembly.GetExecutingAssembly().GetName().Version)
                            return true;
                    }
                }
            }
            catch
            {
                return null;
            }

            return false;
        }


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

        public static Point GetLatLon(TtPoint point, bool adjusted = true)
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



        public static double AzimuthOfPoint(double x1, double y1, double x2, double y2)
        {
            double xCoord = x2 - x1, yCoord = y2 - y1;

            double az = FMSC.Core.Convert.RadiansToDegrees(Math.Atan2(xCoord, yCoord));

            if (az < 0)
                az += 360;

            return az;
        }



        public static bool IsImportableFileType(String fileName)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case Consts.CSV_EXT:
                case Consts.TEXT_EXT:
                case Consts.KML_EXT:
                case Consts.KMZ_EXT:
                case Consts.GPX_EXT:
                case Consts.SHAPE_EXT:
                case Consts.SHAPE_PRJ_EXT:
                case Consts.SHAPE_SHX_EXT:
                case Consts.SHAPE_DBF_EXT:
                case Consts.FILE_EXTENSION:
                case Consts.FILE_EXTENSION_V2:
                    return true;
            }

            return false;
        }

        public static string GetFileTypeName(string fileName)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case Consts.CSV_EXT: return "CSV";
                case Consts.TEXT_EXT: return "Text";
                case Consts.KML_EXT:
                case Consts.KMZ_EXT: return "Google Earth";
                case Consts.GPX_EXT: return "GPX";
                case Consts.SHAPE_EXT:
                case Consts.SHAPE_PRJ_EXT:
                case Consts.SHAPE_SHX_EXT:
                case Consts.SHAPE_DBF_EXT: return "Shape";
                case Consts.FILE_EXTENSION: return "TwoTrails";
                case Consts.FILE_EXTENSION_V2: return "TwoTrails V2";
            }

            return String.Empty;
        }
    }
}
