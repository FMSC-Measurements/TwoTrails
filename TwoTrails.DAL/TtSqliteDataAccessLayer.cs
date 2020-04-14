using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using CSUtil.Databases;
using System.Data.SQLite;
using CSUtil;
using FMSC.Core;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.Types;

using TTS = TwoTrails.DAL.TwoTrailsSchema;

namespace TwoTrails.DAL
{
    public class TtSqliteDataAccessLayer : ITtDataLayer
    {
        public String FilePath { get; }

        internal SQLiteDatabase _Database;

        private DataDictionaryTemplate _DataDictionaryTemplate;

        public bool HasDataDictionary { get { return GetDataDictionaryTemplate() != null; } }


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

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                database.ExecuteNonQuery(TTS.PointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.PolygonSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.GroupSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.MetadataSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.GpsPointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.TravPointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.QuondamPointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.ProjectInfoSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.TtNmeaSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.PolygonAttrSchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.ActivitySchema.CreateTable, conn);
                database.ExecuteNonQuery(TTS.DataDictionarySchema.CreateTable, conn);
            }

            TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(database);
            dal.InsertProjectInfo(projectInfo);

            return dal;
        }


        public bool RequiresUpgrade
        {
            get { return GetDataVersion() < TTS.SchemaVersion; }
        }

        public bool HandlesAllPointTypes => true;



        #region Points
        #region Get Points
        public TtPoint GetPoint(String pointCN, bool linked = false)
        {
            return GetPoints(
                $"{TTS.PointSchema.TableName}.{TTS.SharedSchema.CN} = '{pointCN}'",
                1,
                linked
            ).FirstOrDefault();
        }

        public IEnumerable<TtPoint> GetPoints(string polyCN = null, bool linked = false)
        {
            return GetPoints(
                polyCN != null ?
                    $"{TTS.PointSchema.PolyCN} = '{polyCN}'" :
                    null,
                0,
                linked
            );
        }
        
        protected IEnumerable<TtPoint> GetPoints(String where, int limit = 0, bool linked = true)
        {
            string ddQuerySelect = null, ddQueryTable = null;
            
            List<DataDictionaryField> fields = GetDataDictionaryTemplate()?.ToList();

            if (fields != null && fields.Any())
            {
                ddQueryTable = String.Format(" left join {0} on {0}.{1} = {2}.{3}",
                    TTS.DataDictionarySchema.ExtendDataTableName,
                    TTS.DataDictionarySchema.PointCN,
                    TTS.PointSchema.TableName,
                    TTS.SharedSchema.CN);

                ddQuerySelect = $", {( string.Join(", ", fields.Select(f => $"[{ f.CN }]").ToArray()) )}"; 
            }

            String query = String.Format(@"select {0}.{1}, {2}, {3}, {4}{5} from {0} left join {6} on {6}.{10} = {0}.{10} 
 left join {7} on {7}.{10} = {0}.{10} left join {8} on {8}.{10} = {0}.{10}{9}{11} order by {12} asc",
                TTS.PointSchema.TableName,              //0
                TTS.PointSchema.SelectItems,            //1
                TTS.GpsPointSchema.SelectItemsNoCN,     //2
                TTS.TravPointSchema.SelectItemsNoCN,    //3
                TTS.QuondamPointSchema.SelectItemsNoCN, //4
                ddQuerySelect,                          //5
                TTS.GpsPointSchema.TableName,           //6
                TTS.TravPointSchema.TableName,          //7
                TTS.QuondamPointSchema.TableName,       //8
                ddQueryTable,                           //9
                TTS.SharedSchema.CN,                    //10
                where != null ? $" where {where}" : String.Empty,   //11
                TTS.PointSchema.Index                   //12
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
                        DataDictionary extendedData = null;

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

                            unadjx = dr.GetDouble(13);
                            unadjy = dr.GetDouble(14);
                            unadjz = dr.GetDouble(15);

                            //fix for non-adjusted points
                            adjx = dr.GetDoubleN(10) ?? unadjx;
                            adjy = dr.GetDoubleN(11) ?? unadjy;
                            adjz = dr.GetDoubleN(12) ?? unadjz;

                            acc = dr.GetDoubleN(16) ?? Consts.DEFAULT_POINT_ACCURACY;

                            qlinks = dr.GetStringN(17);
                            
                            if (fields != null)
                            {
                                extendedData = new DataDictionary();

                                int ei = 29;
                                foreach (DataDictionaryField ddf in fields)
                                {
                                    object obj = null;

                                    Type type = dr.GetFieldType(ei);
                                    string n = dr.GetName(ei);

                                    switch (ddf.DataType)
                                    {
                                        case DataType.INTEGER: obj = dr.GetInt32N(ei); break;
                                        case DataType.DECIMAL: obj = dr.GetDecimalN(ei); break;
                                        case DataType.FLOAT: obj = dr.GetDoubleN(ei); break;
                                        case DataType.TEXT: obj = dr.GetStringN(ei); break;
                                        case DataType.BYTE_ARRAY: obj = dr.GetBytesEx(ei); break;
                                        case DataType.BOOLEAN: obj = dr.GetBoolN(ei); break;
                                        default:
                                            throw new Exception("Unknown DataType");
                                    }

                                    extendedData[ddf.CN] = obj;

                                    ei++;
                                }
                            }

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
                                                    acc, qlinks, lat, lon, elev, manacc, rmser, extendedData);
                                        break;
                                    case OpType.Take5:
                                        point = new Take5Point(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    acc, qlinks, lat, lon, elev, manacc, rmser, extendedData);
                                        break;
                                    case OpType.Walk:
                                        point = new WalkPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    acc, qlinks, lat, lon, elev, manacc, rmser, extendedData);
                                        break;
                                    case OpType.WayPoint:
                                        point = new WayPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    acc, qlinks, lat, lon, elev, manacc, rmser, extendedData);
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
                                                acc, qlinks, fw, bk, sd, sa, extendedData);
                                }
                                else
                                {
                                    point = new SideShotPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                acc, qlinks, fw, bk, sd, sa, extendedData);
                                }
                            }
                            else if (op == OpType.Quondam)
                            {
                                pcn = dr.GetString(27);
                                manacc = dr.GetDoubleN(28);

                                point = new QuondamPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                acc, qlinks, pcn, manacc, extendedData);

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
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        InsertBasePoint(point, conn, trans);
                        InsertPointTypeData(point, conn, trans);
                        InsertExtendedData(point, conn, trans);

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

        public int InsertPoints(IEnumerable<TtPoint> points)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        bool hasDD = HasDataDictionary;

                        foreach (TtPoint point in points)
                        {
                            InsertBasePoint(point, conn, trans);
                            InsertPointTypeData(point, conn, trans);

                            if (hasDD)
                                InsertExtendedData(point, conn, trans);
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
        

        private void InsertBasePoint(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Insert(TTS.PointSchema.TableName, GetBasePointValues(point), conn, transaction);
        }
        
        private void InsertExtendedData(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            if (point.ExtendedData != null)
                _Database.Insert(TTS.DataDictionarySchema.ExtendDataTableName, GetExtendedPointValues(point, true), conn, transaction);
        }

        private void InsertPointTypeData(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            switch (point.OpType)
            {
                case OpType.GPS:
                case OpType.Take5:
                case OpType.Walk:
                case OpType.WayPoint:
                    _Database.Insert(TTS.GpsPointSchema.TableName, GetGpsPointValues(point as GpsPoint), conn, transaction);
                    break;
                case OpType.Traverse:
                case OpType.SideShot:
                    _Database.Insert(TTS.TravPointSchema.TableName, GetTravPointValues(point as TravPoint), conn, transaction);
                    break;
                case OpType.Quondam:
                    _Database.Insert(TTS.QuondamPointSchema.TableName, GetQndmPointValues(point as QuondamPoint), conn, transaction);
                    break;
            }
        }

        public bool UpdatePoint(Tuple<TtPoint, TtPoint> point)
        {
            return UpdatePoint(point.Item1, point.Item2);
        }

        private bool UpdatePoint(TtPoint point, TtPoint oldPoint)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string where = $"{TTS.SharedSchema.CN} = '{point.CN}'";

                        UpdateBasePoint(point, oldPoint, conn, trans, where);
                        UpdatePointTypeData(point, oldPoint, conn, trans, where);

                        if (HasDataDictionary)
                            UpdateExtendedData(point, conn, trans, where);

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

        public int UpdatePoints(IEnumerable<Tuple<TtPoint, TtPoint>> points)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        bool hasDD = HasDataDictionary;

                        foreach (Tuple<TtPoint, TtPoint> point in points)
                        {
                            string where = $"{TTS.SharedSchema.CN} = '{point.Item1.CN}'";

                            UpdateBasePoint(point.Item1, point.Item2, conn, trans, where);
                            UpdatePointTypeData(point.Item1, point.Item2, conn, trans, where);

                            if (hasDD)
                                UpdateExtendedData(point.Item1, conn, trans, $"{TTS.DataDictionarySchema.PointCN} = '{point.Item1.CN}'");
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
        


        private void UpdateBasePoint(TtPoint point, TtPoint oldPoint, SQLiteConnection conn, SQLiteTransaction transaction, string  where)
        {
            _Database.Update(TTS.PointSchema.TableName,
                GetBasePointValues(point),
                where,
                conn,
                transaction);
        }

        private void UpdateExtendedData(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction, string where)
        {
            if (point.ExtendedData != null)
                _Database.Update(TTS.DataDictionarySchema.ExtendDataTableName, GetExtendedPointValues(point), where, conn, transaction);
        }

        private void UpdatePointTypeData(TtPoint point, TtPoint oldPoint, SQLiteConnection conn, SQLiteTransaction transaction, string where)
        {
            if (point.OpType != oldPoint.OpType)
            {
                if (oldPoint.IsGpsType())
                {
                    _Database.Delete(TTS.GpsPointSchema.TableName, where, conn, transaction);
                }
                else if (oldPoint.IsTravType())
                {
                    _Database.Delete(TTS.TravPointSchema.TableName, where, conn, transaction);
                }
                else if (oldPoint.OpType == OpType.Quondam)
                {
                    _Database.Delete(TTS.QuondamPointSchema.TableName, where, conn, transaction);
                }

                InsertPointTypeData(point, conn, transaction);
            }
            else
            {
                switch (point.OpType)
                {
                    case OpType.GPS:
                    case OpType.Take5:
                    case OpType.Walk:
                    case OpType.WayPoint:
                        _Database.Update(TTS.GpsPointSchema.TableName,
                            GetGpsPointValues(point as GpsPoint),
                            where,
                            conn,
                            transaction);
                        break;
                    case OpType.Traverse:
                    case OpType.SideShot:
                        _Database.Update(TTS.TravPointSchema.TableName,
                            GetTravPointValues(point as TravPoint),
                            where,
                            conn,
                            transaction);
                        break;
                    case OpType.Quondam:
                        _Database.Update(TTS.QuondamPointSchema.TableName,
                            GetQndmPointValues(point as QuondamPoint),
                            where,
                            conn,
                            transaction);
                        break;
                }
            }
        }



        private Dictionary<string, object> GetBasePointValues(TtPoint point)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>()
            {
                [TTS.SharedSchema.CN] = point.CN,
                [TTS.PointSchema.Index] = point.Index,
                [TTS.PointSchema.ID] = point.PID,
                [TTS.PointSchema.PolyCN] = point.PolygonCN,
                [TTS.PointSchema.GroupCN] = point.GroupCN,
                [TTS.PointSchema.OnBoundary] = point.OnBoundary,
                [TTS.PointSchema.Comment] = point.Comment,
                [TTS.PointSchema.Operation] = (int)point.OpType,
                [TTS.PointSchema.MetadataCN] = point.MetadataCN,
                [TTS.PointSchema.CreationTime] = point.TimeCreated.ToString(Consts.DATE_FORMAT),
                [TTS.PointSchema.Accuracy] = point.Accuracy,
                [TTS.PointSchema.QuondamLinks] = point.LinkedPoints.ToStringContents("_")
            };

            if (point.OpType != OpType.Quondam || (point as QuondamPoint).ParentPoint != null)
            {
                dic.Add(TTS.PointSchema.AdjX, point.AdjX);
                dic.Add(TTS.PointSchema.AdjY, point.AdjY);
                dic.Add(TTS.PointSchema.AdjZ, point.AdjZ);
                dic.Add(TTS.PointSchema.UnAdjX, point.UnAdjX);
                dic.Add(TTS.PointSchema.UnAdjY, point.UnAdjY);
                dic.Add(TTS.PointSchema.UnAdjZ, point.UnAdjZ);
            }
            else
            {
                dic.Add(TTS.PointSchema.AdjX, 0);
                dic.Add(TTS.PointSchema.AdjY, 0);
                dic.Add(TTS.PointSchema.AdjZ, 0);
                dic.Add(TTS.PointSchema.UnAdjX, 0);
                dic.Add(TTS.PointSchema.UnAdjY, 0);
                dic.Add(TTS.PointSchema.UnAdjZ, 0);
            }

            if (point.Polygon != null)
                dic.Add(TTS.PointSchema.PolyName, point.Polygon.Name);

            if (point.Group != null)
                dic.Add(TTS.PointSchema.GroupName, point.Group.Name);

            return dic;
        }

        private Dictionary<string, object> GetExtendedPointValues(TtPoint point, bool insert = false)
        {
            if (insert)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    [TTS.DataDictionarySchema.PointCN] = point.CN
                };

                foreach (KeyValuePair<string, object> kvp in point.ExtendedData)
                    data.Add($"[{kvp.Key}]", kvp.Value);

                return data;
            }
            else
            {
                return point.ExtendedData.ToDictionary(kvp => $"[{kvp.Key}]", kvp => kvp.Value);
            }
        }

        private Dictionary<string, object> GetGpsPointValues(GpsPoint point)
        {
            return new Dictionary<string, object>()
            {
                [TTS.SharedSchema.CN] = point.CN,
                [TTS.GpsPointSchema.Latitude] = point.Latitude,
                [TTS.GpsPointSchema.Longitude] = point.Longitude,
                [TTS.GpsPointSchema.Elevation] = point.Elevation,
                [TTS.GpsPointSchema.ManualAccuracy] = point.ManualAccuracy
            };
        }

        private Dictionary<string, object> GetTravPointValues(TravPoint point)
        {
            return new Dictionary<string, object>()
            {
                [TTS.SharedSchema.CN] = point.CN,
                [TTS.TravPointSchema.ForwardAz] = point.FwdAzimuth,
                [TTS.TravPointSchema.BackAz] = point.BkAzimuth,
                [TTS.TravPointSchema.SlopeDistance] = point.SlopeDistance,
                [TTS.TravPointSchema.SlopeAngle] = point.SlopeAngle,
                [TTS.TravPointSchema.HorizDistance] = point.HorizontalDistance
            };
        }

        private Dictionary<string, object> GetQndmPointValues(QuondamPoint point)
        {
            return new Dictionary<string, object>()
            {
                [TTS.SharedSchema.CN] = point.CN,
                [TTS.QuondamPointSchema.ParentPointCN] = point.ParentPointCN,
                [TTS.QuondamPointSchema.ManualAccuracy] = point.ManualAccuracy
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
                        String where = $"{TTS.SharedSchema.CN} = '{point.CN}'";
                        if (DeleteBasePoint(where, conn, trans))
                        {
                            if (HasDataDictionary)
                                DeleteExtendedData(where, conn, trans);

                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, conn, trans);
                                    DeleteNmeaBursts($"{TTS.TtNmeaSchema.PointCN} = '{point.CN}'", conn, trans);
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
            int deleted = 0;

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (TtPoint point in points)
                        {
                            String where = $"{TTS.SharedSchema.CN} = '{point.CN}'";

                            if (DeleteBasePoint(where, conn, trans))
                                deleted++;

                            if (HasDataDictionary)
                                DeleteExtendedData(where, conn, trans);

                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, conn, trans);
                                    DeleteNmeaBursts($"{TTS.TtNmeaSchema.PointCN} = '{point.CN}'", conn, trans);
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

            return deleted;
        } 


        private bool DeleteBasePoint(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            return _Database.Delete(TTS.PointSchema.TableName, where, conn, transaction) > 0;
        }

        private void DeleteExtendedData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TTS.DataDictionarySchema.ExtendDataTableName, where, conn, transaction);
        }

        private void DeleteGpsPointData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TTS.GpsPointSchema.TableName, where, conn, transaction);
        }

        private void DeleteTravPointData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TTS.TravPointSchema.TableName, where, conn, transaction);
        }

        private void DeleteQndmPointData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TTS.QuondamPointSchema.TableName, where, conn, transaction);
        }
        #endregion
        #endregion


        #region Polygons
        #region Get Polygons
        public bool HasPolygons()
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader($"select count(*) from {TTS.PolygonSchema.TableName}", conn))
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


        public IEnumerable<TtPolygon> GetPolygons() => GetPolygons(null);

        public IEnumerable<TtPolygon> GetPolygons(string where)
        {
            String query = $"select {TTS.PolygonSchema.SelectItems} from {TTS.PolygonSchema.TableName}{(where != null ? $" where {where}" : String.Empty)}";

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
                        _Database.Insert(TTS.PolygonSchema.TableName, GetPolygonValues(polygon), conn, trans);
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
                            _Database.Insert(TTS.PolygonSchema.TableName, GetPolygonValues(poly), conn, trans);
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
                        _Database.Update(TTS.PolygonSchema.TableName,
                            GetPolygonValues(polygon),
                            $"{TTS.SharedSchema.CN} = '{polygon.CN}'",
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
                            _Database.Update(TTS.PolygonSchema.TableName,
                                GetPolygonValues(poly),
                                $"{TTS.SharedSchema.CN} = '{poly.CN}'",
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
                [TTS.SharedSchema.CN] = poly.CN,
                [TTS.PolygonSchema.Name] = poly.Name,
                [TTS.PolygonSchema.Description] = poly.Description,
                [TTS.PolygonSchema.Accuracy] = poly.Accuracy,
                [TTS.PolygonSchema.IncrementBy] = poly.Increment,
                [TTS.PolygonSchema.PointStartIndex] = poly.PointStartIndex,
                [TTS.PolygonSchema.TimeCreated] = poly.TimeCreated.ToString(Consts.DATE_FORMAT),
                [TTS.PolygonSchema.Area] = poly.Area,
                [TTS.PolygonSchema.Perimeter] = poly.Perimeter,
                [TTS.PolygonSchema.PerimeterLine] = poly.PerimeterLine
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
                        _Database.Delete(TTS.PolygonSchema.TableName,
                            $"{TTS.SharedSchema.CN} = '{polygon.CN}'",
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
                            sb.Append($"{TTS.SharedSchema.CN} = '{poly.CN}'{(count < total ? " or " : String.Empty)}");
                        }

                        string where = sb.ToString();
                        
                        if (!String.IsNullOrEmpty(where) &&
                            _Database.Delete(TTS.PolygonSchema.TableName,
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
            String query = $@"select {TTS.MetadataSchema.SelectItems} from {TTS.MetadataSchema.TableName}";

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
                        _Database.Insert(TTS.MetadataSchema.TableName, GetMetadataValues(metadata), conn, trans);
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
                            _Database.Insert(TTS.MetadataSchema.TableName, GetMetadataValues(meta), conn, trans);
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
                        _Database.Update(TTS.MetadataSchema.TableName,
                            GetMetadataValues(metadata),
                            $"{TTS.SharedSchema.CN} = '{metadata.CN}'",
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
                            _Database.Update(TTS.MetadataSchema.TableName,
                                GetMetadataValues(meta),
                                $"{TTS.SharedSchema.CN} = '{meta.CN}'",
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
                [TTS.SharedSchema.CN] = meta.CN,
                [TTS.MetadataSchema.Name] = meta.Name,
                [TTS.MetadataSchema.Comment] = meta.Comment,
                [TTS.MetadataSchema.UtmZone] = meta.Zone,
                [TTS.MetadataSchema.Datum] = (int)meta.Datum,
                [TTS.MetadataSchema.Distance] = (int)meta.Distance,
                [TTS.MetadataSchema.Elevation] = (int)meta.Elevation,
                [TTS.MetadataSchema.Slope] = (int)meta.Slope,
                [TTS.MetadataSchema.DeclinationType] = (int)meta.DecType,
                [TTS.MetadataSchema.MagDec] = meta.MagDec,
                [TTS.MetadataSchema.GpsReceiver] = meta.GpsReceiver,
                [TTS.MetadataSchema.RangeFinder] = meta.RangeFinder,
                [TTS.MetadataSchema.Compass] = meta.Compass,
                [TTS.MetadataSchema.Crew] = meta.Crew
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
                        _Database.Delete(TTS.MetadataSchema.TableName,
                            $"{TTS.SharedSchema.CN} = '{metadata.CN}'",
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
                            sb.Append($"{TTS.SharedSchema.CN} = '{meta.CN}' {(count < total ? " or " : String.Empty)}");
                        }

                        string where = sb.ToString();

                        if (!String.IsNullOrEmpty(where) &&
                            _Database.Delete(TTS.MetadataSchema.TableName,
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
            String query = $"select {TTS.GroupSchema.SelectItems} from {TTS.GroupSchema.TableName}";

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
                        _Database.Insert(TTS.GroupSchema.TableName, GetGroupValues(group), conn, trans);
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
                            _Database.Insert(TTS.GroupSchema.TableName, GetGroupValues(group), conn, trans);
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
                        _Database.Update(TTS.GroupSchema.TableName,
                            GetGroupValues(group),
                            $"{TTS.SharedSchema.CN} = '{group.CN}'",
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
                            _Database.Update(TTS.GroupSchema.TableName,
                                GetGroupValues(group),
                                $"{TTS.SharedSchema.CN} = '{group.CN}'",
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
                [TTS.SharedSchema.CN] = group.CN,
                [TTS.GroupSchema.Name] = group.Name,
                [TTS.GroupSchema.Description] = group.Description,
                [TTS.GroupSchema.Type] = (int)group.GroupType
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
                        _Database.Delete(TTS.GroupSchema.TableName,
                            $"{TTS.SharedSchema.CN} = '{group.CN}'",
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
                            sb.Append($"{TTS.SharedSchema.CN} = '{group.CN}' {(count < total ? " or " : String.Empty)}");
                        }

                        string where = sb.ToString();

                        if (!String.IsNullOrEmpty(where) &&
                            _Database.Delete(TTS.GroupSchema.TableName,
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
            return GetNmeaBursts(pointCN != null ? new string[] { pointCN } : null);
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(IEnumerable<string> pointCNs)
        {
            String query = $@"select {TTS.TtNmeaSchema.SelectItems} from {TTS.TtNmeaSchema.TableName} 
            {(pointCNs == null ? String.Empty : $" where { String.Join(" OR ", pointCNs.Select(pcn => $"{TTS.TtNmeaSchema.PointCN} = '{pcn}'"))}")}";

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
                                dr.GetDoubleN(11), (EastWest?)dr.GetInt32N(12),
                                (Mode)dr.GetInt32(15), (Fix)dr.GetInt32(13),
                                ParseIds(dr.GetStringN(27)),
                                dr.GetDouble(16),
                                dr.GetDouble(17),
                                dr.GetDouble(18),
                                (GpsFixType)dr.GetInt32(14),
                                dr.GetInt32(25),
                                dr.GetDouble(19),
                                dr.GetDouble(20), (UomElevation)dr.GetInt32(21),
                                dr.GetInt32(26),
                                dr.GetStringN(28)
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
                        _Database.Insert(TTS.TtNmeaSchema.TableName, GetNmeaValues(burst), conn, trans);
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
                            _Database.Insert(TTS.TtNmeaSchema.TableName, GetNmeaValues(burst), conn, trans);
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
                        _Database.Update(TTS.PolygonSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TTS.TtNmeaSchema.Used] = burst.IsUsed
                            },
                            $"{TTS.SharedSchema.CN} = '{burst.CN}'",
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
                            _Database.Update(TTS.PolygonSchema.TableName,
                                new Dictionary<string, object>()
                                {
                                    [TTS.TtNmeaSchema.Used] = burst.IsUsed
                                },
                                $"{TTS.SharedSchema.CN} = '{burst.CN}'",
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
                        DeleteNmeaBursts($"{TTS.TtNmeaSchema.PointCN} = '{pointCN}'", conn, trans);
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
        
        private void DeleteNmeaBursts(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TTS.TtNmeaSchema.TableName, where, conn, transaction);
        }


        private Dictionary<string, object> GetNmeaValues(TtNmeaBurst burst)
        {
            return new Dictionary<string, object>()
            {
                [TTS.SharedSchema.CN] = burst.CN,
                [TTS.TtNmeaSchema.PointCN] = burst.PointCN,
                [TTS.TtNmeaSchema.Used] = burst.IsUsed,
                [TTS.TtNmeaSchema.TimeCreated] = burst.TimeCreated.ToString(Consts.DATE_FORMAT),
                [TTS.TtNmeaSchema.FixTime] = burst.FixTime.ToString(Consts.DATE_FORMAT),
                [TTS.TtNmeaSchema.Latitude] = burst.Latitude,
                [TTS.TtNmeaSchema.LatDir] = (int)burst.LatDir,
                [TTS.TtNmeaSchema.Longitude] = burst.Longitude,
                [TTS.TtNmeaSchema.LonDir] = (int)burst.LonDir,
                [TTS.TtNmeaSchema.Elevation] = burst.Elevation,
                [TTS.TtNmeaSchema.ElevUom] = (int)burst.UomElevation,
                [TTS.TtNmeaSchema.MagVar] = burst.MagVar,
                [TTS.TtNmeaSchema.MagDir] = burst.MagVarDir,
                [TTS.TtNmeaSchema.Fix] = (int)burst.Fix,
                [TTS.TtNmeaSchema.FixQuality] = (int)burst.FixQuality,
                [TTS.TtNmeaSchema.Mode] = (int)burst.Mode,
                [TTS.TtNmeaSchema.PDOP] = burst.PDOP,
                [TTS.TtNmeaSchema.HDOP] = burst.HDOP,
                [TTS.TtNmeaSchema.VDOP] = burst.VDOP,
                [TTS.TtNmeaSchema.HorizDilution] = burst.HorizDilution,
                [TTS.TtNmeaSchema.GeiodHeight] = burst.GeoidHeight,
                [TTS.TtNmeaSchema.GeiodHeightUom] = (int)burst.GeoUom,
                [TTS.TtNmeaSchema.GroundSpeed] = burst.GroundSpeed,
                [TTS.TtNmeaSchema.TrackAngle] = burst.TrackAngle,
                [TTS.TtNmeaSchema.SatellitesUsedCount] = burst.UsedSatelliteIDsCount,
                [TTS.TtNmeaSchema.SatellitesTrackedCount] = burst.TrackedSatellitesCount,
                [TTS.TtNmeaSchema.SatellitesInViewCount] = burst.SatellitesInViewCount,
                [TTS.TtNmeaSchema.UsedSatPRNS] = burst.UsedSatelliteIDsString
            };
        }
        #endregion


        #region Project
        public TtProjectInfo GetProjectInfo()
        {
            String query = $"select {TTS.ProjectInfoSchema.SelectItems} from {TTS.ProjectInfoSchema.TableName} limit 1";

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
                            district = dr.GetStringN(1);
                            forest = dr.GetStringN(2);
                            region = dr.GetStringN(3);
                            deviceID = dr.GetStringN(4);
                            date = TtCoreUtils.ParseTime(dr.GetString(5));
                            desc = dr.GetStringN(6);
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
            String query = $"select {TTS.ProjectInfoSchema.TtDbSchemaVersion} from {TTS.ProjectInfoSchema.TableName} limit 1";

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

        private bool InsertProjectInfo(TtProjectInfo info)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        _Database.Insert(TTS.ProjectInfoSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TTS.ProjectInfoSchema.Name] = info.Name.Trim(),
                                [TTS.ProjectInfoSchema.District] = info.District?.Trim(),
                                [TTS.ProjectInfoSchema.Forest] = info.Forest?.Trim(),
                                [TTS.ProjectInfoSchema.Region] = info.Region?.Trim(),
                                [TTS.ProjectInfoSchema.DeviceID] = info.CreationDeviceID?.Trim(),
                                [TTS.ProjectInfoSchema.Created] = info.CreationDate.ToString(Consts.DATE_FORMAT),
                                [TTS.ProjectInfoSchema.Description] = info.Description,
                                [TTS.ProjectInfoSchema.TtDbSchemaVersion] = info.DbVersion.ToString(),
                                [TTS.ProjectInfoSchema.TtVersion] = info.Version.ToString(),
                                [TTS.ProjectInfoSchema.CreatedTtVersion] = info.CreationVersion.ToString()
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
                        _Database.Update(TTS.ProjectInfoSchema.TableName,
                            new Dictionary<string, object>()
                            {
                                [TTS.ProjectInfoSchema.Name] = info.Name,
                                [TTS.ProjectInfoSchema.District] = info.District?.Trim(),
                                [TTS.ProjectInfoSchema.Forest] = info.Forest?.Trim(),
                                [TTS.ProjectInfoSchema.Region] = info.Region?.Trim(),
                                [TTS.ProjectInfoSchema.DeviceID] = info.CreationDeviceID?.Trim(),
                                [TTS.ProjectInfoSchema.Description] = info.Description,
                                [TTS.ProjectInfoSchema.TtVersion] = info.Version.ToString()
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
            String query = $"select {TTS.PolygonAttrSchema.SelectItems} from {TTS.PolygonAttrSchema.TableName}";

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
                        _Database.Insert(TTS.PolygonAttrSchema.TableName, GetGraphicOptionValues(option), conn, trans);
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
                [TTS.SharedSchema.CN] = option.CN,
                [TTS.PolygonAttrSchema.AdjBndColor] = option.AdjBndColor,
                [TTS.PolygonAttrSchema.UnAdjBndColor] = option.UnAdjBndColor,
                [TTS.PolygonAttrSchema.AdjNavColor] = option.AdjNavColor,
                [TTS.PolygonAttrSchema.UnAdjNavColor] = option.UnAdjNavColor,
                [TTS.PolygonAttrSchema.AdjPtsColor] = option.AdjPtsColor,
                [TTS.PolygonAttrSchema.UnAdjPtsColor] = option.UnAdjPtsColor,
                [TTS.PolygonAttrSchema.WayPtsColor] = option.WayPtsColor
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
                        _Database.Delete(TTS.PolygonAttrSchema.TableName,
                            $"{TTS.SharedSchema.CN} = '{option.CN}'",
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
                        _Database.Insert(TTS.ActivitySchema.TableName, GetUserActivityValues(activity), conn, trans);
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
                [TTS.ActivitySchema.UserName] = activity.UserName,
                [TTS.ActivitySchema.DeviceName] = activity.DeviceName,
                [TTS.ActivitySchema.ActivityDate] = activity.Date,
                [TTS.ActivitySchema.ActivityType] = (int)activity.Action,
                [TTS.ActivitySchema.ActivityNotes] = activity.Notes
            };
        }

        public IEnumerable<TtUserAction> GetUserActivity()
        {
            String query = $"select {TTS.ActivitySchema.SelectItems} from {TTS.ActivitySchema.TableName}";

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        string username, devicename, notes;
                        DateTime date;
                        DataActionType dat;

                        while (dr.Read())
                        {
                            username = dr.GetString(0);
                            devicename = dr.GetString(1);
                            date = TtCoreUtils.ParseTime(dr.GetString(2));
                            dat = (DataActionType)dr.GetInt32(3);
                            notes = dr.GetStringN(4);

                            yield return new TtUserAction(username, devicename, date, dat, notes);
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
            if (_DataDictionaryTemplate == null && _Database.TableExists(TTS.DataDictionarySchema.TableName))
            {
                _DataDictionaryTemplate = new DataDictionaryTemplate();

                String query = $"select {TTS.DataDictionarySchema.SelectItems} from {TTS.DataDictionarySchema.TableName}";

                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteDataReader dr = _Database.ExecuteReader(query, conn))
                    {
                        if (dr != null)
                        {
                            string cn, name;
                            object defaultValue = null;
                            int order, flags = 0;
                            FieldType fieldType;
                            List<string> values = null;
                            DataType dataType;
                            bool valRequired = false;

                            while (dr.Read())
                            {
                                cn = dr.GetString(0);
                                name = dr.GetString(1);
                                order = dr.GetInt32(2);
                                fieldType = (FieldType)dr.GetInt32(3);

                                flags = !dr.IsDBNull(4) ? dr.GetInt32(4) : 0;

                                values = !dr.IsDBNull(5) ? dr.GetString(5).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList() : null;
                                
                                dataType = (DataType)dr.GetInt32(8);

                                if (!dr.IsDBNull(6))
                                {
                                    switch (dataType)
                                    {
                                        case DataType.BOOLEAN:
                                            defaultValue = int.Parse(dr.GetString(6)) > 0;
                                            break;
                                        case DataType.INTEGER:
                                            defaultValue = int.Parse(dr.GetString(6));
                                            break;
                                        case DataType.DECIMAL:
                                            defaultValue = decimal.Parse(dr.GetString(6));
                                            break;
                                        case DataType.FLOAT:
                                            defaultValue = double.Parse(dr.GetString(6));
                                            break;
                                        case DataType.TEXT:
                                            defaultValue = dr.GetString(6);
                                            break;
                                    }
                                }
                                else
                                    defaultValue = null;

                                valRequired = dr.GetBoolean(7);

                                _DataDictionaryTemplate.AddField(new DataDictionaryField(cn)
                                {
                                    Name = name,
                                    Order = order,
                                    FieldType = fieldType,
                                    Flags = flags,
                                    Values = values,
                                    DefaultValue = defaultValue,
                                    DataType = dataType,
                                    ValueRequired = valRequired
                                });
                            }

                            dr.Close();
                        }
                    }

                    conn.Close();
                }
            }

            return _DataDictionaryTemplate?.Any() == true ? _DataDictionaryTemplate : null;
        }

        public bool CreateDataDictionary(DataDictionaryTemplate template)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        CreateDataDictionaryTable(template, conn, trans);
                        _DataDictionaryTemplate = new DataDictionaryTemplate(template);
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

        private void CreateDataDictionaryTable(DataDictionaryTemplate template, SQLiteConnection conn, SQLiteTransaction trans)
        {
            Dictionary<string, SQLiteDataType> ddFields = new Dictionary<string, SQLiteDataType>();
            Dictionary<string, object> defaultValues = new Dictionary<string, object>();
            ddFields.Add(TTS.DataDictionarySchema.PointCN, SQLiteDataType.TEXT);

            _Database.ClearTable(TTS.DataDictionarySchema.TableName, conn, trans);

            foreach (DataDictionaryField field in template)
            {
                _Database.Insert(TTS.DataDictionarySchema.TableName, GetDataDictionaryFieldValues(field), conn, trans);

                ddFields.Add($"[{field.CN}]", field.DataType == DataType.BOOLEAN ? SQLiteDataType.INTEGER : (SQLiteDataType)(int)field.DataType);

                if (field.ValueRequired || field.DefaultValue != null)
                {
                    defaultValues.Add($"[{field.CN}]", field.GetDefaultValue());
                }
            }

            if (_Database.TableExists(TTS.DataDictionarySchema.ExtendDataTableName))
            {
                Trace.WriteLine("Overwritting DataDictionary", "DAL:UpdateDataDictionaryTemplate");
                _Database.DropTable(TTS.DataDictionarySchema.ExtendDataTableName, conn, trans);
            }

            _Database.CreateTable(TTS.DataDictionarySchema.ExtendDataTableName, ddFields, TTS.DataDictionarySchema.PointCN, conn, trans);

            if (defaultValues.Count > 0)
            {
                _Database.ExecuteNonQuery(
                    $"INSERT INTO {TTS.DataDictionarySchema.ExtendDataTableName} ({TTS.DataDictionarySchema.PointCN}) SELECT ({TTS.SharedSchema.CN}) FROM {TTS.PointSchema.TableName};", conn, trans);

                _Database.Update(TTS.DataDictionarySchema.ExtendDataTableName, defaultValues, null, conn, trans);
            }
        }

        public bool ModifyDataDictionary(DataDictionaryTemplate template)
        {
            if (!HasDataDictionary)
                return CreateDataDictionary(template);
            else
            {
                using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                {
                    using (SQLiteTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            DataDictionaryTemplate oldTemplate = GetDataDictionaryTemplate();

                            //recreate table is field is deleted
                            if (!oldTemplate.All(f => template.HasField(f.CN)))
                            {
                                //rebuild table since some fields removed
                                if (_Database.TableExists(TTS.DataDictionarySchema.TempExtendDataTableName))
                                    _Database.DropTable(TTS.DataDictionarySchema.TempExtendDataTableName, conn, trans);
                                
                                _Database.ExecuteNonQuery(
                                    $"ALTER TABLE {TTS.DataDictionarySchema.ExtendDataTableName} RENAME TO {TTS.DataDictionarySchema.TempExtendDataTableName};",
                                    conn, trans);


                                IEnumerable<DataDictionaryField> oldFields = template.Where(f => oldTemplate.HasField(f.CN));
                                string oldFieldsStr = String.Join(", ", oldFields.Select(f => $"[{f.CN}]"));
                                
                                _Database.ExecuteNonQuery(
                                    $"CREATE TABLE {TTS.DataDictionarySchema.ExtendDataTableName} AS SELECT {TTS.DataDictionarySchema.PointCN}, {oldFieldsStr} FROM {TTS.DataDictionarySchema.TempExtendDataTableName};",
                                    conn, trans);

                                _Database.DropTable(TTS.DataDictionarySchema.TempExtendDataTableName, conn, trans);

                                _Database.Delete(TTS.DataDictionarySchema.TableName,
                                    String.Join(" OR ", oldTemplate.Where(f => !template.HasField(f.CN)).Select(f => $"CN == '{f.CN}'")),
                                    conn, trans);
                            }

                            //add fields to table
                            List<DataDictionaryField> addFields = template.Where(f => !oldTemplate.HasField(f.CN)).ToList();

                            if (addFields.Any())
                            {
                                Dictionary<string, object> updates = new Dictionary<string, object>();

                                Func<DataType, SQLiteDataType> convertDataType = (dt) => dt == DataType.BOOLEAN ? SQLiteDataType.INTEGER : (SQLiteDataType)(int)dt;

                                foreach (DataDictionaryField field in addFields)
                                {
                                    _Database.Insert(TTS.DataDictionarySchema.TableName, GetDataDictionaryFieldValues(field), conn, trans);
                                        
                                    _Database.ExecuteNonQuery($@"ALTER TABLE {TTS.DataDictionarySchema.ExtendDataTableName} ADD [{field.CN}] {convertDataType(field.DataType).ToString()}", conn, trans);

                                    if (field.DefaultValue != null)
                                    {
                                        updates.Add($"[{field.CN}]", field.GetDefaultValue());
                                    }
                                }

                                if (updates.Count > 0)
                                {
                                    _Database.Update(TTS.DataDictionarySchema.ExtendDataTableName, updates, null, conn, trans);
                                }
                            }
                            
                            //modify fields in table
                            List<DataDictionaryField> modifyFields = template.Where(f => oldTemplate.HasField(f.CN) && !f.Equals(oldTemplate[f.CN])).ToList();
                            
                            if (modifyFields.Any())
                            {
                                foreach (DataDictionaryField field in modifyFields)
                                {
                                    if (field.DataType == oldTemplate[field.CN].DataType)
                                    {
                                        _Database.Update(TTS.DataDictionarySchema.TableName, GetDataDictionaryFieldValues(field),
                                            $"{TTS.SharedSchema.CN} = '{field.CN}'",
                                            conn, trans);

                                        if (field.ValueRequired && !oldTemplate[field.CN].ValueRequired)
                                        {
                                            _Database.Update(TTS.DataDictionarySchema.ExtendDataTableName,
                                                new Dictionary<string, object>() { { $"[{field.CN}]", field.GetDefaultValue() } },
                                                $"[{field.CN}] IS NULL",
                                                conn, trans);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("DataDictionaryField DataType Mismatch");
                                    }
                                }
                            }

                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message, "DAL:ModifyDataDictionaryTemplate");
                            trans.Rollback();
                            return false;
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
            }

            _DataDictionaryTemplate = new DataDictionaryTemplate(template);

            return true;
        }

        private Dictionary<string, object> GetDataDictionaryFieldValues(DataDictionaryField field)
        {
            return new Dictionary<string, object>()
            {
                [TTS.SharedSchema.CN] = field.CN,
                [TTS.DataDictionarySchema.Name] = field.Name,
                [TTS.DataDictionarySchema.FieldOrder] = field.Order,
                [TTS.DataDictionarySchema.FieldType] = (int)field.FieldType,
                [TTS.DataDictionarySchema.Flags] = field.Flags,
                [TTS.DataDictionarySchema.FieldValues] = field.Values != null && field.Values.Any() ? string.Join("\n", field.Values) : null,
                [TTS.DataDictionarySchema.DefaultValue] = field.DefaultValue,
                [TTS.DataDictionarySchema.ValueRequired] = field.ValueRequired,
                [TTS.DataDictionarySchema.DataType] = field.DataType
            };
        }
        
        
        public DataDictionary GetExtendedDataForPoint(string pointCN)
        {
            return GetExtendedData(
                $"{TTS.DataDictionarySchema.ExtendDataTableName}.{TTS.DataDictionarySchema.PointCN} = '{pointCN}'"
            ).First();
        }

        public IEnumerable<DataDictionary> GetExtendedData()
        {
            return GetExtendedData(null);
        }

        protected IEnumerable<DataDictionary> GetExtendedData(string where)
        {
            DataDictionaryTemplate template = GetDataDictionaryTemplate();

            if (template != null)
            {
                List<DataDictionaryField> fields = template.ToList();

                String query = $@"select { string.Join(", ", fields.Select(f => $"_{f.CN}").ToArray()) } from 
{TTS.DataDictionarySchema.ExtendDataTableName}{( where != null ? $" where { where }" : String.Empty )}";

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
                                            case DataType.TEXT:
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
        public void FixErrors(bool removeErrors = false)
        {
            if (removeErrors)
            {
                List<string> badPointCns = GetPointsWithMissingPolygons()
                    .Union(GetPointsWithMissingMetadata())
                    .Union(GetPointsWithMissingGroups()).ToList();

                DeletePoints(GetPoints().Where(p => badPointCns.Contains(p.CN)));
            }
            else
            {
                if (GetPointsWithMissingPolygons().Any())
                {
                    TtPolygon mPoly = GetPolygons($"{TTS.SharedSchema.CN} = {Consts.EmptyGuid}").FirstOrDefault() ??
                        new TtPolygon(Consts.EmptyGuid, "Fix Point Poly",
                            "Polygon that contains points that were fixed from having no polygon.",
                            Consts.DEFAULT_POINT_START_INDEX, 10, DateTime.Now, Consts.DEFAULT_POINT_ACCURACY, 0, 0, 0);

                    InsertPolygon(mPoly);

                    _Database.Update(TTS.PointSchema.TableName,
                        new Dictionary<string, string> { [TTS.PointSchema.PolyCN] = mPoly.CN, [TTS.PointSchema.PolyName] = mPoly.Name },
                        $"{TTS.PointSchema.PolyCN} not in (select {TTS.SharedSchema.CN} from {TTS.PolygonSchema.TableName});");
                }

                _Database.Update(TTS.PointSchema.TableName,
                        new Dictionary<string, string> { [TTS.PointSchema.MetadataCN] = Consts.EmptyGuid },
                        $"{TTS.PointSchema.MetadataCN} not in (select {TTS.SharedSchema.CN} from {TTS.MetadataSchema.TableName});");

                _Database.Update(TTS.PointSchema.TableName,
                        new Dictionary<string, string> { [TTS.PointSchema.GroupCN] = Consts.EmptyGuid, [TTS.PointSchema.GroupName] = Consts.DefaultGroupName },
                        $"{TTS.PointSchema.PolyCN} not in (select {TTS.SharedSchema.CN} from {TTS.GroupSchema.TableName});");
            }

            FixNullAdjLocs();
            ReindexPolygons();
        }

        protected int GetItemCount(String tableName, string where = null)
        {
            int count = -1;

            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                SQLiteDataReader dr = _Database.ExecuteReader($@"SELECT count(*) FROM { tableName }{(where != null ? $" WHERE { where }" : String.Empty)};", conn);

                try
                {
                    if (dr != null && dr.Read())
                    {
                        count = dr.GetInt32(0);
                        dr.Close();
                    }
                }
                catch
                {
                    //
                }

                conn.Close();
            }

            return count;
        }

        protected IEnumerable<string> GetItemList(string tableName, string field, string where = null)
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(
                    $"SELECT {field} from {tableName}{(where != null ? $" where {where}" : String.Empty)};", conn))
                {
                    while (dr.Read())
                    {
                        yield return dr.GetString(0);
                    }
                }
            }
        }


        public DalError GetErrors()
        {
            DalError errors = DalError.None;

            if (GetItemCount(TTS.PointSchema.TableName, $@"{ TTS.PointSchema.AdjX } IS NULL OR { TTS.PointSchema.AdjY
                        } IS NULL OR { TTS.PointSchema.AdjZ } IS NULL OR { TTS.PointSchema.Accuracy } IS NULL") > 0)
            {
                errors |= DalError.NullAdjLocs;
            }

            if (GetPolysInNeedOfReindex().Any())
            {
                errors |= DalError.PointIndexes;
            }

            if (GetPointsWithMissingMetadata().Any())
            {
                errors |= DalError.MissingMetadata;
            }

            if (GetPointsWithMissingPolygons().Any())
            {
                errors |= DalError.MissingPolygon;
            }

            if (GetPointsWithMissingMetadata().Any())
            {
                errors |= DalError.MissingGroup;
            }

            if (GetOrphanedQuondams().Any())
            {
                errors |= DalError.OrphanedQuondams;
            }

            return errors;
        }

        public bool HasErrors()
        {
            return GetErrors() > 0;
        }
        
        public IEnumerable<string> GetPolysInNeedOfReindex()
        {
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = _Database.ExecuteReader(
                    $"SELECT {TTS.PointSchema.PolyCN}, sum({TTS.PointSchema.Index})," +
                    $"Count({TTS.PointSchema.ID}) from {TTS.PointSchema.TableName} GROUP BY {TTS.PointSchema.PolyCN}", conn))
                {
                    Func<int, int> sumOfN = (n) =>
                    {
                        int sum = 0;

                        for (int num = 0; num < n; num++)
                            sum += num;

                        return sum;
                    };

                    while (dr.Read())
                    {
                        if (dr.GetInt32(1) != sumOfN(dr.GetInt32(2)))
                        {
                            yield return dr.GetString(0);
                        }
                    }
                }
            }
        }

        public IEnumerable<string> GetPointsWithMissingMetadata()
        {
            return GetItemList(TTS.PointSchema.TableName, TTS.SharedSchema.CN,
                $"{TTS.PointSchema.MetadataCN} not in (select distinct {TTS.SharedSchema.CN} from {TTS.MetadataSchema.TableName})");
        }

        public IEnumerable<string> GetPointsWithMissingPolygons()
        {
            return GetItemList(TTS.PointSchema.TableName, TTS.SharedSchema.CN,
                $"{TTS.PointSchema.PolyCN} not in (select distinct {TTS.SharedSchema.CN} from {TTS.PolygonSchema.TableName})");
        }

        public IEnumerable<string> GetPointsWithMissingGroups()
        {
            return GetItemList(TTS.PointSchema.TableName, TTS.SharedSchema.CN,
                $"{TTS.PointSchema.GroupCN} not in (select distinct {TTS.SharedSchema.CN} from {TTS.GroupSchema.TableName})");
        }

        public IEnumerable<string> GetOrphanedQuondams()
        {
            return GetItemList(TTS.QuondamPointSchema.TableName, TTS.SharedSchema.CN,
                $"{TTS.QuondamPointSchema.ParentPointCN} not in (select distinct {TTS.SharedSchema.CN} from {TTS.PointSchema.TableName})");
        }

        protected bool ReindexPolygons()
        {
            bool pointIndexesUpdated = false;

            try
            {
                List<string> reindexPolys = GetPolysInNeedOfReindex().ToList();

                if (reindexPolys.Count > 0)
                {
                    using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
                    {
                        using (SQLiteTransaction trans = conn.BeginTransaction())
                        {
                            foreach (string polyCN in reindexPolys)
                            {
                                int count = 0;

                                using (SQLiteDataReader drpoint = _Database.ExecuteReader(
                                    $"select {TTS.SharedSchema.CN},{TTS.PointSchema.Index} from {TTS.PointSchema.TableName} " +
                                    $"where {TTS.PointSchema.PolyCN} = '{polyCN}' order by {TTS.PointSchema.Index};", conn))
                                {
                                    if (drpoint != null)
                                    {
                                        while (drpoint.Read())
                                        {
                                            string cn = drpoint.GetString(0);
                                            int index = drpoint.GetInt32(1);

                                            if (index != count)
                                            {
                                                _Database.ExecuteNonQuery($"update {TTS.PointSchema.TableName} set {TTS.PointSchema.Index} = {count} where " +
                                                    $"{TTS.SharedSchema.CN} = '{cn}';", conn, trans);
                                                pointIndexesUpdated = true;
                                            }

                                            count++;
                                        }

                                        drpoint.Close();
                                    }
                                }
                            }

                            trans.Commit();
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message, "TtSqliteDataAccessLayer:ReindexPolygons");
                throw e;
            }

            return pointIndexesUpdated;
        }
        
        protected void FixNullAdjLocs()
        {
            //fix for non-adjusted points
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                _Database.Update(TTS.PointSchema.TableName,
                        new Dictionary<string, object>()
                        {
                            [TTS.PointSchema.AdjX] = 0d,
                            [TTS.PointSchema.AdjY] = 0d,
                            [TTS.PointSchema.AdjZ] = 0d,
                            [TTS.PointSchema.Accuracy] = 0d
                        }, $@"{ TTS.PointSchema.AdjX } IS NULL OR { TTS.PointSchema.AdjY
                            } IS NULL OR { TTS.PointSchema.AdjZ } IS NULL OR { TTS.PointSchema.Accuracy } IS NULL",
                        conn);

                conn.Close();
            }
        }
        #endregion
    }
}
