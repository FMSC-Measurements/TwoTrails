﻿using CSUtil;
using CSUtil.Databases;
using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Core.Media;

namespace TwoTrails.DAL
{
    public class TtSqliteDataAccessLayer : ITtDataLayer
    {
        private static String DATE_FORMAT = "M/d/yyyy h:mm:ss.SSS";

        public String FilePath { get; }

        private SQLiteDatabase database;


        public TtSqliteDataAccessLayer(String filePath)
        {
            FilePath = filePath;
            database = new SQLiteDatabase(FilePath);
        }

        public TtSqliteDataAccessLayer(SQLiteDatabase database)
        {
            this.FilePath = database.FileName;
            this.database = database;
        }

        public static TtSqliteDataAccessLayer Create(string filePath, TtProjectInfo projectInfo)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            SQLiteDatabase database = new SQLiteDatabase(filePath);
            
            database.ExecuteNonQuery(TwoTrailsSchema.PointSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.PolygonSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.GroupSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.MetadataSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.GpsPointSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.TravPointSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.QuondamPointSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.ProjectInfoSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.TtNmeaSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.MediaSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.PolygonAttrSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.PictureSchema.CreateTable);

            TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(database);
            dal.InsertProjectInfo(projectInfo);

            return dal;
        }


        public bool RequiresUpgrade
        {
            get { return GetDataVersion() < TwoTrailsSchema.SchemaVersion; }
        }



        #region Points
        #region Get Points
        public TtPoint GetPoint(String pointCN)
        {
            List<TtPoint> points = GetPoints(
                String.Format("{0} = '{1}'",
                    TwoTrailsSchema.SharedSchema.CN,
                    pointCN),
                1,
                false
            );

            if (points.Count > 0)
                return points[0];
            return null;
        }

        public List<TtPoint> GetPoints(string polyCN = null)
        {
            return GetPoints(
                polyCN != null ?
                    String.Format("{0} = '{1}'",
                        TwoTrailsSchema.PointSchema.PolyCN,
                        polyCN) :
                    null
            );
        }

        public List<TtPoint> GetPointsUnlinked(string polyCN = null)
        {
            return GetPoints(
                polyCN != null ?
                    String.Format("{0} = '{1}'",
                        TwoTrailsSchema.PointSchema.PolyCN,
                        polyCN) :
                    null,
                0,
                false
            );
        }
        
        protected List<TtPoint> GetPoints(String where, int limit = 0, bool linked = true)
        {
            List<TtPoint> points = new List<TtPoint>();

            String query = String.Format(@"select {0}.{1}, {2}, {3}, {4} from {0} left join {5} on {5}.{8} = {0}.{8} 
 left join {6} on {6}.{8} = {0}.{8}  left join {7} on {7}.{8} = {0}.{8}{9} order by {10} asc",
                TwoTrailsSchema.PointSchema.TableName,              //0
                TwoTrailsSchema.PointSchema.SelectItems,            //1
                TwoTrailsSchema.GpsPointSchema.SelectItemsNoCN,     //2
                TwoTrailsSchema.TravPointSchema.SelectItemsNoCN,    //3
                TwoTrailsSchema.QuondamPointSchema.SelectItemsNoCN, //4
                TwoTrailsSchema.GpsPointSchema.TableName,           //5
                TwoTrailsSchema.TravPointSchema.TableName,          //6
                TwoTrailsSchema.QuondamPointSchema.TableName,       //7
                TwoTrailsSchema.SharedSchema.CN,                    //8
                where != null ? String.Format(" where {0}", where) : String.Empty,
                TwoTrailsSchema.PointSchema.Index
            );

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        TtPoint point = null;
                        OpType op;

                        string cn, polycn, metacn, groupcn, comment, pcn, qlinks;
                        int index, pid;
                        DateTime time;
                        bool onbnd;
                        double adjx, adjy, adjz, unadjx, unadjy, unadjz, acc, sd, sa;
                        double? lat, lon, elev, manacc, rmser, fw, bk;

                        while (dr.Read())
                        {
                            if (dr.IsDBNull(7))
                                throw new Exception("Point has no OpType");
                            
                            cn = dr.GetString(0);
                            index = dr.GetInt32(1);
                            pid = dr.GetInt32(2);
                            polycn = dr.GetString(3);
                            groupcn = dr.GetString(4);
                            onbnd = dr.GetBoolean(5);
                            comment = dr.GetStringN(6);
                            op = (OpType)dr.GetInt32(7);
                            metacn = dr.GetString(8);
                            time = DateTime.Parse(dr.GetString(9));
                            //time = DateTime.ParseExact(dr.GetString(9), DATE_FORMAT, CultureInfo.InvariantCulture);

                            adjx = dr.GetDouble(10);
                            adjy = dr.GetDouble(11);
                            adjz = dr.GetDouble(12);

                            unadjx = dr.GetDouble(13);
                            unadjy = dr.GetDouble(14);
                            unadjz = dr.GetDouble(15);

                            acc = dr.GetDouble(16);

                            qlinks = dr.GetStringN(17);


                            if (op.IsGpsType())
                            {
                                lat = dr.GetDoubleN(18);
                                lon = dr.GetDoubleN(19);
                                elev = dr.GetDoubleN(20);
                                manacc = dr.GetDoubleN(21);
                                rmser = dr.GetDoubleN(22);

                                switch (op)
                                {
                                    case OpType.GPS:
                                        point = new GpsPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    acc, qlinks, lat, lon, elev, manacc, rmser);
                                        break;
                                    case OpType.Take5:
                                        point = new Take5Point(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    acc, qlinks, lat, lon, elev, manacc, rmser);
                                        break;
                                    case OpType.Walk:
                                        point = new WalkPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    acc, qlinks, lat, lon, elev, manacc, rmser);
                                        break;
                                    case OpType.WayPoint:
                                        point = new WayPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    acc, qlinks, lat, lon, elev, manacc, rmser);
                                        break;
                                }
                            }
                            else if (op.IsTravType())
                            {
                                fw = dr.GetDoubleN(23);
                                bk = dr.GetDoubleN(24);
                                sd = dr.GetDouble(25);
                                sa = dr.GetDouble(26);

                                if (op == OpType.Traverse)
                                {
                                    point = new TravPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                acc, qlinks, fw, bk, sd, sa);
                                }
                                else
                                {
                                    point = new SideShotPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                acc, qlinks, fw, bk, sd, sa);
                                }
                            }
                            else if (op == OpType.Quondam)
                            {
                                pcn = dr.GetString(27);
                                manacc = dr.GetDoubleN(28);

                                point = new QuondamPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                acc, qlinks, pcn, manacc);

                                if (linked)
                                {
                                    QuondamPoint qp = point as QuondamPoint;
                                    qp.ParentPoint = GetPoint(qp.ParentPointCN);
                                }
                            }

                            points.Add(point);
                        }

                        dr.Close();
                    }

                    conn.Close();
                } 
            }

            return points;
        }
        #endregion


        #region Insert/Update Points
        public bool InsertPoint(TtPoint point)
        {
            return InsertBasePoint(point);
        }

        public int InsertPoints(IEnumerable<TtPoint> points)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPoint point in points)
                        {
                            InsertBasePoint(point, trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return points.Count();
        }

        private bool InsertBasePoint(TtPoint point)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        InsertBasePoint(point, trans);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        private bool InsertBasePoint(TtPoint point, SQLiteTransaction transaction)
        {
            try
            {
                database.Insert(TwoTrailsSchema.PointSchema.TableName, GetBasePointValues(point), transaction);

                InsertBaseData(point, transaction);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void InsertBaseData(TtPoint point, SQLiteTransaction transaction)
        {
            switch (point.OpType)
            {
                case OpType.GPS:
                case OpType.Take5:
                case OpType.Walk:
                case OpType.WayPoint:
                    database.Insert(TwoTrailsSchema.GpsPointSchema.TableName, GetGpsPointValues(point as GpsPoint), transaction);
                    break;
                case OpType.Traverse:
                case OpType.SideShot:
                    database.Insert(TwoTrailsSchema.TravPointSchema.TableName, GetTravPointValues(point as TravPoint), transaction);
                    break;
                case OpType.Quondam:
                    database.Insert(TwoTrailsSchema.QuondamPointSchema.TableName, GetQndmPointValues(point as QuondamPoint), transaction);
                    break;
            }
        }


        public bool UpdatePoint(Tuple<TtPoint, TtPoint> point)
        {
            return UpdateBasePoint(point.Item1, point.Item2);
        }

        public int UpdatePoints(IEnumerable<Tuple<TtPoint, TtPoint>> points)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (Tuple<TtPoint, TtPoint> point in points)
                        {
                            UpdateBasePoint(point.Item1, point.Item2, trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return points.Count();
        }
        

        private bool UpdateBasePoint(TtPoint point, TtPoint oldPoint)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        UpdateBasePoint(point, oldPoint, trans);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        private bool UpdateBasePoint(TtPoint point, TtPoint oldPoint, SQLiteTransaction transaction)
        {
            try
            {
                string where = String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, point.CN);

                database.Update(TwoTrailsSchema.PointSchema.TableName,
                    GetBasePointValues(point),
                    where,
                    transaction);

                if (point.OpType != oldPoint.OpType)
                {
                    if (oldPoint.IsGpsType())
                    {
                        database.Delete(TwoTrailsSchema.GpsPointSchema.TableName, where, transaction);
                    }
                    else if (oldPoint.IsTravType())
                    {
                        database.Delete(TwoTrailsSchema.TravPointSchema.TableName, where, transaction);
                    }
                    else
                    {
                        database.Delete(TwoTrailsSchema.QuondamPointSchema.TableName, where, transaction);
                    }

                    InsertBaseData(point, transaction);
                }
                else
                {
                    switch (point.OpType)
                    {
                        case OpType.GPS:
                        case OpType.Take5:
                        case OpType.Walk:
                        case OpType.WayPoint:
                            database.Update(TwoTrailsSchema.GpsPointSchema.TableName,
                                GetGpsPointValues(point as GpsPoint),
                                where,
                                transaction);
                            break;
                        case OpType.Traverse:
                        case OpType.SideShot:
                            database.Update(TwoTrailsSchema.TravPointSchema.TableName,
                                GetTravPointValues(point as TravPoint),
                                where,
                                transaction);
                            break;
                        case OpType.Quondam:
                            database.Update(TwoTrailsSchema.QuondamPointSchema.TableName,
                                GetQndmPointValues(point as QuondamPoint),
                                where,
                                transaction);
                            break;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }


        public bool ChangePointOp(TtPoint point, TtPoint oldPoint)
        {
            return UpdateBasePoint(point, oldPoint);
        }


        private Dictionary<string, object> GetBasePointValues(TtPoint point)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = point.CN,
                [TwoTrailsSchema.PointSchema.Index] = point.Index,
                [TwoTrailsSchema.PointSchema.ID] = point.PID,
                [TwoTrailsSchema.PointSchema.PolyName] = point.Polygon.Name,
                [TwoTrailsSchema.PointSchema.PolyCN] = point.PolygonCN,
                [TwoTrailsSchema.PointSchema.GroupName] = point.Group.Name,
                [TwoTrailsSchema.PointSchema.GroupCN] = point.GroupCN,
                [TwoTrailsSchema.PointSchema.OnBoundary] = point.OnBoundary,
                [TwoTrailsSchema.PointSchema.Comment] = point.Comment,
                [TwoTrailsSchema.PointSchema.Operation] = (int)point.OpType,
                [TwoTrailsSchema.PointSchema.MetadataCN] = point.MetadataCN,
                [TwoTrailsSchema.PointSchema.CreationTime] = point.TimeCreated.ToString(DATE_FORMAT),
                [TwoTrailsSchema.PointSchema.AdjX] = point.AdjX,
                [TwoTrailsSchema.PointSchema.AdjY] = point.AdjY,
                [TwoTrailsSchema.PointSchema.AdjZ] = point.AdjZ,
                [TwoTrailsSchema.PointSchema.UnAdjX] = point.UnAdjX,
                [TwoTrailsSchema.PointSchema.UnAdjY] = point.UnAdjY,
                [TwoTrailsSchema.PointSchema.UnAdjZ] = point.UnAdjZ,
                [TwoTrailsSchema.PointSchema.Accuracy] = point.Accuracy,
                [TwoTrailsSchema.PointSchema.QuondamLinks] = point.LinkedPoints.ToStringContents("_")
            };
        }

        private Dictionary<string, object> GetGpsPointValues(GpsPoint point)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = point.CN,
                [TwoTrailsSchema.GpsPointSchema.Latitude] = point.Latitude,
                [TwoTrailsSchema.GpsPointSchema.Longitude] = point.Longitude,
                [TwoTrailsSchema.GpsPointSchema.Elevation] = point.Elevation,
                [TwoTrailsSchema.GpsPointSchema.ManualAccuracy] = point.ManualAccuracy
            };
        }

        private Dictionary<string, object> GetTravPointValues(TravPoint point)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = point.CN,
                [TwoTrailsSchema.TravPointSchema.ForwardAz] = point.FwdAzimuth,
                [TwoTrailsSchema.TravPointSchema.BackAz] = point.BkAzimuth,
                [TwoTrailsSchema.TravPointSchema.SlopeDistance] = point.SlopeDistance,
                [TwoTrailsSchema.TravPointSchema.SlopeAngle] = point.SlopeAngle,
                [TwoTrailsSchema.TravPointSchema.HorizDistance] = point.HorizontalDistance
            };
        }

        private Dictionary<string, object> GetQndmPointValues(QuondamPoint point)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = point.CN,
                [TwoTrailsSchema.QuondamPointSchema.ParentPointCN] = point.ParentPointCN,
                [TwoTrailsSchema.QuondamPointSchema.ManualAccuracy] = point.ManualAccuracy
            };
        }
        #endregion


        #region Delete Points
        public bool DeletePoint(TtPoint point)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        String where = String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, point.CN);
                        if (DeleteBasePoints(where, trans))
                        {
                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, trans);
                                    break;
                                case OpType.Traverse:
                                case OpType.SideShot:
                                    DeleteTravPointData(where, trans);
                                    break;
                                case OpType.Quondam:
                                    DeleteQndmPointData(where, trans);
                                    break;
                            }

                            trans.Commit();
                            return true;
                        }
                        else
                            trans.Rollback();
                    }
                    catch
                    {
                        trans.Rollback();
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return false;
        }

        public int DeletePoints(IEnumerable<TtPoint> points)
        {
            StringBuilder sb = new StringBuilder();
            int total = points.Count();
            int count = 0;

            String where = String.Empty;

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {

                        foreach (TtPoint point in points)
                        {
                            where = String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, point.CN);

                            if (count % 50 == 0)
                            {
                                sb.Append(where);

                                if (!DeleteBasePoints(sb.ToString(), trans))
                                {
                                    trans.Rollback();
                                    return -1;
                                }
                            }
                            else
                            {
                                count++;
                                sb.AppendFormat("{0}{1}", where, count < total ? " or " : "");
                            }

                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, trans);
                                    break;
                                case OpType.Traverse:
                                case OpType.SideShot:
                                    DeleteTravPointData(where, trans);
                                    break;
                                case OpType.Quondam:
                                    DeleteQndmPointData(where, trans);
                                    break;
                            }
                        }

                        where = sb.ToString();

                        if (!String.IsNullOrEmpty(where))
                            DeleteBasePoints(where, trans);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return total;
        } 


        private bool DeleteBasePoints(String where, SQLiteTransaction transaction)
        {
            return database.Delete(TwoTrailsSchema.PointSchema.TableName, where, transaction) > 0;
        }

        private void DeleteGpsPointData(String where, SQLiteTransaction transaction)
        {
            database.Delete(TwoTrailsSchema.GpsPointSchema.TableName, where, transaction);
        }

        private void DeleteTravPointData(String where, SQLiteTransaction transaction)
        {
            database.Delete(TwoTrailsSchema.TravPointSchema.TableName, where, transaction);
        }

        private void DeleteQndmPointData(String where, SQLiteTransaction transaction)
        {
            database.Delete(TwoTrailsSchema.QuondamPointSchema.TableName, where, transaction);
        }
        #endregion
        #endregion


        #region Polygons
        #region Get Polygons
        public bool HasPolygons()
        {
            throw new NotImplementedException();
        }

        public List<TtPolygon> GetPolygons()
        {
            List<TtPolygon> polys = new List<TtPolygon>();

            String query = String.Format(@"select {0} from {1}",
                TwoTrailsSchema.PolygonSchema.SelectItems,
                TwoTrailsSchema.PolygonSchema.TableName
            );

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        string name, desc, cn;
                        int psi, inc;
                        DateTime time;
                        double acc, area, perim;

                        while (dr.Read())
                        {
                            cn = dr.GetString(0);
                            name = dr.GetString(1);
                            desc = dr.GetString(2);
                            acc = dr.GetDouble(3);
                            inc = dr.GetInt32(4);
                            psi = dr.GetInt32(5);
                            time = DateTime.Parse(dr.GetString(6), CultureInfo.InvariantCulture);
                            //time = DateTime.ParseExact(dr.GetString(6), DATE_FORMAT, CultureInfo.InvariantCulture);
                            area = dr.GetDouble(7);
                            perim = dr.GetDouble(8);

                            polys.Add(new TtPolygon(cn, name, desc, psi, inc,
                                time, acc, area, perim));
                        }

                        dr.Close();
                    }

                    conn.Close();
                } 
            }

            return polys;
        } 
        #endregion


        #region Insert/Update Polygons
        public bool InsertPolygon(TtPolygon polygon)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Insert(TwoTrailsSchema.PolygonSchema.TableName, GetPolygonValues(polygon), trans);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int InsertPolygons(IEnumerable<TtPolygon> polygons)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPolygon poly in polygons)
                        {
                            database.Insert(TwoTrailsSchema.PolygonSchema.TableName, GetPolygonValues(poly), trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return polygons.Count();
        }


        public bool UpdatePolygon(TtPolygon polygon)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Update(TwoTrailsSchema.PolygonSchema.TableName,
                            GetPolygonValues(polygon),
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, polygon.CN),
                            trans);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int UpdatePolygons(IEnumerable<TtPolygon> polygons)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPolygon poly in polygons)
                        {
                            database.Update(TwoTrailsSchema.PolygonSchema.TableName,
                                GetPolygonValues(poly),
                                String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, poly.CN),
                                trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                } 
            }

            return polygons.Count();
        }


        private Dictionary<string, object> GetPolygonValues(TtPolygon poly)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = poly.CN,
                [TwoTrailsSchema.PolygonSchema.Name] = poly.Name,
                [TwoTrailsSchema.PolygonSchema.Description] = poly.Description,
                [TwoTrailsSchema.PolygonSchema.Accuracy] = poly.Accuracy,
                [TwoTrailsSchema.PolygonSchema.IncrementBy] = poly.Increment,
                [TwoTrailsSchema.PolygonSchema.PointStartIndex] = poly.PointStartIndex,
                [TwoTrailsSchema.PolygonSchema.TimeCreated] = poly.TimeCreated,
                [TwoTrailsSchema.PolygonSchema.Area] = poly.Area,
                [TwoTrailsSchema.PolygonSchema.Perimeter] = poly.Perimeter
            };
        }
        #endregion


        #region Delete Polygons
        public bool DeletePolygon(TtPolygon polygon)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Delete(TwoTrailsSchema.PolygonSchema.TableName,
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, polygon.CN),
                            trans);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int DeletePolygons(IEnumerable<TtPolygon> polygons)
        {
            StringBuilder sb = new StringBuilder();
            int total = polygons.Count();
            int count = 0;

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {

                        foreach (TtPolygon poly in polygons)
                        {
                            count++;
                            sb.AppendFormat("{0}{1}",
                                String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, poly.CN),
                                count < total ? " or " : "");
                        }

                        string where = sb.ToString();

                        if (!String.IsNullOrEmpty(where) &&
                            database.Delete(TwoTrailsSchema.PolygonSchema.TableName,
                            where, transaction) < 0)
                        {
                            transaction.Rollback();
                            return -1;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return total;
        }
        #endregion
        #endregion


        #region Metadata
        #region Get Metadata
        public List<TtMetadata> GetMetadata()
        {
            List<TtMetadata> metas = new List<TtMetadata>();

            String query = String.Format(@"select {0} from {1}",
                TwoTrailsSchema.MetadataSchema.SelectItems,
                TwoTrailsSchema.MetadataSchema.TableName
            );

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        string cn, name, cmt, gps, rf, compass, crew;
                        int zone;
                        DeclinationType decType;
                        double magdec;
                        Datum datum;
                        Distance dist, elev;
                        Slope slope;

                        while (dr.Read())
                        {
                            cn = dr.GetString(0);
                            name = dr.GetString(1);
                            cmt = dr.GetStringN(2);
                            zone = dr.GetInt32(3);
                            datum = (Datum)dr.GetInt32(4);
                            dist = (Distance)dr.GetInt32(5);
                            elev = (Distance)dr.GetInt32(6);
                            slope = (Slope)dr.GetInt32(7);
                            decType = (DeclinationType)dr.GetInt32(8);
                            magdec = dr.GetDouble(9);
                            gps = dr.GetStringN(10);
                            rf = dr.GetStringN(11);
                            compass = dr.GetStringN(12);
                            crew = dr.GetStringN(13);

                            metas.Add(new TtMetadata(cn, name, cmt, zone,
                                decType, magdec, datum, dist,
                                elev, slope, gps, rf, compass, crew));
                        }

                        dr.Close();
                    }

                    conn.Close();
                } 
            }

            return metas;
        }
        #endregion


        #region Insert/Update Metadata
        public bool InsertMetadata(TtMetadata metadata)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Insert(TwoTrailsSchema.MetadataSchema.TableName, GetMetadataValues(metadata), trans);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int InsertMetadata(IEnumerable<TtMetadata> metadata)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtMetadata meta in metadata)
                        {
                            database.Insert(TwoTrailsSchema.MetadataSchema.TableName, GetMetadataValues(meta), trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return metadata.Count();
        }


        public bool UpdateMetadata(TtMetadata metadata)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Update(TwoTrailsSchema.MetadataSchema.TableName,
                            GetMetadataValues(metadata),
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, metadata.CN),
                            trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int UpdateMetadata(IEnumerable<TtMetadata> metadata)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtMetadata meta in metadata)
                        {
                            database.Update(TwoTrailsSchema.MetadataSchema.TableName,
                                GetMetadataValues(meta),
                                String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, meta.CN),
                                trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                } 
            }

            return metadata.Count();
        }


        private Dictionary<string, object> GetMetadataValues(TtMetadata meta)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = meta.CN,
                [TwoTrailsSchema.MetadataSchema.Name] = meta.Name,
                [TwoTrailsSchema.MetadataSchema.Comment] = meta.Comment,
                [TwoTrailsSchema.MetadataSchema.UtmZone] = meta.Zone,
                [TwoTrailsSchema.MetadataSchema.Datum] = (int)meta.Datum,
                [TwoTrailsSchema.MetadataSchema.Distance] = (int)meta.Distance,
                [TwoTrailsSchema.MetadataSchema.Elevation] = (int)meta.Elevation,
                [TwoTrailsSchema.MetadataSchema.Slope] = (int)meta.Slope,
                [TwoTrailsSchema.MetadataSchema.DeclinationType] = (int)meta.DecType,
                [TwoTrailsSchema.MetadataSchema.MagDec] = meta.MagDec,
                [TwoTrailsSchema.MetadataSchema.GpsReceiver] = meta.GpsReceiver,
                [TwoTrailsSchema.MetadataSchema.RangeFinder] = meta.RangeFinder,
                [TwoTrailsSchema.MetadataSchema.Compass] = meta.Compass,
                [TwoTrailsSchema.MetadataSchema.Crew] = meta.Crew
            };
        }
        #endregion


        #region Delete Metadata
        public bool DeleteMetadata(TtMetadata metadata)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Delete(TwoTrailsSchema.MetadataSchema.TableName,
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, metadata.CN),
                            trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int DeleteMetadata(IEnumerable<TtMetadata> metadata)
        {
            StringBuilder sb = new StringBuilder();
            int total = metadata.Count();
            int count = 0;

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtMetadata meta in metadata)
                        {
                            count++;
                            sb.AppendFormat("{0}{1}",
                                String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, meta.CN),
                                count < total ? " or " : "");
                        }

                        string where = sb.ToString();

                        if (!String.IsNullOrEmpty(where) &&
                            database.Delete(TwoTrailsSchema.MetadataSchema.TableName,
                            where, trans) < 0)
                        {
                            trans.Rollback();
                            return -1;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return total;
        }
        #endregion
        #endregion


        #region Groups
        #region Get Groups
        public List<TtGroup> GetGroups()
        {
            List<TtGroup> groups = new List<TtGroup>();

            String query = String.Format(@"select {0} from {1}",
                TwoTrailsSchema.GroupSchema.SelectItems,
                TwoTrailsSchema.GroupSchema.TableName
            );

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        string name, desc, cn;
                        GroupType gt;

                        while (dr.Read())
                        {
                            cn = dr.GetString(0);
                            name = dr.GetString(1);
                            desc = dr.GetString(2);
                            gt = (GroupType)dr.GetInt32(3);

                            groups.Add(new TtGroup(cn, name, desc, gt));
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return groups;
        }
        #endregion


        #region Insert/Update Groups
        public bool InsertGroup(TtGroup group)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Insert(TwoTrailsSchema.GroupSchema.TableName, GetGroupValues(group), trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int InsertGroups(IEnumerable<TtGroup> groups)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtGroup group in groups)
                        {
                            database.Insert(TwoTrailsSchema.GroupSchema.TableName, GetGroupValues(group), trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return groups.Count();
        }


        public bool UpdateGroup(TtGroup group)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Update(TwoTrailsSchema.GroupSchema.TableName,
                            GetGroupValues(group),
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, group.CN),
                            trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int UpdateGroups(IEnumerable<TtGroup> groups)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtGroup group in groups)
                        {
                            database.Update(TwoTrailsSchema.GroupSchema.TableName,
                                GetGroupValues(group),
                                String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, group.CN),
                                trans);
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                } 
            }

            return groups.Count();
        }


        private Dictionary<string, object> GetGroupValues(TtGroup group)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = group.CN,
                [TwoTrailsSchema.GroupSchema.Name] = group.Name,
                [TwoTrailsSchema.GroupSchema.Description] = group.Description,
                [TwoTrailsSchema.GroupSchema.Type] = (int)group.GroupType
            };
        }
        #endregion


        #region Delete Groups
        public bool DeleteGroup(TtGroup group)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Delete(TwoTrailsSchema.GroupSchema.TableName,
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, group.CN),
                            trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public int DeleteGroups(IEnumerable<TtGroup> groups)
        {
            StringBuilder sb = new StringBuilder();
            int total = groups.Count();
            int count = 0;

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {

                        foreach (TtGroup group in groups)
                        {
                            count++;
                            sb.AppendFormat("{0}{1}",
                                String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, group.CN),
                                count < total ? " or " : "");
                        }

                        string where = sb.ToString();

                        if (!String.IsNullOrEmpty(where) &&
                            database.Delete(TwoTrailsSchema.GroupSchema.TableName,
                            where, trans) < 0)
                        {
                            trans.Rollback();
                            return -1;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return total;
        }
        #endregion
        #endregion


        #region TTNmea
        public List<TtNmeaBurst> GetNmeaBursts(string pointCN = null)
        {
            throw new NotImplementedException();
        }

        public bool InsertNmeaBurst(TtNmeaBurst burst)
        {
            throw new NotImplementedException();
        }

        public bool InsertNmeaBursts(IEnumerable<TtNmeaBurst> bursts)
        {
            throw new NotImplementedException();
        }

        public bool UpdateNmeaBurst(TtNmeaBurst burst)
        {
            throw new NotImplementedException();
        }

        public int UpdateNmeaBursts(IEnumerable<TtNmeaBurst> bursts)
        {
            throw new NotImplementedException();
        }

        public void DeleteNmeaBursts(string pointCN)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Project
        public TtProjectInfo GetProjectInfo()
        {
            String query = String.Format(@"select {0} from {1} limit 1",
                   TwoTrailsSchema.ProjectInfoSchema.SelectItems,
                   TwoTrailsSchema.ProjectInfoSchema.TableName
               );

            TtProjectInfo info = null;

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            string name, desc, region, forest, district, deviceID, version, creationVersion;
                            Version dbVersion;
                            DateTime date;

                            name = dr.GetString(0);
                            district = dr.GetString(1);
                            forest = dr.GetString(2);
                            region = dr.GetString(3);
                            deviceID = dr.GetString(4);
                            date = DateTime.Parse(dr.GetString(5));
                            //date = DateTime.ParseExact(dr.GetString(5), DATE_FORMAT, CultureInfo.InvariantCulture);
                            desc = dr.GetString(6);
                            dbVersion = new Version(dr.GetString(7));
                            version = dr.GetString(8);
                            creationVersion = dr.GetString(9);

                            info = new TtProjectInfo(name, desc, region, forest, district,
                                version, creationVersion, dbVersion, deviceID, date);
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return info;
        }

        public Version GetDataVersion()
        {
            String query = String.Format(@"select {0} from {1} limit 1",
                   TwoTrailsSchema.ProjectInfoSchema.TtDbSchemaVersion,
                   TwoTrailsSchema.ProjectInfoSchema.TableName
               );

            Version version = null;

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            version = new Version(dr.GetString(0));
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return version;
        }

        public bool InsertProjectInfo(TtProjectInfo info)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Insert(TwoTrailsSchema.ProjectInfoSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TwoTrailsSchema.ProjectInfoSchema.Name] = info.Name,
                                [TwoTrailsSchema.ProjectInfoSchema.District] = info.District,
                                [TwoTrailsSchema.ProjectInfoSchema.Forest] = info.Forest,
                                [TwoTrailsSchema.ProjectInfoSchema.Region] = info.Region,
                                [TwoTrailsSchema.ProjectInfoSchema.DeviceID] = info.CreationDeviceID,
                                [TwoTrailsSchema.ProjectInfoSchema.Created] = info.CreationDate,
                                [TwoTrailsSchema.ProjectInfoSchema.Description] = info.Description,
                                [TwoTrailsSchema.ProjectInfoSchema.TtDbSchemaVersion] = info.DbVersion.ToString(),
                                [TwoTrailsSchema.ProjectInfoSchema.TtVersion] = info.Version.ToString(),
                                [TwoTrailsSchema.ProjectInfoSchema.CreatedTtVersion] = info.CreationVersion.ToString()
                            }, trans);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }

        public bool UpdateProjectInfo(TtProjectInfo info)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Insert(TwoTrailsSchema.ProjectInfoSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TwoTrailsSchema.ProjectInfoSchema.Name] = info.Name,
                                [TwoTrailsSchema.ProjectInfoSchema.District] = info.District,
                                [TwoTrailsSchema.ProjectInfoSchema.Forest] = info.Forest,
                                [TwoTrailsSchema.ProjectInfoSchema.Region] = info.Region,
                                [TwoTrailsSchema.ProjectInfoSchema.DeviceID] = info.CreationDeviceID,
                                [TwoTrailsSchema.ProjectInfoSchema.Description] = info.Description,
                                [TwoTrailsSchema.ProjectInfoSchema.TtVersion] = info.Version.ToString()
                            },
                            trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                } 
            }

            return true;
        }
        #endregion


        #region TtMedia
        public List<TtImage> GetPictures(string pointCN)
        {
            List<TtImage> images = new List<TtImage>();

            String query = String.Format(@"select {1}.{0}, {2} from {1} left join {3} on {1}.{4} = {3}.{4}",
                TwoTrailsSchema.MediaSchema.SelectItems,
                TwoTrailsSchema.MediaSchema.TableName,
                TwoTrailsSchema.PictureSchema.SelectItemsNoCN,
                TwoTrailsSchema.PictureSchema.TableName,
                TwoTrailsSchema.SharedSchema.CN
            );

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        string cn, pcn, name, file, cmt;
                        MediaType mt;
                        PictureType pt;
                        DateTime date;
                        float? az, pitch, roll;

                        TtImage image = null;

                        while (dr.Read())
                        {
                            cn = dr.GetString(0);
                            pcn = dr.GetString(1);
                            mt = (MediaType)dr.GetInt32(2);
                            name = dr.GetString(3);
                            file = dr.GetStringN(4);
                            date = DateTime.ParseExact(dr.GetString(5), DATE_FORMAT, CultureInfo.InvariantCulture);
                            cmt = dr.GetStringN(6);

                            pt = (PictureType)dr.GetInt32(7);
                            az = dr.GetFloatN(8);
                            pitch = dr.GetFloatN(9);
                            roll = dr.GetFloatN(10);

                            switch (pt)
                            {
                                case PictureType.Regular:
                                    image = new TtImage(cn, name, file, cmt, date, pcn, az, pitch, roll);
                                    break;
                                case PictureType.Panorama:
                                    image = new TtPanorama(cn, name, file, cmt, date, pcn, az, pitch, roll);
                                    break;
                                case PictureType.PhotoSphere:
                                    image = new TtPhotoShpere(cn, name, file, cmt, date, pcn, az, pitch, roll);
                                    break;
                                default:
                                    throw new Exception("Unknown Image Type");
                            }

                            images.Add(image);
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return images;
        }

        public bool InsertMedia(TtMedia media)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        if (database.Insert(TwoTrailsSchema.MediaSchema.TableName, GetMediaValues(media), trans) > 0)
                        {
                            switch (media.MediaType)
                            {
                                case MediaType.Picture:
                                    database.Insert(TwoTrailsSchema.PictureSchema.TableName, GetImageValues(media as TtImage), trans);
                                    break;
                                case MediaType.Video:
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return true;
        }

        public bool UpdateMedia(TtMedia media)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Update(TwoTrailsSchema.MediaSchema.TableName,
                            GetMediaValues(media),
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, media.CN),
                            trans);

                        switch (media.MediaType)
                        {
                            case MediaType.Picture:
                                database.Update(TwoTrailsSchema.PictureSchema.TableName,
                                    GetImageValues(media as TtImage),
                                    String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, media.CN),
                                    trans);
                                break;
                            case MediaType.Video:
                                break;
                        }
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return true;
        }


        private Dictionary<string, object> GetMediaValues(TtMedia media)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = media.CN,
                [TwoTrailsSchema.MediaSchema.Name] = media.Name,
                [TwoTrailsSchema.MediaSchema.FilePath] = media.FilePath,
                [TwoTrailsSchema.MediaSchema.Comment] = media.Comment,
                [TwoTrailsSchema.MediaSchema.CreationTime] = media.TimeCreated.ToString(DATE_FORMAT),
                [TwoTrailsSchema.MediaSchema.MediaType] = (int)media.MediaType,
                [TwoTrailsSchema.MediaSchema.PointCN] = media.PointCN
            };
        }

        private Dictionary<string, object> GetImageValues(TtImage image)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = image.CN,
                [TwoTrailsSchema.PictureSchema.PicType] = (int)image.PictureType,
                [TwoTrailsSchema.PictureSchema.Azimuth] = image.Azimuth,
                [TwoTrailsSchema.PictureSchema.Pitch] = image.Pitch,
                [TwoTrailsSchema.PictureSchema.Roll] = image.Roll
            };
        }

        public bool DeleteMedia(TtMedia media)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Delete(TwoTrailsSchema.MediaSchema.TableName,
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, media.CN),
                            trans);

                        if (media.MediaType == MediaType.Picture)
                        {
                            database.Delete(TwoTrailsSchema.PictureSchema.TableName,
                                String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, media.CN),
                                trans);
                        }
                        
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return true;
        }
        #endregion


        #region Polygon Attrs
        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            List<PolygonGraphicOptions> pgos = new List<PolygonGraphicOptions>();

            String query = String.Format(@"select {0} from {1}",
                TwoTrailsSchema.PolygonAttrSchema.SelectItems,
                TwoTrailsSchema.PolygonAttrSchema.TableName
            );

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        string cn;
                        int adjbnd, unadjbnd, adjnav, unadjnav,
                            adjpts, unadjpts, waypts;

                        while (dr.Read())
                        {
                            cn = dr.GetString(0);
                            adjbnd = dr.GetInt32(1);
                            unadjbnd = dr.GetInt32(2);
                            adjnav = dr.GetInt32(2);
                            unadjnav = dr.GetInt32(2);
                            adjpts = dr.GetInt32(2);
                            unadjpts = dr.GetInt32(2);
                            waypts = dr.GetInt32(2);

                            pgos.Add(new PolygonGraphicOptions(cn, adjbnd, unadjbnd, adjnav, unadjnav,
                                adjpts, unadjpts, waypts, 0, 0));
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return pgos;
        }

        public bool InsertPolygonGraphicOption(PolygonGraphicOptions option)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Insert(TwoTrailsSchema.PolygonAttrSchema.TableName, GetGraphicOptionValues(option), trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return true;
        }

        private Dictionary<string, object> GetGraphicOptionValues(PolygonGraphicOptions option)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = option.CN,
                [TwoTrailsSchema.PolygonAttrSchema.AdjBndColor] = option.AdjBndColor,
                [TwoTrailsSchema.PolygonAttrSchema.UnAdjBndColor] = option.UnAdjBndColor,
                [TwoTrailsSchema.PolygonAttrSchema.AdjNavColor] = option.AdjNavColor,
                [TwoTrailsSchema.PolygonAttrSchema.UnAdjNavColor] = option.UnAdjNavColor,
                [TwoTrailsSchema.PolygonAttrSchema.AdjPtsColor] = option.AdjPtsColor,
                [TwoTrailsSchema.PolygonAttrSchema.UnAdjPtsColor] = option.UnAdjPtsColor,
                [TwoTrailsSchema.PolygonAttrSchema.WayPtsColor] = option.WayPtsColor
            };
        }

        public bool DeletePolygonGraphicOption(PolygonGraphicOptions option)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        database.Delete(TwoTrailsSchema.PolygonAttrSchema.TableName,
                            String.Format("{0} = '{1}'", TwoTrailsSchema.SharedSchema.CN, option.CN),
                            trans);
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return true;
        }
        #endregion


        #region Utils
        public bool Duplicate(ITtDataLayer dataLayer)
        {
            throw new NotImplementedException();
        }

        public bool Clean()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
