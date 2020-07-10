using FMSC.Core;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public class DataHelper
    {
        public static DataErrors Analyze(ITtManager manager)
        {
            DataErrors errors = DataErrors.None;

            Dictionary<string, TtPoint> points = manager.GetPoints().ToDictionary(p => p.CN, p => p);

            //check rezone
            foreach (TtPoint point in points.Values)
            {
                if (!errors.HasFlag(DataErrors.MiszonedPoints) && point.IsGpsType() && point is GpsPoint gps)
                {
                    //get real lat and lon
                    Point latLon = gps.HasLatLon ? new Point((double)gps.Longitude, (double)gps.Latitude) : UTMTools.ConvertUTMtoLatLonSignedDecAsPoint(point.UnAdjX, point.UnAdjY, point.Metadata.Zone);
                    //get real utm
                    UTMCoords realCoords = UTMTools.ConvertLatLonSignedDecToUTM(latLon.Y, latLon.X);

                    if (realCoords.Zone != point.Metadata.Zone)
                    {
                        errors |= DataErrors.MiszonedPoints;
                    }
                }
                
                //check for orphans
                if (!errors.HasFlag(DataErrors.OrphanedQuondams) && point.OpType == OpType.Quondam && point is QuondamPoint qp)
                {
                    if (qp.ParentPoint == null || !points.ContainsKey(qp.ParentPointCN))
                        errors |= DataErrors.OrphanedQuondams;
                }
            }

            //check for empty polygons
            if (manager.GetPolygons().Any(poly => !points.Values.Any(p => p.PolygonCN == poly.CN)))
                errors |= DataErrors.EmptyPolygons;

            //check for unused metadata
            if (manager.GetMetadata().Any(meta => !points.Values.Any(p => p.MetadataCN == meta.CN)))
                errors |= DataErrors.UnusedMetadata;

            //check for unused groups
            if (manager.GetGroups().Any(group => !points.Values.Any(g => g.GroupCN == group.CN)))
                errors |= DataErrors.UnusedGroups;

            //check for duplicate metadata
            if (manager.GetMetadata()
                .GroupBy(meta =>
                    new { meta.Zone, meta.Comment, meta.Compass, meta.Crew, meta.Datum, meta.DecType, meta.Distance, meta.Elevation, meta.GpsReceiver, meta.MagDec, meta.RangeFinder, meta.Slope })
                    .Any(group => group.Count() > 1))
                errors |= DataErrors.DuplicateMetadata;

            return errors;
        }
    }

    [Flags]
    public enum DataErrors
    {
        None = 0,
        MiszonedPoints = 1 << 0,
        OrphanedQuondams = 1 << 1,
        EmptyPolygons = 1 << 2,
        UnusedMetadata = 1 << 3,
        UnusedGroups = 1 << 4,
        DuplicateMetadata = 1 << 5
    }
}
