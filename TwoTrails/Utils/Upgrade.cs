using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.DAL;

namespace TwoTrails.Utils
{
    public static class Upgrade
    {
        public static void DAL(ITtDataLayer ndal, ITtSettings settings, TtV2SqliteDataAccessLayer odal)
        {
            TtUserActivity activity = new TtUserActivity("Upgrader", settings.DeviceName);
            
            List<TtMetadata> meta = odal.GetMetadata();
            if (meta.Any())
            {
                ndal.InsertMetadata(meta);
                activity.UpdateActivity(DataActivityType.InsertedMetadata);
            }

            List<TtGroup> groups = odal.GetGroups();
            if (groups.Any())
            {
                ndal.InsertGroups(groups);
                activity.UpdateActivity(DataActivityType.InsertedGroups);
            }

            DateTime time = ndal.GetProjectInfo().CreationDate;
            IEnumerable<TtPolygon> polys = odal.GetPolygons().Select(
                    poly => {
                        time = time.AddHours(1);
                        poly.TimeCreated = time;

                        return poly;
                    });

            if (polys.Any())
            {
                ndal.InsertPolygons(polys);
                activity.UpdateActivity(DataActivityType.InsertedPolygons);

                List<TtPoint> points = new List<TtPoint>();

                foreach (TtPolygon poly in polys)
                {
                    points.AddRange(odal.GetPointsUnlinked(poly.CN));
                }

                if (points.Any())
                {
                    ndal.InsertPoints(points);
                    activity.UpdateActivity(DataActivityType.InsertedPoints);
                }
            }

            List<TtNmeaBurst> nmea = odal.GetNmeaBursts();
            if (nmea.Any())
            {
                ndal.InsertNmeaBursts(nmea);
            }

            ndal.InsertActivity(activity);

            TtManager manger = new TtManager(ndal, settings);

            manger.RecalculatePolygons();
            manger.Save();
        }
    }
}
