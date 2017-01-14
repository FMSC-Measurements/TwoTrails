using CSUtil.Databases;
using FMSC.Core;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.Types;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Media;
using TwoTrails.Core.Points;

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
                return _Version < TwoTrailsV2Schema.RequiredSchemaVersion;
            }
        }

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
                    String.Format("select {0} from {1}",
                        TwoTrailsV2Schema.ProjectInfoSchema.TtDbSchemaVersion,
                        TwoTrailsV2Schema.ProjectInfoSchema.TableName
                    ), conn))
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


        public List<TtMetadata> GetMetadata()
        {
            CheckVersion();
            
            List<TtMetadata> metas = new List<TtMetadata>();

            String query = String.Format("SELECT {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13} from {14} ",
                TwoTrailsV2Schema.MetaDataSchema.CN, //0
                TwoTrailsV2Schema.MetaDataSchema.Comment,//1
                TwoTrailsV2Schema.MetaDataSchema.Compass, //2
                TwoTrailsV2Schema.MetaDataSchema.Crew,    //3
                TwoTrailsV2Schema.MetaDataSchema.Datum,   //4
                TwoTrailsV2Schema.MetaDataSchema.DeclinationType, //5
                TwoTrailsV2Schema.MetaDataSchema.ID,      //6
                TwoTrailsV2Schema.MetaDataSchema.Laser,   //7
                TwoTrailsV2Schema.MetaDataSchema.MagDec,  //8
                TwoTrailsV2Schema.MetaDataSchema.Receiver,    //9
                TwoTrailsV2Schema.MetaDataSchema.UomDistance,     //10
                TwoTrailsV2Schema.MetaDataSchema.UomElevation,    //11
                TwoTrailsV2Schema.MetaDataSchema.UomSlope,        //12
                TwoTrailsV2Schema.MetaDataSchema.UtmZone,         //13
                TwoTrailsV2Schema.MetaDataSchema.TableName);


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
                            md.Distance = (Distance)Enum.Parse(typeof(Distance), reader.GetString(10), true);
                            md.Elevation = Types.ParseDistance(reader.GetString(11));
                            md.Slope = Types.ParseSlope(reader.GetString(12));
                            md.Zone = reader.GetInt32(13);
                            metas.Add(md);
                        }
                    }

                    reader.Close();
                }

                conn.Close();
            }

            return metas;
        }

        public List<TtPolygon> GetPolygons()
        {
            CheckVersion();

            List<TtPolygon> polys = new List<TtPolygon>();
            
            string query = String.Format("SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} from {8} ",
                TwoTrailsV2Schema.SharedSchema.CN,
                TwoTrailsV2Schema.PolygonSchema.PolyID,
                TwoTrailsV2Schema.PolygonSchema.Description,
                TwoTrailsV2Schema.PolygonSchema.Accuracy,
                TwoTrailsV2Schema.PolygonSchema.Area,
                TwoTrailsV2Schema.PolygonSchema.Perimeter,
                TwoTrailsV2Schema.PolygonSchema.IncrementBy,
                TwoTrailsV2Schema.PolygonSchema.PointStartIndex,
                TwoTrailsV2Schema.PolygonSchema.TableName);


            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader reader = database.ExecuteReader(query, conn))
                {
                    if (reader != null)
                    {
                        TtPolygon poly;
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

                            polys.Add(poly);
                        }

                        reader.Close();
                    }
                }

                conn.Close();
            }

            return polys;
        }



        public List<TtPoint> GetPoints(String polyCN = null)
        {
            return GetTtPoints(polyCN != null ? String.Format("{0} = '{1}'", TwoTrailsV2Schema.PointSchema.PolyCN, polyCN) : null);
        }

        public List<TtPoint> GetPointsUnlinked(String polyCN = null)
        {
            return GetTtPoints(polyCN != null ? String.Format("{0} = '{1}'", TwoTrailsV2Schema.PointSchema.PolyCN, polyCN) : null, false);
        }

        private List<TtPoint> GetTtPoints(String where = null, bool linkPoints = true, int limit = 0)
        {
            CheckVersion();
            {
                List<TtPoint> points = new List<TtPoint>();

                String query = String.Format(@"select {0}.{1}, {2}, {3}, {4} from {0} left join {5} on {5}.{8} = {0}.{8} 
 left join {6} on {6}.{8} = {0}.{8} left join {7} on {7}.{8} = {0}.{8}{9} order by {10} asc{11}",
                    TwoTrailsV2Schema.PointSchema.TableName,              //0
                    TwoTrailsV2Schema.PointSchema.SelectItems,            //1
                    TwoTrailsV2Schema.GpsPointSchema.SelectItemsNoCN,     //2
                    TwoTrailsV2Schema.TravPointSchema.SelectItemsNoCN,    //3
                    TwoTrailsV2Schema.QuondamPointSchema.SelectItemsNoCN, //4
                    TwoTrailsV2Schema.GpsPointSchema.TableName,           //5
                    TwoTrailsV2Schema.TravPointSchema.TableName,          //6
                    TwoTrailsV2Schema.QuondamPointSchema.TableName,       //7
                    TwoTrailsV2Schema.SharedSchema.CN,                    //8
                    where != null ? String.Format(" where {0}", where) : String.Empty,
                    TwoTrailsV2Schema.PointSchema.Order,
                    limit > 0 ? String.Format(" limit {0}", limit) : String.Empty
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

                                //acc = dr.GetDouble(16);

                                qlinks = dr.GetStringN(16);


                                if (op.IsGpsType())
                                {
                                    //lat = dr.GetDoubleN(18);
                                    //lon = dr.GetDoubleN(19);
                                    //elev = dr.GetDoubleN(20);
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
                                                    Consts.DEFAULT_POINT_ACCURACY, qlinks, pcn, manacc);

                                    if (linkPoints)
                                    {
                                        QuondamPoint qp = point as QuondamPoint;
                                        List<TtPoint> pps = GetTtPoints(String.Format("{0}.CN = '{1}'",
                                            TwoTrailsV2Schema.PointSchema.TableName, qp.ParentPointCN), false, 1);
                                        qp.ParentPoint = pps.Any() ? pps.First() : null; 
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
        }


        public List<TtGroup> GetGroups()
        {
            CheckVersion();

            List<TtGroup> groups = new List<TtGroup>();

            String query = String.Format("SELECT {0}, {1}, {2}, {3} from {4} ",
                TwoTrailsV2Schema.GroupSchema.CN,
                TwoTrailsV2Schema.GroupSchema.Name,
                TwoTrailsV2Schema.GroupSchema.Description,
                TwoTrailsV2Schema.GroupSchema.Type,
                TwoTrailsV2Schema.GroupSchema.TableName);


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

                            groups.Add(group);
                        }

                        dr.Close();
                    }
                }

                conn.Close();
            }

            return groups;
        }


        public List<TtNmeaBurst> GetNmeaBursts(String pointCN = null)
        {
            CheckVersion();

            List<TtNmeaBurst> bursts = new List<TtNmeaBurst>();

            String query = String.Format(@"select {0} from {1}{2}",
                TwoTrailsV2Schema.TtNmeaSchema.SelectItems,
                TwoTrailsV2Schema.TtNmeaSchema.TableName,
                pointCN != null ?
                String.Format(" where {0} = '{1}'",
                        TwoTrailsV2Schema.TtNmeaSchema.PointCN,
                        pointCN) :
                String.Empty
            );

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
                            
                            bursts.Add(new TtNmeaBurst(
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
                                (Fix)(dr.GetInt32(14) - 1),     //converts from real value
                                ParseIds(dr.GetString(25)),
                                dr.GetDouble(15),
                                dr.GetDouble(16),
                                dr.GetDouble(17),
                                (GpsFixType)(dr.GetInt32(13)),  //original file type had wrong field name
                                0,
                                dr.GetDouble(19),
                                dr.GetDouble(18),
                                UomElevationExtensions.Parse(dr.GetString(20)),
                                dr.GetInt32(24)
                            ));
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return bursts;
        }


        public TtProjectInfo GetProjectInfo()
        {
            CheckVersion();

            String query = String.Format("SELECT {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} from {8}",
                TwoTrailsV2Schema.ProjectInfoSchema.ID,
                TwoTrailsV2Schema.ProjectInfoSchema.Description,
                TwoTrailsV2Schema.ProjectInfoSchema.Region,
                TwoTrailsV2Schema.ProjectInfoSchema.Forest,
                TwoTrailsV2Schema.ProjectInfoSchema.District,
                TwoTrailsV2Schema.ProjectInfoSchema.TtVersion,
                TwoTrailsV2Schema.ProjectInfoSchema.DeviceID,
                TwoTrailsV2Schema.ProjectInfoSchema.Year,
                TwoTrailsV2Schema.ProjectInfoSchema.TableName);

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


        public List<TtImage> GetPictures(String pointCN)
        {
            return new List<TtImage>();
        }


        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            return new List<PolygonGraphicOptions>();
        }


        public List<TtUserActivity> GetUserActivity()
        {
            return new List<TtUserActivity>();
        }


        public Boolean HasPolygons()
        {
            CheckVersion();

            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(String.Format("select count(*) from {0}", TwoTrailsV2Schema.PolygonSchema.TableName), conn))
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
    }
}
