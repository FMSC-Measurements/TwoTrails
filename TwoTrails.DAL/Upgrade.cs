using CSUtil.Databases;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.DAL
{
    public static class Upgrade
    {
        //Old TTX Files
        public static bool DAL(TtSqliteDataAccessLayer dal, ITtSettings settings)
        {
            String file = dal.FilePath;
            String oldfile = $"{file}.old";

            Version oldVersion = dal.GetDataVersion();

            File.Copy(file, oldfile, true);

            SQLiteDatabase _Database = dal._Database;

            try
            {
                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        if (oldVersion < TwoTrailsSchema.OSV_2_0_2)
                        {
                            _Database.ExecuteNonQuery(TwoTrailsSchema.UPGRADE_OSV_2_0_2, conn, trans);
                        }

                        if (oldVersion < TwoTrailsSchema.OSV_2_0_3 && dal.GetProjectInfo().CreationVersion.StartsWith("ANDROID"))
                        {
                            _Database.ExecuteNonQuery(TwoTrailsSchema.UPGRADE_OSV_2_0_3, conn, trans);
                        }

                        if (oldVersion < TwoTrailsSchema.OSV_2_1_0)
                        {
                            _Database.ExecuteNonQuery(TwoTrailsSchema.UPGRADE_OSV_2_1_0, conn, trans);
                        }

                        _Database.Update(TwoTrailsSchema.ProjectInfoSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TwoTrailsSchema.ProjectInfoSchema.TtDbSchemaVersion] = TwoTrailsSchema.SchemaVersion.ToString()
                            }, null, conn, trans);

                        trans.Commit();

                        dal.InsertActivity(new TtUserAction("Upgrader", settings.DeviceName, $"PC: {settings.AppVersion}", DateTime.Now, DataActionType.ProjectUpgraded, $"DAL {oldVersion} -> {TwoTrailsSchema.SchemaVersion}"));

                        Trace.WriteLine($"Upgrade DAL ({oldVersion} -> {TwoTrailsSchema.SchemaVersion}): {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (File.Exists(oldfile))
                    File.Delete(oldfile);
                
                Trace.WriteLine(ex.Message, $"Upgrade:DAL (TTX): {file}");

                return false;
            }

            return true;
        }

        //TwoTrails V2 Files
        public static void DAL(ITtDataLayer ndal, ITtSettings settings, TtV2SqliteDataAccessLayer odal)
        {
            TtUserAction activity = new TtUserAction("Upgrader", settings.DeviceName, $"PC: {settings.AppVersion}", DateTime.Now, DataActionType.ProjectUpgraded, $"TwoTrailsV2 {odal.GetDataVersion()} -> {TwoTrailsSchema.SchemaVersion}");

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
                    points.AddRange(odal.GetPoints(poly.CN, true).Select(p => { p.Index = i++; return p; }));
                }

                foreach (QuondamPoint qpoint in points.Where(p => p.OpType == OpType.Quondam).ToList())
                {
                    if (!points.Any(p => p.CN == qpoint.ParentPointCN))
                    {
                        TtPoint cPoint = ndal.GetPoint(qpoint.ParentPointCN) ?? qpoint;

                        GpsPoint gpsPoint = new GpsPoint(cPoint)
                        {
                            CN = qpoint.CN,
                            Polygon = qpoint.Polygon,
                            Metadata = qpoint.Metadata,
                            Group = qpoint.Group,
                            Comment = string.IsNullOrWhiteSpace(qpoint.Comment) ?
                                (cPoint.OpType == OpType.Quondam ? qpoint.ParentPoint.Comment : cPoint.Comment) : qpoint.Comment,
                            TimeCreated = DateTime.Now
                        };

                        points.Remove(qpoint);
                        points.Add(gpsPoint);
                    }
                }

                if (points.Any())
                {
                    ndal.InsertPoints(points);
                    activity.UpdateAction(DataActionType.InsertedPoints);
                }
            }

            
            List<TtNmeaBurst> nmea = odal.GetNmeaBursts().ToList();
            if (nmea.Any())
            {
                ndal.InsertNmeaBursts(nmea);
                activity.UpdateAction(DataActionType.InsertedNmea);
            }

            ndal.InsertActivity(activity);

            TtManager manger = new TtManager(ndal, null, settings);

            manger.RecalculatePolygons(false);
            manger.Save();
        }
    }
}
