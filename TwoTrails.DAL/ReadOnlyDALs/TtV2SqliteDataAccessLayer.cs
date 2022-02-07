using FMSC.Core;
using FMSC.Core.Databases;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.Types;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;
using TTV2S = TwoTrails.DAL.TwoTrailsV2Schema;

namespace TwoTrails.DAL
{
    public class TtV2SqliteDataAccessLayer : IReadOnlyTtDataLayer
    {
        public String FilePath { get; }

        private Version _Version;
        public Boolean RequiresUpgrade
        {
            get
            {
                if (_Version == null)
                    _Version = GetDataVersion();
                return _Version < TTV2S.RequiredSchemaVersion;
            }
        }

        public bool HandlesAllPointTypes => true;

        private SQLiteDatabase database;


        public TtV2SqliteDataAccessLayer(string filePath)
        {
            FilePath = filePath;
            database = new SQLiteDatabase(FilePath);
        }

        private void CheckVersion()
        {
            if (RequiresUpgrade)
                throw new Exception("File version is not supported");
        }


        public Version GetDataVersion()
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(
                    $"select {TTV2S.ProjectInfoSchema.TtDbSchemaVersion} from {TTV2S.ProjectInfoSchema.TableName}", conn))
                {
                    if (dr != null)
                    {
                        if (dr.Read())
                        {
                            return new Version(dr.GetString(0));
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return new Version("0.0.0");
        }


        public IEnumerable<TtMetadata> GetMetadata()
        {
            CheckVersion();

            String query = String.Format("SELECT {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13} from {14} ",
                TTV2S.MetaDataSchema.CN, //0
                TTV2S.MetaDataSchema.Comment,//1
                TTV2S.MetaDataSchema.Compass, //2
                TTV2S.MetaDataSchema.Crew,    //3
                TTV2S.MetaDataSchema.Datum,   //4
                TTV2S.MetaDataSchema.DeclinationType, //5
                TTV2S.MetaDataSchema.ID,      //6
                TTV2S.MetaDataSchema.Laser,   //7
                TTV2S.MetaDataSchema.MagDec,  //8
                TTV2S.MetaDataSchema.Receiver,    //9
                TTV2S.MetaDataSchema.UomDistance,     //10
                TTV2S.MetaDataSchema.UomElevation,    //11
                TTV2S.MetaDataSchema.UomSlope,        //12
                TTV2S.MetaDataSchema.UtmZone,         //13
                TTV2S.MetaDataSchema.TableName);


            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader reader = database.ExecuteReader(query, conn))
                {
                    if (reader != null)
                    {
                        TtMetadata md;
                        while (reader.Read())
                        {
                            md = new TtMetadata();
                            md.CN = reader.GetString(0);
                            if (!reader.IsDBNull(1))
                                md.Comment = reader.GetString(1);
                            if (!reader.IsDBNull(2))
                                md.Compass = reader.GetString(2);
                            if (!reader.IsDBNull(3))
                                md.Crew = reader.GetString(3);
                            if (!reader.IsDBNull(4))
                                md.Datum = Types.ParseDatum(reader.GetString(4));
                            if (!reader.IsDBNull(5))
                                md.DecType = Types.ParseDeclinationType(reader.GetString(5));
                            md.Name = reader.GetString(6);
                            if (!reader.IsDBNull(7))
                                md.RangeFinder = reader.GetString(7);
                            if (!reader.IsDBNull(8))
                                md.MagDec = reader.GetDouble(8);
                            if (!reader.IsDBNull(9))
                                md.GpsReceiver = reader.GetString(9);
                            md.Distance = Types.ParseDistance(reader.GetString(10));
                            md.Elevation = Types.ParseDistance(reader.GetString(11));
                            md.Slope = Types.ParseSlope(reader.GetString(12));
                            md.Zone = reader.GetInt32(13);
                            yield return md;
                        }
                    }

                    reader.Close();
                }

                conn.Close();
            }
        }

        public IEnumerable<TtPolygon> GetPolygons()
        {
            CheckVersion();
            
            string query = String.Format("SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} from {8} ",
                TTV2S.SharedSchema.CN,
                TTV2S.PolygonSchema.PolyID,
                TTV2S.PolygonSchema.Description,
                TTV2S.PolygonSchema.Accuracy,
                TTV2S.PolygonSchema.Area,
                TTV2S.PolygonSchema.Perimeter,
                TTV2S.PolygonSchema.IncrementBy,
                TTV2S.PolygonSchema.PointStartIndex,
                TTV2S.PolygonSchema.TableName);


            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader reader = database.ExecuteReader(query, conn))
                {
                    if (reader != null)
                    {
                        TtPolygon poly;
                        int milliSeconds = 0;

                        while (reader.Read())
                        {
                            poly = new TtPolygon();
                            poly.CN = reader.GetString(0);
                            poly.Name = reader.GetString(1);
                            if (!reader.IsDBNull(2))
                                poly.Description = reader.GetString(2);
                            if (!reader.IsDBNull(3))
                                poly.Accuracy = reader.GetDouble(3);
                            if (!reader.IsDBNull(4))
                                poly.Area = reader.GetDouble(4);
                            if (!reader.IsDBNull(5))
                                poly.Perimeter = reader.GetDouble(5);
                            if (!reader.IsDBNull(6))
                                poly.Increment = reader.GetInt32(6);
                            if (!reader.IsDBNull(7))
                                poly.PointStartIndex = reader.GetInt32(7);

                            poly.TimeCreated = DateTime.Now.AddMilliseconds(milliSeconds++);

                            yield return poly;
                        }

                        reader.Close();
                    }
                }

                conn.Close();
            }
        }
        
        public TtPoint GetPoint(String cn, bool linked = false)
        {
            return GetTtPoints($"{TTV2S.PointSchema.TableName}.{TTV2S.SharedSchema.CN} = '{cn}'", linked).FirstOrDefault();
        }

        public IEnumerable<TtPoint> GetPoints(String polyCN = null, bool linked = false)
        {
            return GetTtPoints(polyCN != null ? $"{TTV2S.PointSchema.PolyCN} = '{polyCN}'" : null, linked);
        }

        private IEnumerable<TtPoint> GetTtPoints(String where = null, bool linkPoints = true, int limit = 0)
        {
            CheckVersion();
            String query = String.Format(@"select {0}.{1}, {2}, {3}, {4} from {0} left join {5} on {5}.{8} = {0}.{8} 
left join {6} on {6}.{8} = {0}.{8} left join {7} on {7}.{8} = {0}.{8}{9} order by {10} asc{11}",
                TTV2S.PointSchema.TableName,              //0
                TTV2S.PointSchema.SelectItems,            //1
                TTV2S.GpsPointSchema.SelectItemsNoCN,     //2
                TTV2S.TravPointSchema.SelectItemsNoCN,    //3
                TTV2S.QuondamPointSchema.SelectItemsNoCN, //4
                TTV2S.GpsPointSchema.TableName,           //5
                TTV2S.TravPointSchema.TableName,          //6
                TTV2S.QuondamPointSchema.TableName,       //7
                TTV2S.SharedSchema.CN,                    //8
                where != null ? $" where {where}" : String.Empty,
                TTV2S.PointSchema.Order,
                limit > 0 ? $" limit {limit}" : String.Empty
            );

            Dictionary<string, TtPolygon> polygons = null;
            Dictionary<string, TtMetadata> metadata = null;
            Dictionary<string, TtGroup> groups = null;

            if (linkPoints)
            {
                polygons = GetPolygons().ToDictionary(p => p.CN, p => p);
                metadata = GetMetadata().ToDictionary(m => m.CN, m => m);
                groups = GetGroups().ToDictionary(g => g.CN, g => g); 
            }

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
                        double adjx, adjy, adjz, unadjx, unadjy, unadjz, sd, sa;
                        double? manacc, rmser, fw, bk;

                        while (dr.Read())
                        {
                            if (dr.IsDBNull(7))
                                throw new Exception("Point has no OpType");

                            cn = dr.GetString(0);
                            index = dr.GetInt32(1);
                            pid = dr.GetInt32(2);
                            polycn = dr.GetString(3);
                            groupcn = dr.GetString(4);
                            onbnd = Boolean.Parse(dr.GetString(5));
                            comment = dr.GetStringN(6);
                            op = TtTypes.ParseOpType(dr.GetString(7));
                            metacn = dr.GetString(8);
                            time = TtCoreUtils.ParseTime(dr.GetString(9));

                            adjx = dr.GetDoubleN(10) ?? 0;
                            adjy = dr.GetDoubleN(11) ?? 0;
                            adjz = dr.GetDoubleN(12) ?? 0;

                            unadjx = dr.GetDouble(13);
                            unadjy = dr.GetDouble(14);
                            unadjz = dr.GetDouble(15);

                            qlinks = dr.GetStringN(16);


                            if (op.IsGpsType())
                            {
                                manacc = dr.GetDoubleN(17);
                                rmser = dr.GetDoubleN(18);

                                switch (op)
                                {
                                    case OpType.GPS:
                                        point = new GpsPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    Consts.DEFAULT_POINT_ACCURACY, qlinks, null, null, null, manacc, rmser);
                                        break;
                                    case OpType.Take5:
                                        point = new Take5Point(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    Consts.DEFAULT_POINT_ACCURACY, qlinks, null, null, null, manacc, rmser);
                                        break;
                                    case OpType.Walk:
                                        point = new WalkPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    Consts.DEFAULT_POINT_ACCURACY, qlinks, null, null, null, manacc, rmser);
                                        break;
                                    case OpType.WayPoint:
                                        point = new WayPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                    comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                    Consts.DEFAULT_POINT_ACCURACY, qlinks, null, null, null, manacc, rmser);
                                        break;
                                }

                                if (linkPoints)
                                {
                                    point.Polygon = polygons[point.PolygonCN];
                                    point.Metadata = metadata[point.MetadataCN];
                                    point.Group = groups[point.GroupCN];
                                }
                            }
                            else if (op.IsTravType())
                            {
                                fw = dr.GetDoubleN(19);
                                bk = dr.GetDoubleN(20);
                                sd = dr.GetDouble(21);
                                sa = dr.GetDouble(22);

                                if (op == OpType.Traverse)
                                {
                                    point = new TravPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                Consts.DEFAULT_POINT_ACCURACY, qlinks, fw, bk, sd, sa);
                                }
                                else
                                {
                                    point = new SideShotPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                Consts.DEFAULT_POINT_ACCURACY, qlinks, fw, bk, sd, sa);
                                }
                            }
                            else if (op == OpType.Quondam)
                            {
                                pcn = dr.GetString(23);
                                manacc = dr.GetDoubleN(24);

                                point = new QuondamPoint(cn, index, pid, time, polycn, metacn, groupcn,
                                                comment, onbnd, adjx, adjy, adjz, unadjx, unadjy, unadjz,
                                                Consts.DEFAULT_POINT_ACCURACY, qlinks, pcn,
                                                (manacc == 0d) ? null : manacc);

                                if (linkPoints)
                                {
                                    QuondamPoint qp = point as QuondamPoint;
                                    qp.ParentPoint = GetTtPoints($"{TTV2S.PointSchema.TableName}.{TTV2S.SharedSchema.CN} = '{qp.ParentPointCN}'", false).FirstOrDefault();
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


        public IEnumerable<TtGroup> GetGroups()
        {
            CheckVersion();

            String query = String.Format("SELECT {0}, {1}, {2}, {3} from {4} ",
                TTV2S.GroupSchema.CN,
                TTV2S.GroupSchema.Name,
                TTV2S.GroupSchema.Description,
                TTV2S.GroupSchema.Type,
                TTV2S.GroupSchema.TableName);


            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        while (dr.Read())
                        {
                            TtGroup group = new TtGroup();
                            group.CN = dr.GetString(0);
                            group.Name = dr.GetString(1);
                            if (!dr.IsDBNull(2))
                                group.Description = dr.GetString(2);
                            if (!dr.IsDBNull(3))
                                group.GroupType = (GroupType)Enum.Parse(typeof(GroupType), dr.GetString(3), true);

                            yield return group;
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }
        }


        public IEnumerable<TtNmeaBurst> GetNmeaBursts(string pointCN = null)
        {
            return GetNmeaBursts(pointCN != null ? new string[] { pointCN } : null);
        }

        public IEnumerable<TtNmeaBurst> GetNmeaBursts(IEnumerable<String> pointCNs)
        {
            CheckVersion();

            String query = $@"select {TTV2S.TtNmeaSchema.SelectItems} from {TTV2S.TtNmeaSchema.TableName} 
            { (pointCNs == null ? String.Empty : $" where { String.Join(" OR ", pointCNs.Select(pcn => $"{TwoTrailsSchema.TtNmeaSchema.PointCN} = '{pcn}'"))}")}";

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
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
                            DateTime time = TtCoreUtils.ParseTime(dr.GetString(3));

                            Func<int?, Fix> parseFix = (val) => val == null ? Fix.NoFix : (Fix)(val + 1);

                            yield return new TtNmeaBurst(
                                dr.GetString(0),
                                time,
                                dr.GetString(1),
                                dr.GetBoolean(2),
                                new GeoPosition(
                                    dr.GetDouble(4), NorthSouthExtentions.Parse(dr.GetString(5)),
                                    dr.GetDouble(6), EastWestExtentions.Parse(dr.GetString(7)),
                                    dr.GetDouble(8), UomElevationExtensions.Parse(dr.GetString(9))
                                ),
                                time,
                                dr.GetDouble(21),
                                dr.GetDouble(22),
                                dr.GetDouble(10),
                                EastWestExtentions.Parse(dr.GetString(11)),
                                (Mode)(dr.GetInt32N(12) ?? 0),
                                parseFix(dr.GetInt32(14) - 1),     //converts from real value
                                ParseIds(dr.GetString(25)),
                                dr.GetDouble(15),
                                dr.GetDouble(16),
                                dr.GetDouble(17),
                                (GpsFixType)(dr.GetInt32(13)),  //original file type had wrong field name
                                0,
                                dr.GetDouble(19),
                                dr.GetDouble(18),
                                UomElevationExtensions.Parse(dr.GetString(20)),
                                dr.GetInt32(24),
                                String.Empty
                            );
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
        }


        public TtProjectInfo GetProjectInfo()
        {
            CheckVersion();

            String query = String.Format("SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} from {8}",
                TTV2S.ProjectInfoSchema.ID,
                TTV2S.ProjectInfoSchema.Description,
                TTV2S.ProjectInfoSchema.Region,
                TTV2S.ProjectInfoSchema.Forest,
                TTV2S.ProjectInfoSchema.District,
                TTV2S.ProjectInfoSchema.TtVersion,
                TTV2S.ProjectInfoSchema.DeviceID,
                TTV2S.ProjectInfoSchema.Year,
                TTV2S.ProjectInfoSchema.TableName);

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query, conn))
                {
                    if (dr != null)
                    {
                        if (dr.Read())
                        {
                            string year = dr.GetStringN(7);
                            return new TtProjectInfo(
                                dr.GetStringN(0),
                                dr.GetStringN(1),
                                dr.GetStringN(2),
                                dr.GetStringN(3),
                                dr.GetStringN(4),
                                dr.GetStringN(5),
                                dr.GetStringN(5),
                                GetDataVersion(),
                                dr.GetString(6),
                                new DateTime(year != null ? int.Parse(year) : DateTime.Now.Year, 1, 1));
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            throw new Exception("No Project Information");
        }


        public IEnumerable<TtImage> GetPictures(String pointCN)
        {
            return new List<TtImage>();
        }


        public IEnumerable<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return new List<PolygonGraphicOptions>();
        }


        public IEnumerable<TtUserAction> GetUserActivity()
        {
            return new List<TtUserAction>();
        }


        public DataDictionaryTemplate GetDataDictionaryTemplate()
        {
            return new DataDictionaryTemplate();
        }

        public DataDictionary GetExtendedDataForPoint(string pointCN)
        {
            return new DataDictionary(pointCN);
        }

        public IEnumerable<DataDictionary> GetExtendedData()
        {
            return new List<DataDictionary>();
        }


        protected int GetItemCount(String tableName, string where = null)
        {
            int count = -1;

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                SQLiteDataReader dr = database.ExecuteReader($@"SELECT count(*) FROM { tableName }{(where != null ? $" WHERE { where }" : String.Empty)};", conn);

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

        public int GetPointCount(params string[] polyCNs)
        {
            if (polyCNs == null || !polyCNs.Any())
            {
                return GetItemCount(TwoTrailsV2Schema.PointSchema.TableName);
            }
            else
            {
                return polyCNs.Sum(cn => GetItemCount(TwoTrailsV2Schema.PointSchema.TableName, $"{TwoTrailsV2Schema.PointSchema.PolyCN} == '{cn}'"));
            }
        }

        protected IEnumerable<string> GetItemList(string tableName, string field, string where = null)
        {
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(
                    $"SELECT {field} from {tableName}{(where != null ? $" where {where}" : String.Empty)};", conn))
                {
                    while (dr.Read())
                    {
                        yield return dr.GetString(0);
                    }
                }
            }
        }


        public bool HasPolygons()
        {
            CheckVersion();

            return GetItemCount(TTV2S.PolygonSchema.TableName) > 0;
        }

        
        public DalError GetErrors()
        {
            DalError errors = DalError.None;

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
            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(
                    $"SELECT {TTV2S.PointSchema.PolyCN}, sum({TTV2S.PointSchema.Order})," +
                    $"Count({TTV2S.PointSchema.ID}) from {TTV2S.PointSchema.TableName} GROUP BY {TTV2S.PointSchema.PolyCN}", conn))
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
            return GetItemList(TTV2S.PointSchema.TableName, TTV2S.SharedSchema.CN,
                $"{TTV2S.PointSchema.MetaDataID} not in (select distinct {TTV2S.SharedSchema.CN} from {TTV2S.MetaDataSchema.TableName})");
        }

        public IEnumerable<string> GetPointsWithMissingPolygons()
        {
            return GetItemList(TTV2S.PointSchema.TableName, TTV2S.SharedSchema.CN,
                $"{TTV2S.PointSchema.PolyCN} not in (select distinct {TTV2S.SharedSchema.CN} from {TTV2S.PolygonSchema.TableName})");
        }

        public IEnumerable<string> GetPointsWithMissingGroups()
        {
            return GetItemList(TTV2S.PointSchema.TableName, TTV2S.SharedSchema.CN,
                $"{TTV2S.PointSchema.GroupCN} not in (select distinct {TTV2S.SharedSchema.CN} from {TTV2S.GroupSchema.TableName})");
        }

        public IEnumerable<string> GetOrphanedQuondams()
        {
            return GetItemList(TTV2S.QuondamPointSchema.TableName, TTV2S.SharedSchema.CN,
                $"{TTV2S.QuondamPointSchema.ParentPointCN} not in (select distinct {TTV2S.SharedSchema.CN} from {TTV2S.PointSchema.TableName})");
        }
    }
}
