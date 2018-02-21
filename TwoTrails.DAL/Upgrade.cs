using CSUtil.Databases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public static class Upgrade
    {
        //Old TTX Files
        public static bool DAL(TtSqliteDataAccessLayer dal)
        {
            String file = dal.FilePath;
            String oldfile = $"{file}.old";

            Version oldVersion = dal.GetDataVersion();

            File.Copy(file, oldfile);

            SQLiteDatabase _Database = dal._Database;

            try
            {
                _Database.Update(TwoTrailsSchema.ProjectInfoSchema.TableName,
                    new Dictionary<string, object>()
                    {
                        [TwoTrailsSchema.ProjectInfoSchema.TtDbSchemaVersion] = TwoTrailsSchema.SchemaVersion.ToString()
                    });

                if (oldVersion < TwoTrailsSchema.OSV_2_0_2)
                {
                    _Database.ExecuteNonQuery(TwoTrailsSchema.UPGRADE_OSV_2_0_2);
                }

                Trace.WriteLine($"Upgrade ({oldVersion} -> {TwoTrailsSchema.SchemaVersion}): {file}");
            }
            catch (Exception ex)
            {
                //restore old file
                File.Delete(file);
                File.Copy(oldfile, file);
                
                Trace.WriteLine(ex.Message, $"Upgrade:DAL (TTX): {file}");

                return false;
            }

            return true;
        }

        //TwoTrails V2 Files
        public static void DAL(ITtDataLayer ndal, ITtSettings settings, TtV2SqliteDataAccessLayer odal)
        {
            TtUserAction activity = new TtUserAction("Upgrader", settings.DeviceName);

            IEnumerable<TtMetadata> meta = odal.GetMetadata();
            if (meta.Any())
            {
                ndal.InsertMetadata(meta);
                activity.UpdateAction(DataActionType.InsertedMetadata);
            }

            IEnumerable<TtGroup> groups = odal.GetGroups();
            if (groups.Any())
            {
                ndal.InsertGroups(groups);
                activity.UpdateAction(DataActionType.InsertedGroups);
            }

            DateTime time = ndal.GetProjectInfo().CreationDate;
            IEnumerable<TtPolygon> polys = odal.GetPolygons().Select(
                    poly => {
                        poly.TimeCreated = (time = time.AddSeconds(1));
                        return poly;
                    });

            if (polys.Any())
            {
                ndal.InsertPolygons(polys);
                activity.UpdateAction(DataActionType.InsertedPolygons);

                List<TtPoint> points = new List<TtPoint>();

                foreach (TtPolygon poly in polys)
                {
                    int i = 0;
                    points.AddRange(odal.GetPoints(poly.CN).Select(p => { p.Index = i++; return p; }));
                }

                if (points.Any())
                {
                    ndal.InsertPoints(points);
                    activity.UpdateAction(DataActionType.InsertedPoints);
                }
            }

            IEnumerable<TtNmeaBurst> nmea = odal.GetNmeaBursts();
            if (nmea.Any())
            {
                ndal.InsertNmeaBursts(nmea);
            }

            ndal.InsertActivity(activity);

            TtManager manger = new TtManager(ndal, null, settings);

            manger.RecalculatePolygons(false);
            manger.Save();
        }
    }
}
