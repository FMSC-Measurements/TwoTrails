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
        public static void DAL(ITtDataLayer cDal, ITtSettings settings, TtV2SqliteDataAccessLayer dal)
        {
            TtUserActivity activity = new TtUserActivity("Upgrader", settings.DeviceName);

            List<TtMetadata> meta = dal.GetMetadata();
            if (meta.Any())
            {
                cDal.InsertMetadata(meta);
                activity.UpdateActivity(DataActivityType.InsertedMetadata);
            }

            List<TtGroup> groups = dal.GetGroups();
            if (groups.Any())
            {
                cDal.InsertGroups(groups);
                activity.UpdateActivity(DataActivityType.InsertedGroups);
            }

            List<TtPolygon> polys = dal.GetPolygons();
            if (polys.Any())
            {
                cDal.InsertPolygons(polys);
                activity.UpdateActivity(DataActivityType.InsertedPolygons);
            }

            List<TtPoint> points = dal.GetPoints();
            if (points.Any())
            {
                cDal.InsertPoints(points);
                activity.UpdateActivity(DataActivityType.InsertedPoints);
            }

            List<TtNmeaBurst> nmea = dal.GetNmeaBursts();
            if (nmea.Any())
            {
                cDal.InsertNmeaBursts(nmea);
            }

            cDal.UpdateProjectInfo(dal.GetProjectInfo());
            activity.UpdateActivity(DataActivityType.ModifiedProject);

            cDal.InsertActivity(activity);

            TtManager manger = new TtManager(cDal, settings);

            manger.RecalculatePolygons();
            manger.Save();
        }
    }
}
