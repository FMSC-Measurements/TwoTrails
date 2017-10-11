using CSUtil;
using CSUtil.Databases;
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
using FMSC.Core;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.NMEA.Sentences;
using System.Diagnostics;

namespace TwoTrails.DAL
{
    public class TtSqliteDataAccessLayer : ITtDataLayer
    {
        public String FilePath { get; }

        private SQLiteDatabase _Database;

        private DataDictionaryTemplate _DataDictionaryTemplate;


        public TtSqliteDataAccessLayer(String filePath)
        {
            FilePath = filePath;
            _Database = new SQLiteDatabase(FilePath);
        }

        private TtSqliteDataAccessLayer(SQLiteDatabase database)
        {
            this.FilePath = database.FileName;
            this._Database = database;
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
            database.ExecuteNonQuery(TwoTrailsSchema.PolygonAttrSchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.ActivitySchema.CreateTable);
            database.ExecuteNonQuery(TwoTrailsSchema.DataDictionarySchema.CreateTable);

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
            return GetPoints(
                $"{TwoTrailsSchema.PointSchema.TableName}.{TwoTrailsSchema.SharedSchema.CN} = '{pointCN}'",
                1,
                false
            ).First();
        }

        public IEnumerable<TtPoint> GetPoints(string polyCN = null, bool linked = false)
        {
            return GetPoints(
                polyCN != null ?
                    $"{TwoTrailsSchema.PointSchema.PolyCN} = '{polyCN}'" :
                    null,
                0,
                linked
            );
        }
        
        protected IEnumerable<TtPoint> GetPoints(String where, int limit = 0, bool linked = true)
        {
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
                where != null ? $" where {where}" : String.Empty,
                TwoTrailsSchema.PointSchema.Index
            );

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
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
                            time = TtCoreUtils.ParseTime(dr.GetString(9));

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

                            yield return point;
                        }

                        dr.Close();
                    }

                    conn.Close();
                } 
            }
        }
        #endregion


        #region Insert/Update Points
        public bool InsertPoint(TtPoint point)
        {
            return InsertBasePoint(point);
        }

        public int InsertPoints(IEnumerable<TtPoint> points)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPoint point in points)
                        {
                            InsertBasePoint(point, conn, trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertPoints");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        InsertBasePoint(point, conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertBasePoint");
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

        private bool InsertBasePoint(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Insert(TwoTrailsSchema.PointSchema.TableName, GetBasePointValues(point), conn, transaction);

            InsertBaseData(point, conn, transaction);

            return true;
        }

        private void InsertBaseData(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            switch (point.OpType)
            {
                case OpType.GPS:
                case OpType.Take5:
                case OpType.Walk:
                case OpType.WayPoint:
                    _Database.Insert(TwoTrailsSchema.GpsPointSchema.TableName, GetGpsPointValues(point as GpsPoint), conn, transaction);
                    break;
                case OpType.Traverse:
                case OpType.SideShot:
                    _Database.Insert(TwoTrailsSchema.TravPointSchema.TableName, GetTravPointValues(point as TravPoint), conn, transaction);
                    break;
                case OpType.Quondam:
                    _Database.Insert(TwoTrailsSchema.QuondamPointSchema.TableName, GetQndmPointValues(point as QuondamPoint), conn, transaction);
                    break;
            }
        }


        public bool UpdatePoint(Tuple<TtPoint, TtPoint> point)
        {
            return UpdateBasePoint(point.Item1, point.Item2);
        }

        public int UpdatePoints(IEnumerable<Tuple<TtPoint, TtPoint>> points)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (Tuple<TtPoint, TtPoint> point in points)
                        {
                            UpdateBasePoint(point.Item1, point.Item2, conn, trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdatePoints");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        UpdateBasePoint(point, oldPoint, conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateBasePoint");
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

        private bool UpdateBasePoint(TtPoint point, TtPoint oldPoint, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            string where = $"{TwoTrailsSchema.SharedSchema.CN} = '{point.CN}'";

            _Database.Update(TwoTrailsSchema.PointSchema.TableName,
                GetBasePointValues(point),
                where,
                conn,
                transaction);

            if (point.OpType != oldPoint.OpType)
            {
                if (oldPoint.IsGpsType())
                {
                    _Database.Delete(TwoTrailsSchema.GpsPointSchema.TableName, where, conn, transaction);
                }
                else if (oldPoint.IsTravType())
                {
                    _Database.Delete(TwoTrailsSchema.TravPointSchema.TableName, where, conn, transaction);
                }
                else
                {
                    _Database.Delete(TwoTrailsSchema.QuondamPointSchema.TableName, where, conn, transaction);
                }

                InsertBaseData(point, conn, transaction);
            }
            else
            {
                switch (point.OpType)
                {
                    case OpType.GPS:
                    case OpType.Take5:
                    case OpType.Walk:
                    case OpType.WayPoint:
                        _Database.Update(TwoTrailsSchema.GpsPointSchema.TableName,
                            GetGpsPointValues(point as GpsPoint),
                            where,
                            conn,
                            transaction);
                        break;
                    case OpType.Traverse:
                    case OpType.SideShot:
                        _Database.Update(TwoTrailsSchema.TravPointSchema.TableName,
                            GetTravPointValues(point as TravPoint),
                            where,
                            conn,
                            transaction);
                        break;
                    case OpType.Quondam:
                        _Database.Update(TwoTrailsSchema.QuondamPointSchema.TableName,
                            GetQndmPointValues(point as QuondamPoint),
                            where,
                            conn,
                            transaction);
                        break;
                }
            }

            return true;
        }


        public bool ChangePointOp(TtPoint point, TtPoint oldPoint)
        {
            return UpdateBasePoint(point, oldPoint);
        }


        private Dictionary<string, object> GetBasePointValues(TtPoint point)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = point.CN,
                [TwoTrailsSchema.PointSchema.Index] = point.Index,
                [TwoTrailsSchema.PointSchema.ID] = point.PID,
                [TwoTrailsSchema.PointSchema.PolyCN] = point.PolygonCN,
                [TwoTrailsSchema.PointSchema.GroupCN] = point.GroupCN,
                [TwoTrailsSchema.PointSchema.OnBoundary] = point.OnBoundary,
                [TwoTrailsSchema.PointSchema.Comment] = point.Comment,
                [TwoTrailsSchema.PointSchema.Operation] = (int)point.OpType,
                [TwoTrailsSchema.PointSchema.MetadataCN] = point.MetadataCN,
                [TwoTrailsSchema.PointSchema.CreationTime] = point.TimeCreated.ToString(Consts.DATE_FORMAT),
                [TwoTrailsSchema.PointSchema.Accuracy] = point.Accuracy,
                [TwoTrailsSchema.PointSchema.QuondamLinks] = point.LinkedPoints.ToStringContents("_")
            };

            if (point.OpType != OpType.Quondam || (point as QuondamPoint).ParentPoint != null)
            {
                dic.Add(TwoTrailsSchema.PointSchema.AdjX, point.AdjX);
                dic.Add(TwoTrailsSchema.PointSchema.AdjY, point.AdjY);
                dic.Add(TwoTrailsSchema.PointSchema.AdjZ, point.AdjZ);
                dic.Add(TwoTrailsSchema.PointSchema.UnAdjX, point.UnAdjX);
                dic.Add(TwoTrailsSchema.PointSchema.UnAdjY, point.UnAdjY);
                dic.Add(TwoTrailsSchema.PointSchema.UnAdjZ, point.UnAdjZ);
            }
            else
            {
                dic.Add(TwoTrailsSchema.PointSchema.AdjX, 0);
                dic.Add(TwoTrailsSchema.PointSchema.AdjY, 0);
                dic.Add(TwoTrailsSchema.PointSchema.AdjZ, 0);
                dic.Add(TwoTrailsSchema.PointSchema.UnAdjX, 0);
                dic.Add(TwoTrailsSchema.PointSchema.UnAdjY, 0);
                dic.Add(TwoTrailsSchema.PointSchema.UnAdjZ, 0);
            }

            if (point.Polygon != null)
                dic.Add(TwoTrailsSchema.PointSchema.PolyName, point.Polygon.Name);

            if (point.Group != null)
                dic.Add(TwoTrailsSchema.PointSchema.GroupName, point.Group.Name);

            return dic;
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        String where = $"{TwoTrailsSchema.SharedSchema.CN} = '{point.CN}'";
                        if (DeleteBasePoints(where, conn, trans))
                        {
                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, conn, trans);
                                    break;
                                case OpType.Traverse:
                                case OpType.SideShot:
                                    DeleteTravPointData(where, conn, trans);
                                    break;
                                case OpType.Quondam:
                                    DeleteQndmPointData(where, conn, trans);
                                    break;
                            }

                            trans.Commit();
                            return true;
                        }
                        else
                            trans.Rollback();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeletePoint");
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

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {

                        foreach (TtPoint point in points)
                        {
                            count++;
                            where = $"{TwoTrailsSchema.SharedSchema.CN} = '{point.CN}'";

                            if (count % 50 == 0)
                            {
                                sb.Append(where);

                                if (!DeleteBasePoints(sb.ToString(), conn, trans))
                                {
                                    trans.Rollback();
                                    return -1;
                                }

                                count = 0;
                            }
                            else
                            {
                                sb.Append($"{where}{(count < total ? " or " : String.Empty)}");
                            }

                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, conn, trans);
                                    break;
                                case OpType.Traverse:
                                case OpType.SideShot:
                                    DeleteTravPointData(where, conn, trans);
                                    break;
                                case OpType.Quondam:
                                    DeleteQndmPointData(where, conn, trans);
                                    break;
                            }
                        }

                        where = sb.ToString();

                        if (!String.IsNullOrEmpty(where))
                            DeleteBasePoints(where, conn, trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeletePoints");
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


        private bool DeleteBasePoints(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            return _Database.Delete(TwoTrailsSchema.PointSchema.TableName, where, conn, transaction) > 0;
        }

        private void DeleteGpsPointData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TwoTrailsSchema.GpsPointSchema.TableName, where, conn, transaction);
        }

        private void DeleteTravPointData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TwoTrailsSchema.TravPointSchema.TableName, where, conn, transaction);
        }

        private void DeleteQndmPointData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TwoTrailsSchema.QuondamPointSchema.TableName, where, conn, transaction);
        }
        #endregion
        #endregion


        #region Polygons
        #region Get Polygons
        public bool HasPolygons()
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader($"select count(*) from {TwoTrailsSchema.PolygonSchema.TableName}", conn))
                {
                    if (dr != null)
                    {
                        if (dr.Read())
                        {
                            return dr.GetInt32(0) > 0;
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            throw new Exception("Unable to get polygon count.");
        }

        public IEnumerable<TtPolygon> GetPolygons()
        {
            String query = $"select {TwoTrailsSchema.PolygonSchema.SelectItems} from {TwoTrailsSchema.PolygonSchema.TableName}";

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            yield return new TtPolygon(
                                dr.GetString(0),
                                dr.GetString(1),
                                dr.GetString(2),
                                dr.GetInt32(5),
                                dr.GetInt32(4),
                                TtCoreUtils.ParseTime(dr.GetString(6)),
                                dr.GetDouble(3),
                                dr.GetDouble(7),
                                dr.GetDouble(8),
                                dr.GetDoubleN(9)??0);
                        }

                        dr.Close();
                    }

                    conn.Close();
                } 
            }
        } 
        #endregion


        #region Insert/Update Polygons
        public bool InsertPolygon(TtPolygon polygon)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TwoTrailsSchema.PolygonSchema.TableName, GetPolygonValues(polygon), conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertPolygon");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPolygon poly in polygons)
                        {
                            _Database.Insert(TwoTrailsSchema.PolygonSchema.TableName, GetPolygonValues(poly), conn, trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertPolygons");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Update(TwoTrailsSchema.PolygonSchema.TableName,
                            GetPolygonValues(polygon),
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{polygon.CN}'",
                            conn,
                            trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdatePolygon");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPolygon poly in polygons)
                        {
                            _Database.Update(TwoTrailsSchema.PolygonSchema.TableName,
                                GetPolygonValues(poly),
                                $"{TwoTrailsSchema.SharedSchema.CN} = '{poly.CN}'",
                                conn,
                                trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdatePolygons");
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
                [TwoTrailsSchema.PolygonSchema.TimeCreated] = poly.TimeCreated.ToString(Consts.DATE_FORMAT),
                [TwoTrailsSchema.PolygonSchema.Area] = poly.Area,
                [TwoTrailsSchema.PolygonSchema.Perimeter] = poly.Perimeter,
                [TwoTrailsSchema.PolygonSchema.PerimeterLine] = poly.PerimeterLine
            };
        }
        #endregion


        #region Delete Polygons
        public bool DeletePolygon(TtPolygon polygon)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Delete(TwoTrailsSchema.PolygonSchema.TableName,
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{polygon.CN}'",
                            conn,
                            trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeletePolygon");
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

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPolygon poly in polygons)
                        {
                            count++;
                            sb.Append($"{TwoTrailsSchema.SharedSchema.CN} = '{poly.CN}'{(count < total ? " or " : String.Empty)}");
                        }

                        string where = sb.ToString();
                        
                        if (!String.IsNullOrEmpty(where) &&
                            _Database.Delete(TwoTrailsSchema.PolygonSchema.TableName,
                            where, conn, transaction) < 0)
                        {
                            transaction.Rollback();
                            return -1;
                        }
                        else
                            transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeletePolygons");
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
        public IEnumerable<TtMetadata> GetMetadata()
        {
            String query = $@"select {TwoTrailsSchema.MetadataSchema.SelectItems} from {TwoTrailsSchema.MetadataSchema.TableName}";

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
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

                            yield return new TtMetadata(cn, name, cmt, zone,
                                decType, magdec, datum, dist,
                                elev, slope, gps, rf, compass, crew);
                        }

                        dr.Close();
                    }

                    conn.Close();
                } 
            }
        }
        #endregion


        #region Insert/Update Metadata
        public bool InsertMetadata(TtMetadata metadata)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TwoTrailsSchema.MetadataSchema.TableName, GetMetadataValues(metadata), conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertMetadata");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtMetadata meta in metadata)
                        {
                            _Database.Insert(TwoTrailsSchema.MetadataSchema.TableName, GetMetadataValues(meta), conn, trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertMetadata");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Update(TwoTrailsSchema.MetadataSchema.TableName,
                            GetMetadataValues(metadata),
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{metadata.CN}'",
                            conn,
                            trans);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateMetadata");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtMetadata meta in metadata)
                        {
                            _Database.Update(TwoTrailsSchema.MetadataSchema.TableName,
                                GetMetadataValues(meta),
                                $"{TwoTrailsSchema.SharedSchema.CN} = '{meta.CN}'",
                                conn,
                                trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateMetadata");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Delete(TwoTrailsSchema.MetadataSchema.TableName,
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{metadata.CN}'",
                            conn,
                            trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeleteMetadata");
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

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtMetadata meta in metadata)
                        {
                            count++;
                            sb.Append($"{TwoTrailsSchema.SharedSchema.CN} = '{meta.CN}' {(count < total ? " or " : String.Empty)}");
                        }

                        string where = sb.ToString();

                        if (!String.IsNullOrEmpty(where) &&
                            _Database.Delete(TwoTrailsSchema.MetadataSchema.TableName,
                            where, conn, trans) < 0)
                        {
                            trans.Rollback();
                            return -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeleteMetadata");
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
        public IEnumerable<TtGroup> GetGroups()
        {
            String query = $"select {TwoTrailsSchema.GroupSchema.SelectItems} from {TwoTrailsSchema.GroupSchema.TableName}";

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
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

                            yield return new TtGroup(cn, name, desc, gt);
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }
        }
        #endregion


        #region Insert/Update Groups
        public bool InsertGroup(TtGroup group)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TwoTrailsSchema.GroupSchema.TableName, GetGroupValues(group), conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertGroup");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtGroup group in groups)
                        {
                            _Database.Insert(TwoTrailsSchema.GroupSchema.TableName, GetGroupValues(group), conn, trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertGroups");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Update(TwoTrailsSchema.GroupSchema.TableName,
                            GetGroupValues(group),
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{group.CN}'",
                            conn,
                            trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateGroup");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtGroup group in groups)
                        {
                            _Database.Update(TwoTrailsSchema.GroupSchema.TableName,
                                GetGroupValues(group),
                                $"{TwoTrailsSchema.SharedSchema.CN} = '{group.CN}'",
                                conn,
                                trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateGroups");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Delete(TwoTrailsSchema.GroupSchema.TableName,
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{group.CN}'",
                            conn,
                            trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeleteGroup");
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

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {

                        foreach (TtGroup group in groups)
                        {
                            count++;
                            sb.Append($"{TwoTrailsSchema.SharedSchema.CN} = '{group.CN}' {(count < total ? " or " : String.Empty)}");
                        }

                        string where = sb.ToString();

                        if (!String.IsNullOrEmpty(where) &&
                            _Database.Delete(TwoTrailsSchema.GroupSchema.TableName,
                            where, conn, trans) < 0)
                        {
                            trans.Rollback();
                            return -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeleteGroups");
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
        public IEnumerable<TtNmeaBurst> GetNmeaBursts(string pointCN = null)
        {
            String query = $@"select {TwoTrailsSchema.TtNmeaSchema.SelectItems} from {TwoTrailsSchema.TtNmeaSchema.TableName}
{(pointCN != null ? $" where {TwoTrailsSchema.TtNmeaSchema.PointCN} = '{pointCN}'" : String.Empty)}"; ;

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        Func<string, List<int>> ParseIds = s =>
                        {
                            List<int> ids = new List<int>();
                            if (s != null)
                            {
                                foreach (string prn in s.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    ids.Add(Int32.Parse(prn));
                                }
                            }

                            return ids;
                        };

                        while (dr.Read())
                        {
                            yield return new TtNmeaBurst(
                                dr.GetString(0),
                                TtCoreUtils.ParseTime(dr.GetString(3)),
                                dr.GetString(1),
                                dr.GetBoolean(2),
                                new GeoPosition(
                                    dr.GetDouble(5), (NorthSouth)dr.GetInt32(6),
                                    dr.GetDouble(7), (EastWest)dr.GetInt32(8),
                                    dr.GetDouble(9), (UomElevation)dr.GetInt32(10)
                                ),
                                TtCoreUtils.ParseTime(dr.GetString(4)),
                                dr.GetDouble(22),
                                dr.GetDouble(23),
                                dr.GetDouble(11), (EastWest)dr.GetInt32(12),
                                (Mode)dr.GetInt32(15), (Fix)dr.GetInt32(13),
                                ParseIds(dr.GetStringN(27)),
                                dr.GetDouble(16),
                                dr.GetDouble(17),
                                dr.GetDouble(18),
                                (GpsFixType)dr.GetInt32(14),
                                dr.GetInt32(25),
                                dr.GetDouble(19),
                                dr.GetDouble(20), (UomElevation)dr.GetInt32(21),
                                dr.GetInt32(26)
                            );
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
        }

        public bool InsertNmeaBurst(TtNmeaBurst burst)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TwoTrailsSchema.TtNmeaSchema.TableName, GetNmeaValues(burst), conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertNmeaBurst");
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

        public int InsertNmeaBursts(IEnumerable<TtNmeaBurst> bursts)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtNmeaBurst burst in bursts)
                        {
                            _Database.Insert(TwoTrailsSchema.TtNmeaSchema.TableName, GetNmeaValues(burst), conn, trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertNmeaBursts");
                        trans.Rollback();
                        return -1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return bursts.Count();
        }

        public bool UpdateNmeaBurst(TtNmeaBurst burst)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Update(TwoTrailsSchema.PolygonSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TwoTrailsSchema.TtNmeaSchema.Used] = burst.IsUsed
                            },
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{burst.CN}'",
                            conn,
                            trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateNmeaBurst");
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

        public int UpdateNmeaBursts(IEnumerable<TtNmeaBurst> bursts)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtNmeaBurst burst in bursts)
                        {
                            _Database.Update(TwoTrailsSchema.PolygonSchema.TableName,
                                new Dictionary<string, object>()
                                {
                                    [TwoTrailsSchema.TtNmeaSchema.Used] = burst.IsUsed
                                },
                                $"{TwoTrailsSchema.SharedSchema.CN} = '{burst.CN}'",
                                conn,
                                trans);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateNmeaBursts");
                        trans.Rollback();
                        return -1;
                    }
                }
            }

            return bursts.Count();
        }

        public int DeleteNmeaBursts(string pointCN)
        {
            int res = -1;

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        res = _Database.Delete(TwoTrailsSchema.TtNmeaSchema.TableName,
                            $"{TwoTrailsSchema.TtNmeaSchema.PointCN} = '{pointCN}'",
                            conn,
                            trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeleteNmeaBursts");
                        trans.Rollback();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return res;
        }


        private Dictionary<string, object> GetNmeaValues(TtNmeaBurst burst)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = burst.CN,
                [TwoTrailsSchema.TtNmeaSchema.PointCN] = burst.PointCN,
                [TwoTrailsSchema.TtNmeaSchema.Used] = burst.IsUsed,
                [TwoTrailsSchema.TtNmeaSchema.TimeCreated] = burst.TimeCreated.ToString(Consts.DATE_FORMAT),
                [TwoTrailsSchema.TtNmeaSchema.FixTime] = burst.FixTime.ToString(Consts.DATE_FORMAT),
                [TwoTrailsSchema.TtNmeaSchema.Latitude] = burst.Latitude,
                [TwoTrailsSchema.TtNmeaSchema.LatDir] = (int)burst.LatDir,
                [TwoTrailsSchema.TtNmeaSchema.Longitude] = burst.Longitude,
                [TwoTrailsSchema.TtNmeaSchema.LonDir] = (int)burst.LonDir,
                [TwoTrailsSchema.TtNmeaSchema.Elevation] = burst.Elevation,
                [TwoTrailsSchema.TtNmeaSchema.ElevUom] = (int)burst.UomElevation,
                [TwoTrailsSchema.TtNmeaSchema.MagVar] = burst.MagVar,
                [TwoTrailsSchema.TtNmeaSchema.MagDir] = burst.MagVarDir,
                [TwoTrailsSchema.TtNmeaSchema.Fix] = (int)burst.Fix,
                [TwoTrailsSchema.TtNmeaSchema.FixQuality] = (int)burst.FixQuality,
                [TwoTrailsSchema.TtNmeaSchema.Mode] = (int)burst.Mode,
                [TwoTrailsSchema.TtNmeaSchema.PDOP] = burst.PDOP,
                [TwoTrailsSchema.TtNmeaSchema.HDOP] = burst.HDOP,
                [TwoTrailsSchema.TtNmeaSchema.VDOP] = burst.VDOP,
                [TwoTrailsSchema.TtNmeaSchema.HorizDilution] = burst.HorizDilution,
                [TwoTrailsSchema.TtNmeaSchema.GeiodHeight] = burst.GeoidHeight,
                [TwoTrailsSchema.TtNmeaSchema.GeiodHeightUom] = (int)burst.GeoUom,
                [TwoTrailsSchema.TtNmeaSchema.GroundSpeed] = burst.GroundSpeed,
                [TwoTrailsSchema.TtNmeaSchema.TrackAngle] = burst.TrackAngle,
                [TwoTrailsSchema.TtNmeaSchema.SatellitesUsedCount] = burst.UsedSatelliteIDsCount,
                [TwoTrailsSchema.TtNmeaSchema.SatellitesTrackedCount] = burst.TrackedSatellitesCount,
                [TwoTrailsSchema.TtNmeaSchema.SatellitesInViewCount] = burst.SatellitesInViewCount,
                [TwoTrailsSchema.TtNmeaSchema.UsedSatPRNS] = burst.UsedSatelliteIDsString
            };
        }
        #endregion


        #region Project
        public TtProjectInfo GetProjectInfo()
        {
            String query = $"select {TwoTrailsSchema.ProjectInfoSchema.SelectItems} from {TwoTrailsSchema.ProjectInfoSchema.TableName} limit 1";

            TtProjectInfo info = null;

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
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
                            date = TtCoreUtils.ParseTime(dr.GetString(5));
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
            String query = $"select {TwoTrailsSchema.ProjectInfoSchema.TtDbSchemaVersion} from {TwoTrailsSchema.ProjectInfoSchema.TableName} limit 1";

            Version version = null;

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        if (dr.Read())
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TwoTrailsSchema.ProjectInfoSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TwoTrailsSchema.ProjectInfoSchema.Name] = info.Name.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.District] = info.District.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.Forest] = info.Forest.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.Region] = info.Region.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.DeviceID] = info.CreationDeviceID,
                                [TwoTrailsSchema.ProjectInfoSchema.Created] = info.CreationDate.ToString(Consts.DATE_FORMAT),
                                [TwoTrailsSchema.ProjectInfoSchema.Description] = info.Description,
                                [TwoTrailsSchema.ProjectInfoSchema.TtDbSchemaVersion] = info.DbVersion.ToString(),
                                [TwoTrailsSchema.ProjectInfoSchema.TtVersion] = info.Version.ToString(),
                                [TwoTrailsSchema.ProjectInfoSchema.CreatedTtVersion] = info.CreationVersion.ToString()
                            },
                            conn,
                            trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertProjectInfo");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Update(TwoTrailsSchema.ProjectInfoSchema.TableName,
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
                            null,
                            conn,
                            trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateProjectInfo");
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
        public IEnumerable<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            String query = $"select {TwoTrailsSchema.PolygonAttrSchema.SelectItems} from {TwoTrailsSchema.PolygonAttrSchema.TableName}";

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
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
                            adjnav = dr.GetInt32(3);
                            unadjnav = dr.GetInt32(4);
                            adjpts = dr.GetInt32(5);
                            unadjpts = dr.GetInt32(6);
                            waypts = dr.GetInt32(7);

                            yield return new PolygonGraphicOptions(cn, adjbnd, unadjbnd, adjnav, unadjnav,
                                adjpts, unadjpts, waypts, 0, 0);
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }
        }

        public bool InsertPolygonGraphicOption(PolygonGraphicOptions option)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TwoTrailsSchema.PolygonAttrSchema.TableName, GetGraphicOptionValues(option), conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertPolygonGraphicOption");
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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Delete(TwoTrailsSchema.PolygonAttrSchema.TableName,
                            $"{TwoTrailsSchema.SharedSchema.CN} = '{option.CN}'",
                            conn,
                            trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:DeletePolygonGraphicOption");
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


        #region Activity

        public void InsertActivity(TtUserAction activity)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TwoTrailsSchema.ActivitySchema.TableName, GetUserActivityValues(activity), conn, trans);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:InsertActivity");
                        trans.Rollback();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        private Dictionary<string, object> GetUserActivityValues(TtUserAction activity)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.ActivitySchema.UserName] = activity.UserName,
                [TwoTrailsSchema.ActivitySchema.DeviceName] = activity.DeviceName,
                [TwoTrailsSchema.ActivitySchema.ActivityDate] = activity.Date,
                [TwoTrailsSchema.ActivitySchema.DataActivity] = (int)activity.Action
            };
        }

        public IEnumerable<TtUserAction> GetUserActivity()
        {
            String query = $"select {TwoTrailsSchema.ActivitySchema.SelectItems} from {TwoTrailsSchema.ActivitySchema.TableName}";

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        string username, devicename;
                        DateTime date;
                        DataActionType dat;

                        while (dr.Read())
                        {
                            username = dr.GetString(0);
                            devicename = dr.GetString(1);
                            date = TtCoreUtils.ParseTime(dr.GetString(2));
                            dat = (DataActionType)dr.GetInt32(3);

                            yield return new TtUserAction(username, devicename, date, dat);
                        }

                        dr.Close();
                    }
                }
            }
        }
        #endregion


        #region DataDictionary
        public DataDictionaryTemplate GetDataDictionaryTemplate()
        {
            if (_DataDictionaryTemplate == null)
            {
                _DataDictionaryTemplate = new DataDictionaryTemplate();

                String query = $"select {TwoTrailsSchema.DataDictionarySchema.SelectItems} from {TwoTrailsSchema.DataDictionarySchema.TableName}";

                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                    {
                        if (dr != null)
                        {
                            string cn, name, defaultValue = null;
                            int order, flag;
                            FeildType fieldType;
                            List<string> values = null;
                            DataType dataType;

                            while (dr.Read())
                            {
                                cn = dr.GetString(0);
                                name = dr.GetString(1);
                                order = dr.GetInt32(2);
                                fieldType = (FeildType)dr.GetInt32(3);
                                flag = dr.GetInt32(4);

                                if (!dr.IsDBNull(5))
                                    values = dr.GetString(5).Split('\n').ToList();

                                if (!dr.IsDBNull(6))
                                    defaultValue = dr.GetString(6);

                                dataType = (DataType)dr.GetInt32(7);

                                _DataDictionaryTemplate.AddField(new DataDictionaryField(cn)
                                {
                                    Name = name,
                                    Order = order,
                                    FeildType = fieldType,
                                    Flag = flag,
                                    Values = values,
                                    DefaultValue = defaultValue,
                                    DataType = dataType
                                });
                            }

                            dr.Close();
                        }
                    }

                    conn.Close();
                }
            }

            return _DataDictionaryTemplate;
        }

        public bool CreateDataDictionary(DataDictionaryTemplate template)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.ClearTable(TwoTrailsSchema.DataDictionarySchema.TableName, conn, trans);

                        Dictionary<string, SQLiteDataType> ddFields = new Dictionary<string, SQLiteDataType>();
                        ddFields.Add(TwoTrailsSchema.DataDictionarySchema.PointCN, SQLiteDataType.TEXT);

                        foreach (DataDictionaryField field in template)
                        {
                            InsertDataDictionaryField(field, conn, trans);
                            
                            ddFields.Add(field.CN, field.DataType == DataType.BOOLEAN ? SQLiteDataType.INTEGER : (SQLiteDataType)(int)field.DataType);
                        }

                        if (_Database.TableExists(TwoTrailsSchema.DataDictionarySchema.DataTableName))
                        {
                            _Database.DropTable(TwoTrailsSchema.DataDictionarySchema.DataTableName, conn, trans);
                        }

                        _Database.CreateTable(TwoTrailsSchema.DataDictionarySchema.DataTableName, ddFields, null, conn, trans);

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message, "DAL:UpdateDataDictionaryTemplate");
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


        protected void InsertDataDictionaryField(DataDictionaryField field, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Insert(TwoTrailsSchema.DataDictionarySchema.TableName, GetDataDictionaryFieldValues(field), conn, transaction);
        }

        private Dictionary<string, object> GetDataDictionaryFieldValues(DataDictionaryField field)
        {
            return new Dictionary<string, object>()
            {
                [TwoTrailsSchema.SharedSchema.CN] = field.CN,
                [TwoTrailsSchema.DataDictionarySchema.Name] = field.Name,
                [TwoTrailsSchema.DataDictionarySchema.Order] = field.Order,
                [TwoTrailsSchema.DataDictionarySchema.FieldType] = (int)field.FeildType,
                [TwoTrailsSchema.DataDictionarySchema.Flag] = field.Flag,
                [TwoTrailsSchema.DataDictionarySchema.Values] = field.Values != null && field.Values.Any() ? string.Join(Environment.NewLine, field.Values) : null,
                [TwoTrailsSchema.DataDictionarySchema.DefaultValue] = field.DefaultValue,
                [TwoTrailsSchema.DataDictionarySchema.DataType] = field.DataType
            };
        }
        
        
        public DataDictionary GetDataDictionary(string pointCN)
        {
            return GetDataDictionaries(
                $"{TwoTrailsSchema.DataDictionarySchema.DataTableName}.{TwoTrailsSchema.DataDictionarySchema.PointCN} = '{pointCN}'"
            ).First();
        }

        public IEnumerable<DataDictionary> GetDataDictionaries()
        {
            return GetDataDictionaries(null);
        }

        protected IEnumerable<DataDictionary> GetDataDictionaries(string where)
        {
            DataDictionaryTemplate template = GetDataDictionaryTemplate();

            if (template != null)
            {
                List<DataDictionaryField> fields = template.ToList();

                String query = $@"select { string.Join(", ", fields.Select(f => f.CN).ToArray()) } from 
{TwoTrailsSchema.DataDictionarySchema.DataTableName}{( where != null ? $" where { where }" : String.Empty )}";

                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                    {
                        if (dr != null)
                        {
                            Dictionary<string, object> dic;
                            string pointCN = null;
                            int findex;

                            while (dr.Read())
                            {
                                dic = new Dictionary<string, object>();
                                findex = 1;

                                pointCN = dr.GetString(0);

                                foreach (DataDictionaryField field in fields)
                                {
                                    object value = null;

                                    if (!dr.IsDBNull(findex))
                                    {
                                        switch (field.DataType)
                                        {
                                            case DataType.INTEGER:
                                                value = dr.GetInt32(findex);
                                                break;
                                            case DataType.DECIMAL:
                                                value = dr.GetDecimal(findex);
                                                break;
                                            case DataType.FLOAT:
                                                value = dr.GetFloat(findex);
                                                break;
                                            case DataType.STRING:
                                                value = dr.GetString(findex);
                                                break;
                                            case DataType.BYTE_ARRAY:
                                                value = dr.GetBytesEx(findex);
                                                break;
                                            case DataType.BOOLEAN:
                                                value = dr.GetBoolean(findex);
                                                break;
                                        }
                                    }

                                    dic.Add(field.CN, value);
                                    findex++;
                                }

                                yield return new DataDictionary(pointCN, dic);
                            }

                            dr.Close();
                        }
                    }

                    conn.Close();
                }
            }

            throw new Exception("No DataDictionary Template");
        }
        #endregion


        #region Utils
        public void Clean()
        {
            Dictionary<string, TtPolygon> polyCNs = GetPolygons().ToDictionary(p => p.CN, p => p);

            IEnumerable<TtPoint> delPoints = GetPoints().Where(p => !polyCNs.ContainsKey(p.PolygonCN));

            DeletePoints(delPoints);
        }
        #endregion
    }
}
