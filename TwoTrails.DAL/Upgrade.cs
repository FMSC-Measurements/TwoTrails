using FMSC.Core.Databases;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

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

            Dictionary<String, TtMetadata> meta = odal.GetMetadata().ToDictionary(m => m.CN, m => m);
            if (meta.Any())
            {
                ndal.InsertMetadata(meta.Values);
                activity.UpdateAction(DataActionType.InsertedMetadata);
            }

            Dictionary<String, TtGroup> groups = odal.GetGroups().ToDictionary(g => g.CN, g => g);
            if (groups.Any())
            {
                ndal.InsertGroups(groups.Values);
                activity.UpdateAction(DataActionType.InsertedGroups);
            }

            DateTime time = ndal.GetProjectInfo().CreationDate;
            Dictionary<String, TtPolygon> polys = odal.GetPolygons().Select(
                    poly => {
                        poly.TimeCreated = (time = time.AddSeconds(1));
                        return poly;
                    }).ToDictionary(p => p.CN, p => p);

            if (polys.Any())
            {
                ndal.InsertPolygons(polys.Values);
                activity.UpdateAction(DataActionType.InsertedPolygons);

                Dictionary<String, TtPoint> points = new Dictionary<string, TtPoint>();

                foreach (TtPolygon poly in polys.Values)
                {
                    int i = 0;
                    foreach (TtPoint point in odal.GetPoints(poly.CN, false).Select(p => { 
                        p.Index = i++;
                        p.Polygon = polys[p.PolygonCN];
                        p.Metadata = meta[p.MetadataCN];
                        p.Group = groups[p.GroupCN];
                        return p; 
                    }))
                    {
                        points.Add(point.CN, point);
                    }
                }

                foreach (QuondamPoint qpoint in points.Values.ToList().Where(p => p.OpType == OpType.Quondam).ToList())
                {
                    if (!points.ContainsKey(qpoint.ParentPointCN))
                    {
                        TtPoint cPoint =  ndal.GetPoint(qpoint.ParentPointCN) ?? qpoint;

                        GpsPoint gpsPoint = new GpsPoint(cPoint)
                        {
                            CN = qpoint.CN,
                            Polygon = qpoint.Polygon,
                            Metadata = qpoint.Metadata,
                            Group = qpoint.Group,
                            Comment = string.IsNullOrWhiteSpace(qpoint.Comment) ?
                                (string.IsNullOrWhiteSpace(cPoint.Comment) ? String.Empty : cPoint.Comment) : qpoint.Comment,
                            TimeCreated = DateTime.Now
                        };

                        points[qpoint.CN] = gpsPoint;
                    }
                    else
                    {
                        qpoint.ParentPoint = points[qpoint.ParentPointCN];
                    }
                }

                if (points.Any())
                {
                    ndal.InsertPoints(points.Values);
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
