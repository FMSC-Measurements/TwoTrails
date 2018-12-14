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
                database.ExecuteNonQuery(TwoTrailsSchema.PointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.PolygonSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.GroupSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.MetadataSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.GpsPointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.TravPointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.QuondamPointSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.ProjectInfoSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.TtNmeaSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.PolygonAttrSchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.ActivitySchema.CreateTable, conn);
                database.ExecuteNonQuery(TwoTrailsSchema.DataDictionarySchema.CreateTable, conn);
            }

            TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(database);
            dal.InsertProjectInfo(projectInfo);

            return dal;
        }


        public bool RequiresUpgrade
        {
            get { return GetDataVersion() < TwoTrailsSchema.SchemaVersion; }
        }

        public bool HandlesAllPointTypes => true;



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
            string ddQuerySelect = null, ddQueryTable = null;
            
            List<DataDictionaryField> fields = GetDataDictionaryTemplate()?.ToList();

            if (fields != null && fields.Any())
            {
                ddQueryTable = String.Format(" left join {0} on {0}.{1} = {2}.{3}",
                    TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName,
                    TwoTrailsSchema.DataDictionarySchema.PointCN,
                    TwoTrailsSchema.PointSchema.TableName,
                    TwoTrailsSchema.SharedSchema.CN);

                ddQuerySelect = $", {( string.Join(", ", fields.Select(f => $"[{ f.CN }]").ToArray()) )}"; 
            }

            String query = String.Format(@"select {0}.{1}, {2}, {3}, {4}{5} from {0} left join {6} on {6}.{10} = {0}.{10} 
 left join {7} on {7}.{10} = {0}.{10} left join {8} on {8}.{10} = {0}.{10}{9}{11} order by {12} asc",
                TwoTrailsSchema.PointSchema.TableName,              //0
                TwoTrailsSchema.PointSchema.SelectItems,            //1
                TwoTrailsSchema.GpsPointSchema.SelectItemsNoCN,     //2
                TwoTrailsSchema.TravPointSchema.SelectItemsNoCN,    //3
                TwoTrailsSchema.QuondamPointSchema.SelectItemsNoCN, //4
                ddQuerySelect,                                      //5
                TwoTrailsSchema.GpsPointSchema.TableName,           //6
                TwoTrailsSchema.TravPointSchema.TableName,          //7
                TwoTrailsSchema.QuondamPointSchema.TableName,       //8
                ddQueryTable,                                       //9
                TwoTrailsSchema.SharedSchema.CN,                    //10
                where != null ? $" where {where}" : String.Empty,   //11
                TwoTrailsSchema.PointSchema.Index                   //12
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
            _Database.Insert(TwoTrailsSchema.PointSchema.TableName, GetBasePointValues(point), conn, transaction);
        }
        
        private void InsertExtendedData(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            if (point.ExtendedData != null)
                _Database.Insert(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName, GetExtendedPointValues(point, true), conn, transaction);
        }

        private void InsertPointTypeData(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction)
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
                        string where = $"{TwoTrailsSchema.SharedSchema.CN} = '{point.CN}'";

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
                            string where = $"{TwoTrailsSchema.SharedSchema.CN} = '{point.Item1.CN}'";

                            UpdateBasePoint(point.Item1, point.Item2, conn, trans, where);
                            UpdatePointTypeData(point.Item1, point.Item2, conn, trans, where);

                            if (hasDD)
                                UpdateExtendedData(point.Item1, conn, trans, $"{TwoTrailsSchema.DataDictionarySchema.PointCN} = '{point.Item1.CN}'");
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
            _Database.Update(TwoTrailsSchema.PointSchema.TableName,
                GetBasePointValues(point),
                where,
                conn,
                transaction);
        }

        private void UpdateExtendedData(TtPoint point, SQLiteConnection conn, SQLiteTransaction transaction, string where)
        {
            if (point.ExtendedData != null)
                _Database.Update(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName, GetExtendedPointValues(point), where, conn, transaction);
        }

        private void UpdatePointTypeData(TtPoint point, TtPoint oldPoint, SQLiteConnection conn, SQLiteTransaction transaction, string where)
        {
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
        }

        public bool ChangePointOp(TtPoint point, TtPoint oldPoint)
        {
            return UpdatePoint(point, oldPoint);
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

        private Dictionary<string, object> GetExtendedPointValues(TtPoint point, bool insert = false)
        {
            if (insert)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    [TwoTrailsSchema.DataDictionarySchema.PointCN] = point.CN
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
                            if (HasDataDictionary)
                                DeleteExtendedData(where, conn, trans);

                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, conn, trans);
                                    DeleteNmeaBursts($"{TwoTrailsSchema.TtNmeaSchema.PointCN} = '{point.CN}'", conn, trans);
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

        //todo optimize
        public int DeletePoints(IEnumerable<TtPoint> points)
        {
            StringBuilder sb = new StringBuilder();
            int remaining = points.Count();
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
                            
                            sb.Append($"{where}{(count < remaining ? " or " : String.Empty)}");

                            if (HasDataDictionary)
                                DeleteExtendedData(where, conn, trans);

                            switch (point.OpType)
                            {
                                case OpType.GPS:
                                case OpType.Take5:
                                case OpType.Walk:
                                case OpType.WayPoint:
                                    DeleteGpsPointData(where, conn, trans);
                                    DeleteNmeaBursts($"{TwoTrailsSchema.TtNmeaSchema.PointCN} = '{point.CN}'", conn, trans);
                                    break;
                                case OpType.Traverse:
                                case OpType.SideShot:
                                    DeleteTravPointData(where, conn, trans);
                                    break;
                                case OpType.Quondam:
                                    DeleteQndmPointData(where, conn, trans);
                                    break;
                            }

                            if (count == 50)
                            {
                                if (!DeleteBasePoints(sb.ToString(), conn, trans))
                                {
                                    trans.Rollback();
                                    return -1;
                                }

                                sb.Clear();

                                remaining -= 50;
                                count = 0;
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

            return remaining;
        } 


        private bool DeleteBasePoints(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            return _Database.Delete(TwoTrailsSchema.PointSchema.TableName, where, conn, transaction) > 0;
        }

        private void DeleteExtendedData(String where, SQLiteConnection conn, SQLiteTransaction transaction)
        {
            _Database.Delete(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName, where, conn, transaction);
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
            return GetNmeaBursts(new string[] { pointCN });
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(IEnumerable<string> pointCNs)
        {
            String query = $@"select {TwoTrailsSchema.TtNmeaSchema.SelectItems} from {TwoTrailsSchema.TtNmeaSchema.TableName} 
            {(pointCNs == null ? String.Empty : $" where { String.Join(" OR ", pointCNs.Select(pcn => $"{TwoTrailsSchema.TtNmeaSchema.PointCN} = '{pcn}'"))}")}";

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
                        DeleteNmeaBursts($"{TwoTrailsSchema.TtNmeaSchema.PointCN} = '{pointCN}'", conn, trans);
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
            _Database.Delete(TwoTrailsSchema.TtNmeaSchema.TableName, where, conn, transaction);
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

        private bool InsertProjectInfo(TtProjectInfo info)
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
                                [TwoTrailsSchema.ProjectInfoSchema.District] = info.District?.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.Forest] = info.Forest?.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.Region] = info.Region?.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.DeviceID] = info.CreationDeviceID?.Trim(),
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
                                [TwoTrailsSchema.ProjectInfoSchema.District] = info.District?.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.Forest] = info.Forest?.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.Region] = info.Region?.Trim(),
                                [TwoTrailsSchema.ProjectInfoSchema.DeviceID] = info.CreationDeviceID?.Trim(),
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
                [TwoTrailsSchema.ActivitySchema.ActivityType] = (int)activity.Action,
                [TwoTrailsSchema.ActivitySchema.ActivityNotes] = activity.Notes
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
            if (_DataDictionaryTemplate == null && _Database.TableExists(TwoTrailsSchema.DataDictionarySchema.TableName))
            {
                _DataDictionaryTemplate = new DataDictionaryTemplate();

                String query = $"select {TwoTrailsSchema.DataDictionarySchema.SelectItems} from {TwoTrailsSchema.DataDictionarySchema.TableName}";

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
            ddFields.Add(TwoTrailsSchema.DataDictionarySchema.PointCN, SQLiteDataType.TEXT);

            _Database.ClearTable(TwoTrailsSchema.DataDictionarySchema.TableName, conn, trans);

            foreach (DataDictionaryField field in template)
            {
                _Database.Insert(TwoTrailsSchema.DataDictionarySchema.TableName, GetDataDictionaryFieldValues(field), conn, trans);

                ddFields.Add($"[{field.CN}]", field.DataType == DataType.BOOLEAN ? SQLiteDataType.INTEGER : (SQLiteDataType)(int)field.DataType);

                if (field.ValueRequired || field.DefaultValue != null)
                {
                    defaultValues.Add($"[{field.CN}]", field.GetDefaultValue());
                }
            }

            if (_Database.TableExists(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName))
            {
                Trace.WriteLine("Overwritting DataDictionary", "DAL:UpdateDataDictionaryTemplate");
                _Database.DropTable(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName, conn, trans);
            }

            _Database.CreateTable(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName, ddFields, TwoTrailsSchema.DataDictionarySchema.PointCN, conn, trans);

            if (defaultValues.Count > 0)
            {
                _Database.ExecuteNonQuery(
                    $"INSERT INTO {TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName} ({TwoTrailsSchema.DataDictionarySchema.PointCN}) SELECT ({TwoTrailsSchema.SharedSchema.CN}) FROM {TwoTrailsSchema.PointSchema.TableName};", conn, trans);

                _Database.Update(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName, defaultValues, null, conn, trans);
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
                                if (_Database.TableExists(TwoTrailsSchema.DataDictionarySchema.TempExtendDataTableName))
                                    _Database.DropTable(TwoTrailsSchema.DataDictionarySchema.TempExtendDataTableName, conn, trans);
                                
                                _Database.ExecuteNonQuery(
                                    $"ALTER TABLE {TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName} RENAME TO {TwoTrailsSchema.DataDictionarySchema.TempExtendDataTableName};",
                                    conn, trans);


                                IEnumerable<DataDictionaryField> oldFields = template.Where(f => oldTemplate.HasField(f.CN));
                                string oldFieldsStr = String.Join(", ", oldFields.Select(f => $"[{f.CN}]"));
                                
                                _Database.ExecuteNonQuery(
                                    $"CREATE TABLE {TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName} AS SELECT {TwoTrailsSchema.DataDictionarySchema.PointCN}, {oldFieldsStr} FROM {TwoTrailsSchema.DataDictionarySchema.TempExtendDataTableName};",
                                    conn, trans);

                                _Database.DropTable(TwoTrailsSchema.DataDictionarySchema.TempExtendDataTableName, conn, trans);

                                _Database.Delete(TwoTrailsSchema.DataDictionarySchema.TableName,
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
                                    _Database.Insert(TwoTrailsSchema.DataDictionarySchema.TableName, GetDataDictionaryFieldValues(field), conn, trans);
                                        
                                    _Database.ExecuteNonQuery($@"ALTER TABLE {TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName} ADD [{field.CN}] {convertDataType(field.DataType).ToString()}", conn, trans);

                                    if (field.DefaultValue != null)
                                    {
                                        updates.Add($"[{field.CN}]", field.GetDefaultValue());
                                    }
                                }

                                if (updates.Count > 0)
                                {
                                    _Database.Update(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName, updates, null, conn, trans);
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
                                        _Database.Update(TwoTrailsSchema.DataDictionarySchema.TableName, GetDataDictionaryFieldValues(field),
                                            $"{TwoTrailsSchema.SharedSchema.CN} = '{field.CN}'",
                                            conn, trans);

                                        if (field.ValueRequired && !oldTemplate[field.CN].ValueRequired)
                                        {
                                            _Database.Update(TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName,
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
                [TwoTrailsSchema.SharedSchema.CN] = field.CN,
                [TwoTrailsSchema.DataDictionarySchema.Name] = field.Name,
                [TwoTrailsSchema.DataDictionarySchema.FieldOrder] = field.Order,
                [TwoTrailsSchema.DataDictionarySchema.FieldType] = (int)field.FieldType,
                [TwoTrailsSchema.DataDictionarySchema.Flags] = field.Flags,
                [TwoTrailsSchema.DataDictionarySchema.FieldValues] = field.Values != null && field.Values.Any() ? string.Join("\n", field.Values) : null,
                [TwoTrailsSchema.DataDictionarySchema.DefaultValue] = field.DefaultValue,
                [TwoTrailsSchema.DataDictionarySchema.ValueRequired] = field.ValueRequired,
                [TwoTrailsSchema.DataDictionarySchema.DataType] = field.DataType
            };
        }
        
        
        public DataDictionary GetExtendedDataForPoint(string pointCN)
        {
            return GetExtendedData(
                $"{TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName}.{TwoTrailsSchema.DataDictionarySchema.PointCN} = '{pointCN}'"
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
{TwoTrailsSchema.DataDictionarySchema.ExtendDataTableName}{( where != null ? $" where { where }" : String.Empty )}";

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
        public void Clean()
        {
            Dictionary<string, TtPolygon> polyCNs = GetPolygons().ToDictionary(p => p.CN, p => p);

            IEnumerable<TtPoint> delPoints = GetPoints().Where(p => !polyCNs.ContainsKey(p.PolygonCN));

            DeletePoints(delPoints);
        }

        public void Fix()
        {
            //fix for non-adjusted points
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                _Database.Update(TwoTrailsSchema.PointSchema.TableName,
                        new Dictionary<string, object>()
                        {
                            [TwoTrailsSchema.PointSchema.AdjX] = 0d,
                            [TwoTrailsSchema.PointSchema.AdjY] = 0d,
                            [TwoTrailsSchema.PointSchema.AdjZ] = 0d,
                            [TwoTrailsSchema.PointSchema.Accuracy] = 0d
                        }, $@"{ TwoTrailsSchema.PointSchema.AdjX } IS NULL OR { TwoTrailsSchema.PointSchema.AdjY
                            } IS NULL OR { TwoTrailsSchema.PointSchema.AdjZ } IS NULL OR { TwoTrailsSchema.PointSchema.Accuracy } IS NULL",
                        conn);

                conn.Close();
            }
        }

        public bool HasErrors()
        {
            bool errors = false;

            //check for non-adjusted points
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                SQLiteDataReader dr = _Database.ExecuteReader($@"SELECT count(*) FROM { TwoTrailsSchema.PointSchema.TableName
                        } WHERE { TwoTrailsSchema.PointSchema.AdjX } IS NULL OR { TwoTrailsSchema.PointSchema.AdjY
                        } IS NULL OR { TwoTrailsSchema.PointSchema.AdjZ } IS NULL OR { TwoTrailsSchema.PointSchema.Accuracy } IS NULL;", conn);

                try
                {
                    if (dr != null && dr.Read())
                    {
                        errors = dr.GetInt32(0) > 0;
                        dr.Close();
                    }
                }
                catch
                {
                    errors = true;
                }

                conn.Close();
            }

            return GetItemCount(TwoTrailsSchema.PointSchema.TableName, $@"{ TwoTrailsSchema.PointSchema.AdjX } IS NULL OR { TwoTrailsSchema.PointSchema.AdjY
                        } IS NULL OR { TwoTrailsSchema.PointSchema.AdjZ } IS NULL OR { TwoTrailsSchema.PointSchema.Accuracy } IS NULL") > 0;
        }

        protected int GetItemCount(String tableName, string where = null)
        {
            int count = -1;
            
            using (SQLiteConnection conn = _Database.CreateAndOpenConnection())
            {
                SQLiteDataReader dr = _Database.ExecuteReader($@"SELECT count(*) FROM { tableName }{( where != null ? $" WHERE { where }" : String.Empty )};", conn);

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
        #endregion
    }
}
