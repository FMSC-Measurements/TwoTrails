using CSUtil.Databases;
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

        public Boolean RequiresUpgrade { get { return GetDataVersion() < TwoTrailsV2Schema.RequiredSchemaVersion; } }

        private SQLiteDatabase database;


        public TtV2SqliteDataAccessLayer(string filePath)
        {
            FilePath = filePath;
            database = new SQLiteDatabase(FilePath);
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


        public List<TtGroup> GetGroups()
        {
            List<TtGroup> groups = new List<TtGroup>();

            StringBuilder query = new StringBuilder();
            query.AppendFormat("SELECT {0}, {1}, {2}, {3} from {4} ",
                TwoTrailsV2Schema.GroupSchema.CN,
                TwoTrailsV2Schema.GroupSchema.Name,
                TwoTrailsV2Schema.GroupSchema.Description,
                TwoTrailsV2Schema.GroupSchema.Type,
                TwoTrailsV2Schema.GroupSchema.TableName);


            using (SQLiteConnection conn = database.CreateAndOpenConnection())
            {
                using (SQLiteDataReader dr = database.ExecuteReader(query.ToString(), conn))
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

        public List<TtMetadata> GetMetadata()
        {
            throw new NotImplementedException();
        }

        public List<TtNmeaBurst> GetNmeaBursts(String pointCN = null)
        {
            throw new NotImplementedException();
        }

        public List<TtImage> GetPictures(String pointCN)
        {
            throw new NotImplementedException();
        }

        public List<TtPoint> GetPoints(String polyCN = null)
        {
            throw new NotImplementedException();
        }

        public List<PolygonGraphicOptions> GetPolygonGraphicOptions()
        {
            throw new NotImplementedException();
        }

        public List<TtPolygon> GetPolygons()
        {
            throw new NotImplementedException();
        }

        public TtProjectInfo GetProjectInfo()
        {
            throw new NotImplementedException();
        }

        public List<TtUserActivity> GetUserActivity()
        {
            throw new NotImplementedException();
        }

        public Boolean HasPolygons()
        {
            throw new NotImplementedException();
        }
    }
}
