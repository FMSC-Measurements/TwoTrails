using System;
using TwoTrails.Core;

namespace TwoTrails.DAL
{
    public static class TwoTrailsSchema
    {
        //Old Schema Versions
        public static readonly Version OSV_2_0_0 = new Version(2, 0, 0);

        //Schema Version
        public static readonly Version SchemaVersion = OSV_2_0_0;


        public static class SharedSchema
        {
            public const String CN = "CN";
        }

        #region Point Info Table
        public static class PointSchema
        {
            public const String TableName = "Points";

            public const String Index = "PointIndex";
            public const String ID = "PID";
            public const String PolyName = "PolyName";
            public const String PolyCN = "PolyCN";
            public const String GroupName = "GroupName";
            public const String GroupCN = "GroupCN";
            public const String OnBoundary = "Boundary";
            public const String Comment = "Comment";
            public const String Operation = "Operation";
            public const String MetadataCN = "MetaCN";
            public const String CreationTime = "CreationTime";
            public const String AdjX = "AdjX";
            public const String AdjY = "AdjY";
            public const String AdjZ = "AdjZ";
            public const String UnAdjX = "UnAdjX";
            public const String UnAdjY = "UnAdjY";
            public const String UnAdjZ = "UnAdjZ";
            public const String Accuracy = "Accuracy";
            public const String QuondamLinks = "QuondamLinks";


            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                TwoTrailsSchema.SharedSchema.CN + " TEXT, " +
                Index + " INTEGER NOT NULL, " +
                ID + " INTEGER NOT NULL, " +
                PolyName + " TEXT, " +
                PolyCN + " TEXT NOT NULL, " +
                GroupName + " TEXT, " +
                GroupCN + " TEXT NOT NULL, " +
                OnBoundary + " BOOLEAN NOT NULL, " +
                Comment + " TEXT, " +
                Operation + " INTEGERL, " +
                MetadataCN + " TEXT REFERENCES " +
                MetadataSchema.TableName + ", " +
                CreationTime + " TEXT, " +
                AdjX + " REAL, " +
                AdjY + " REAL, " +
                AdjZ + " REAL, " +
                UnAdjX + " REAL NOT NULL, " +
                UnAdjY + " REAL NOT NULL, " +
                UnAdjZ + " REAL NOT NULL, " +
                Accuracy + " REAL, " +
                QuondamLinks + " TEXT, " +
                "PRIMARY KEY (" + TwoTrailsSchema.SharedSchema.CN + "));";


            public const String SelectItems =
                SharedSchema.CN + ", " +
                Index + ", " +
                ID + ", " +
                PolyCN + ", " +
                GroupCN + ", " +
                OnBoundary + ", " +
                Comment + ", " +
                Operation + ", " +
                MetadataCN + ", " +
                CreationTime + ", " +
                AdjX + ", " +
                AdjY + ", " +
                AdjZ + ", " +
                UnAdjX + ", " +
                UnAdjY + ", " +
                UnAdjZ + ", " +
                Accuracy + ", " +
                QuondamLinks;
        }
        #endregion

        #region Point Info GPS
        public static class GpsPointSchema
        {
            public const String TableName = "GpsPointData";

            public const String Latitude = "UnAdjLatitude";
            public const String Longitude = "UnAdjLongitude";
            public const String Elevation = "Elevation";
            public const String ManualAccuracy = "ManualAccuracy";
            public const String RMSEr = "RMSEr";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT REFERENCES " +
                PointSchema.TableName + ", " +
                Latitude + " REAL, " +
                Longitude + " REAL, " +
                Elevation + " REAL, " +
                ManualAccuracy + " REAL, " +
                RMSEr + " REAL, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";


            public const String SelectItems =
                SharedSchema.CN + ", " +
                Latitude + ", " +
                Longitude + ", " +
                Elevation + ", " +
                ManualAccuracy + ", " +
                RMSEr;

            public const String SelectItemsNoCN =
                Latitude + ", " +
                Longitude + ", " +
                Elevation + ", " +
                TableName + "." + ManualAccuracy + ", " +
                RMSEr;
        }
        #endregion

        #region Point Info Traverse/SideShot table
        public static class TravPointSchema
        {
            public const String TableName = "TravPointData";

            public const String ForwardAz = "ForwardAz";
            public const String BackAz = "BackAz";
            public const String SlopeDistance = "SlopeDistance";
            public const String SlopeAngle = "VerticalAngle";
            public const String HorizDistance = "HorizontalDistance";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT REFERENCES " +
                PointSchema.TableName + ", " +
                ForwardAz + " REAL, " +
                BackAz + " REAL, " +
                SlopeDistance + " REAL NOT NULL, " +
                SlopeAngle + " REAL NOT NULL, " +
                HorizDistance + " REAL, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";


            public const String SelectItems =
                SharedSchema.CN + ", " +
                ForwardAz + ", " +
                BackAz + ", " +
                SlopeDistance + ", " +
                SlopeAngle;

            public const String SelectItemsNoCN =
                ForwardAz + ", " +
                BackAz + ", " +
                SlopeDistance + ", " +
                SlopeAngle;
        }
        #endregion

        #region Point Info Quondam Table
        public static class QuondamPointSchema
        {
            public const String TableName = "QuondamPointData";

            public const String ParentPointCN = "ParentPointCN";
            public const String ManualAccuracy = "ManualAccuracy";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT REFERENCES " +
                PointSchema.TableName + ", " +
                ParentPointCN + " TEXT NOT NULL, " +
                ManualAccuracy + " REAL, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";


            public const String SelectItems =
                SharedSchema.CN + ", " +
                ParentPointCN + ", " +
                ManualAccuracy;

            public const String SelectItemsNoCN =
                ParentPointCN + ", " +
                TableName + "." + "ManualAccuracy";
        }
        #endregion

        #region Polygon Table
        public static class PolygonSchema
        {
            public const String TableName = "Polygons";

            public const String Name = "Name";
            public const String Accuracy = "Accuracy";
            public const String Description = "Description";
            public const String Area = "Area";
            public const String Perimeter = "Perimeter";
            public const String PerimeterLine = "PerimeterLine";
            public const String IncrementBy = "Increment";
            public const String PointStartIndex = "PointStartIndex";
            public const String TimeCreated = "TimeCreated";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT, " +
                Name + " TEXT, " +
                Accuracy + " REAL, " +
                Description + " TEXT, " +
                Area + " REAL, " +
                Perimeter + " REAL, " +
                PerimeterLine + " REAL, " +
                IncrementBy + " INTEGER, " +
                PointStartIndex + " INTEGER, " +
                TimeCreated + " TEXT, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";


            public const String SelectItems =
                SharedSchema.CN + ", " +
                Name + ", " +
                Description + ", " +
                Accuracy + ", " +
                IncrementBy + ", " +
                PointStartIndex + ", " +
                TimeCreated + ", " +
                Area + ", " +
                Perimeter + ", " +
                PerimeterLine;
        }
        #endregion

        #region Group Table
        public static class GroupSchema
        {
            public const String TableName = "Groups";

            public const String Name = "Name";
            public const String Description = "Description";
            public const String Type = "Type";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT, " +
                Name + " TEXT, " +
                Description + " TEXT, " +
                Type + " INTEGER, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";


            public const String SelectItems =
                SharedSchema.CN + ", " +
                Name + ", " +
                Description + ", " +
                Type;
        }
        #endregion

        #region TTNMEA table
        public static class TtNmeaSchema
        {
            public const String TableName = "TTNMEA";

            public const String PointCN = "PointCN";
            public const String Used = "Used";
            public const String TimeCreated = "TimeCreated";
            public const String FixTime = "FixTime";
            public const String Latitude = "Latitude";
            public const String LatDir = "LatDir";
            public const String Longitude = "Longitude";
            public const String LonDir = "LonDir";
            public const String Elevation = "Elevation";
            public const String ElevUom = "ElevUom";
            public const String MagVar = "MagVar";
            public const String MagDir = "MagDir";
            public const String Fix = "Fix";
            public const String FixQuality = "FixQuality";
            public const String Mode = "Mode";
            public const String PDOP = "PDOP";
            public const String HDOP = "HDOP";
            public const String VDOP = "VDOP";
            public const String HorizDilution = "HorizDilution";
            public const String GeiodHeight = "GeiodHeight";
            public const String GeiodHeightUom = "GeiodHeightUom";
            public const String GroundSpeed = "GroundSpeed";
            public const String TrackAngle = "TrackAngle";
            public const String SatellitesUsedCount = "SatUsedCount";
            public const String SatellitesTrackedCount = "SatTrackCount";
            public const String SatellitesInViewCount = "SatInViewCount";
            public const String UsedSatPRNS = "PRNS";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT, " +
                PointCN + " TEXT, " +
                Used + " BOOLEAN, " +
                TimeCreated + " TEXT, " +
                FixTime + " TEXT, " +
                Latitude + " REAL, " +
                LatDir + " INTEGER, " +
                Longitude + " REAL, " +
                LonDir + " INTEGER, " +
                Elevation + " REAL, " +
                ElevUom + " INTEGER, " +
                MagVar + " REAL, " +
                MagDir + " INTEGER, " +
                Fix + " INTEGER, " +
                FixQuality + " INTEGER, " +
                Mode + " INTEGER, " +
                PDOP + " REAL, " +
                HDOP + " REAL, " +
                VDOP + " REAL, " +
                HorizDilution + " REAL, " +
                GeiodHeight + " REAL, " +
                GeiodHeightUom + " INTEGER, " +
                GroundSpeed + " REAL, " +
                TrackAngle + " REAL, " +
                SatellitesUsedCount + " INTEGER, " +
                SatellitesTrackedCount + " INTEGER, " +
                SatellitesInViewCount + " INTEGER, " +
                UsedSatPRNS + " TEXT, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";

            public const String SelectItems =
                SharedSchema.CN + ", " +
                PointCN + ", " +
                Used + ", " +
                TimeCreated + ", " +
                FixTime + ", " +
                Latitude + ", " +
                LatDir + ", " +
                Longitude + ", " +
                LonDir + ", " +
                Elevation + ", " +
                ElevUom + ", " +
                MagVar + ", " +
                MagDir + ", " +
                Fix + ", " +
                FixQuality + ", " +
                Mode + ", " +
                PDOP + ", " +
                HDOP + ", " +
                VDOP + ", " +
                HorizDilution + ", " +
                GeiodHeight + ", " +
                GeiodHeightUom + ", " +
                GroundSpeed + ", " +
                TrackAngle + ", " +
                SatellitesUsedCount + ", " +
                SatellitesTrackedCount + ", " +
                SatellitesInViewCount + ", " +
                UsedSatPRNS;
        }
        #endregion

        #region MetaData Table
        public static class MetadataSchema
        {
            public const String TableName = "Metadata";

            public const String Name = "Name";
            public const String Distance = "Distance";
            public const String Slope = "Slope";
            public const String MagDec = "MagDec";
            public const String DeclinationType = "DecType";
            public const String Elevation = "Elevation";
            public const String Comment = "Comment";
            public const String Datum = "Datum";
            public const String GpsReceiver = "GpsReceiver";
            public const String RangeFinder = "RangeFinder";
            public const String Compass = "Compass";
            public const String Crew = "Crew";
            public const String UtmZone = "UtmZone";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT NOT NULL, " +
                Name + " TEXT, " +
                UtmZone + " INTEGER, " +
                Distance + " INTEGER, " +
                Slope + " INTEGER, " +
                MagDec + " REAL, " +
                DeclinationType + " INTEGER, " +
                Elevation + " INTEGER, " +
                Datum + " INTEGER, " +
                Comment + " TEXT, " +
                GpsReceiver + " TEXT, " +
                RangeFinder + " TEXT, " +
                Compass + " TEXT, " +
                Crew + " TEXT, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";

            public const String SelectItems =
                SharedSchema.CN + ", " +
                Name + ", " +
                Comment + ", " +
                UtmZone + ", " +
                Datum + ", " +
                Distance + ", " +
                Elevation + ", " +
                Slope + ", " +
                DeclinationType + ", " +
                MagDec + ", " +
                GpsReceiver + ", " +
                RangeFinder + ", " +
                Compass + ", " +
                Crew;
        }
        #endregion

        #region Project Info Table
        public static class ProjectInfoSchema
        {
            public const String TableName = "ProjectInfo";

            public const String DeviceID = "Device";
            public const String Name = "Name";
            public const String Region = "Region";
            public const String Forest = "Forest";
            public const String District = "District";
            public const String Created = "DateCreated";
            public const String Description = "Description";
            public const String TtDbSchemaVersion = "TtDbSchemaVersion";
            public const String TtVersion = "TtVersion";
            public const String CreatedTtVersion = "CreatedTtVersion";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                Name + " TEXT, " +
                District + " TEXT, " +
                Forest + " TEXT, " +
                Region + " TEXT, " +
                DeviceID + " TEXT, " +
                Created + " TEXT, " +
                Description + " TEXT, " +
                TtDbSchemaVersion + " TEXT, " +
                TtVersion + " TEXT, " +
                CreatedTtVersion + " TEXT" +
                ");";
            
            public const String SelectItems =
                Name + ", " +
                District + ", " +
                Forest + ", " +
                Region + ", " +
                DeviceID + ", " +
                Created + ", " +
                Description + ", " +
                TtDbSchemaVersion + ", " +
                TtVersion + ", " +
                CreatedTtVersion;
        }
        #endregion

        #region Polygon Attr Table
        public static class PolygonAttrSchema
        {
            public const String TableName = "PolygonAttr";

            public const String AdjBndColor = "AdjBndColor";
            public const String UnAdjBndColor = "UnAdjBndColor";
            public const String AdjNavColor = "AdjNavColor";
            public const String UnAdjNavColor = "UnAdjNavColor";
            public const String AdjPtsColor = "AdjPtsColor";
            public const String UnAdjPtsColor = "UnAdjPtsColor";
            public const String WayPtsColor = "WayPtsColor";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT, " +
                AdjBndColor + " INTEGER, " +
                UnAdjBndColor + " INTEGER, " +
                AdjNavColor + " INTEGER, " +
                UnAdjNavColor + " INTEGER, " +
                AdjPtsColor + " INTEGER, " +
                UnAdjPtsColor + " INTEGER, " +
                WayPtsColor + " INTEGER, " +
                "PRIMARY KEY (" + SharedSchema.CN + "));";

            public const String SelectItems =
                SharedSchema.CN + ", " +
                AdjBndColor + ", " +
                UnAdjBndColor + ", " +
                AdjNavColor + ", " +
                UnAdjNavColor + ", " +
                AdjPtsColor + ", " +
                UnAdjPtsColor + ", " +
                WayPtsColor;
        }
        #endregion

        #region Media
        #region Media Table
        public static class MediaSchema
        {
            public const String TableName = "Media";

            public const String PointCN = "PointCN";
            public const String MediaType = "MediaType";
            public const String Name = "Name";
            public const String FilePath = "FilePath";
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
        public static class PictureSchema
        {
            public const String TableName = "PictureData";

            public const String PicType = "Type";
            public const String Azimuth = "Azimuth";
            public const String Pitch = "Pitch";
            public const String Roll = "Roll";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                SharedSchema.CN + " TEXT REFERENCES " +
                MediaSchema.TableName + ", " +
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
        #endregion


        #region Activity
        public static class ActivitySchema
        {
            public const String TableName = "Activity";

            public const String UserName = "UserName";
            public const String DeviceName = "DeviceName";
            public const String ActivityDate = "ActivityDate";
            public const String DataActivity = "ActivityType";

            public const String CreateTable =
                "CREATE TABLE " + TableName + " (" +
                UserName + " TEXT, " +
                DeviceName + " TEXT, " +
                ActivityDate + " TEXT, " +
                DataActivity + " INTEGER" +
                ");";


            public const String SelectItems =
                UserName + ", " +
                DeviceName + ", " +
                ActivityDate + ", " +
                DataActivity;
        }
        #endregion
    }
}
