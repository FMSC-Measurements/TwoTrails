using System;
using TwoTrails.Core;

namespace TwoTrails.DAL
{
    public static class TwoTrailsMediaSchema
    {
        //Old Schema Versions
        public static readonly Version OSV_2_0_0 = new Version(2, 0, 0);

        //Schema Version
        public static readonly Version SchemaVersion = OSV_2_0_0;


        public static class SharedSchema
        {
            public const String CN = "CN";
        }
        
        #region Info Table
        public static class Info
        {
            public const String TableName = "Info";

            public const String TtMediaDbSchemaVersion = "TtMediaDbSchemaVersion";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                TtMediaDbSchemaVersion + " TEXT" +
                ");";
            
            public const String SelectItems =
                TtMediaDbSchemaVersion;
        }
        #endregion

        #region Media Table
        public static class Media
        {
            public const String TableName = "Media";

            public const String PointCN = "PointCN";
            public const String MediaType = "MediaType";
            public const String Name = "Name";
            public const String FilePath = "FileName";
            public const String CreationTime = "CreationTime";
            public const String Comment = "Comment";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT NOT NULL, " +
                PointCN + " TEXT, " +
                MediaType + " INTEGER, " +
                Name + " TEXT, " +
                FilePath + " TEXT, " +
                CreationTime + " TEXT, " +
                Comment + " TEXT, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";

            public const String SelectItems =
                    SharedSchema.CN + ", " +
                    PointCN + ", " +
                    MediaType + ", " +
                    Name + ", " +
                    FilePath + ", " +
                    CreationTime + ", " +
                    Comment;
        }
        #endregion
        
        #region PictureTable
        public static class Pictures
        {
            public const String TableName = "PictureData";

            public const String PicType = "Type";
            public const String Azimuth = "Azimuth";
            public const String Pitch = "Pitch";
            public const String Roll = "Roll";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT REFERENCES " +
                Media.TableName + ", " +
                PicType + " INTEGER, " +
                Azimuth + " REAL, " +
                Pitch + " REAL, " +
                Roll + " REAL, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";

            public const String SelectItemsNoCN =
                PicType + ", " +
                Azimuth + ", " +
                Pitch + ", " +
                Roll;
        }
        #endregion
    }
}
