using FMSC.Core;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using Point = FMSC.Core.Point;

namespace TwoTrails.Utils
{
    public static class TtUtils
    {
        public static UpdateStatus CheckForUpdate()
        {
            UpdateStatus status = new UpdateStatus();

            try
            {
                string res = new WebClient().DownloadString(Consts.URL_TWOTRAILS_UPDATE);

                if (res != null)
                {
                    string[] tokens = res.Split('\n').Select(l => l.Trim('\r', ' ')).ToArray();

                    if (tokens.Length > 0)
                    {
                        status.CheckStatus = new Version(tokens[0].Trim()) > Assembly.GetExecutingAssembly().GetName().Version;

                        if (status.CheckStatus == true && tokens.Length > 1)
                        {
                            UpdateType updateType = UpdateType.None;

                            //for (int r = 1; r < tokens.Length; r += 3)
                            //{
                                string updateStr = tokens[1];
                                for (int i = 0; i < 5 && i < updateStr.Length; i++)
                                {
                                    if (updateStr[i] != '0')
                                        updateType |= (UpdateType)(1 << (i + 1));
                                }
                            //}

                            status.UpdateType = updateType;

                            status.UpdateMessage = (tokens.Length > 2) ? tokens[2] : String.Empty;

                        }
                    }
                }
            }
            catch
            {

            }

            return status;
        }


        public static UtmExtent GetExtents(IEnumerable<TtPoint> points, bool adjusted = true)
        {
            if (!points.Any())
                throw new ArgumentException("Points contains no points");

            int zone = points.First().Metadata.Zone;

            UtmExtent.Builder buider = new UtmExtent.Builder(zone);
            
            foreach (TtPoint p in points)
                buider.Include(p.GetCoords(zone, adjusted));

            return buider.Build();
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
                //case Consts.SHAPE_PRJ_EXT:
                //case Consts.SHAPE_SHX_EXT:
                //case Consts.SHAPE_DBF_EXT:
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

    public struct UpdateStatus
    {
        public bool? CheckStatus { get; set; }
        public UpdateType UpdateType { get; set; }
        public string UpdateMessage { get; set; }
    }

    [Flags]
    public enum UpdateType
    {
        None = 0,
        MinorBugFixes = 1 << 1,
        MajorBugFixes = 1 << 2,
        CriticalBugFixes = 1 << 3,
        Optimiztions = 1 << 4,
        FeatureUpdates = 1 << 5
    }
}
