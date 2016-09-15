using FMSC.GeoSpatial;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public static class TtUtils
    {

        public static UtmExtent GetBoundaries(IEnumerable<TtPoint> points, bool adjusted = true)
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

                if (!adjusted && gps != null && gps.HasLatLon)
                {
                    return UTMTools.convertLatLonSignedDecToUTM((double)gps.Latitude, (double)gps.Longitude, targetZone);
                }
                else //Use reverse location calculation
                {
                    Position pos;

                    if (adjusted)
                        pos = UTMTools.convertUTMtoLatLonSignedDec(point.AdjX, point.AdjY, point.Metadata.Zone);
                    else
                        pos = UTMTools.convertUTMtoLatLonSignedDec(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);

                    return UTMTools.convertLatLonToUTM(pos, targetZone);
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
    }
}
