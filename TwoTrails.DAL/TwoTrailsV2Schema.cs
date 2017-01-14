using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.DAL
{
    public class TwoTrailsV2Schema
    {
        //Schema Version
        public static readonly Version RequiredSchemaVersion = new Version("1.2.0");


        #region Point Info Table
        public class SharedSchema
        {
            public const string CN = "CN";
        }

        public class PointSchema
        {
            public const string TableName = "PointInfo";

            public const string Order = "PointIndex";
            public const string ID = "PID";
            public const string Polygon = "PolyID";
            public const string PolyCN = "PolyCN";
            public const string OnBoundary = "Boundary";
            public const string Comment = "Comment";
            public const string Operation = "Operation";
            public const string MetaDataID = "MetaID";
            public const string Time = "CreationTime";
            public const string AdjX = "AdjX";
            public const string AdjY = "AdjY";
            public const string AdjZ = "AdjZ";
            public const string UnAdjX = "UnAdjX";
            public const string UnAdjY = "UnAdjY";
            public const string UnAdjZ = "UnAdjZ";
            public const string QuondamLinks = "QuondamLinks";
            public const string GroupName = "GroupName";
            public const string GroupCN = "GroupCN";

            public const string SelectItems = SharedSchema.CN + ", " +
                Order + ", " +
                ID + ", " +
                PolyCN + ", " +
                GroupCN + ", " +
                OnBoundary + ", " +
                Comment + ", " +
                Operation + ", " +
                MetaDataID + ", " +
                Time + ", " +
                AdjX + ", " +
                AdjY + ", " +
                AdjZ + ", " +
                UnAdjX + ", " +
                UnAdjY + ", " +
                UnAdjZ + ", " +
                QuondamLinks;
        }
        #endregion
        
        #region Point Info GPS/Waypoint table
        public class GpsPointSchema
        {
            public const string TableName = "GpsPointData";

            public const string X = "X";
            public const string Y = "Y";
            public const string Z = "Z";
            public const string UserAccuracy = "UserAccuracy";
            public const string RMSEr = "RMSEr";
            
            public const String SelectItemsNoCN =
                TableName + "." + UserAccuracy + ", " +
                RMSEr;
        }
        #endregion

        #region Point Info Traverse/SideShot table
        public class TravPointSchema
        {
            public const string TableName = "TravPointData";

            public const string ForwardAz = "ForwardAz";
            public const string BackAz = "BackAz";
            public const string SlopeDistance = "SlopeDistance";
            public const string VerticalAngle = "VerticalAngle";
            public const string HorizDistance = "HorizontalDistance";
            public const string Accuracy = "Accuracy";

            public const String SelectItemsNoCN =
                ForwardAz + ", " +
                BackAz + ", " +
                SlopeDistance + ", " +
                VerticalAngle;
        }
        #endregion

        #region Point Info Quondam Table
        public class QuondamPointSchema
        {
            public const string TableName = "QuondamPointData";

            public const string ParentPointCN = "ParentPointCN";
            public const string UserAccuracy = "UserAccuracy";

            public const String SelectItemsNoCN =
                ParentPointCN + ", " +
                TableName + "." + UserAccuracy;
        }
        #endregion


        #region Polygon Table
        public class PolygonSchema
        {
            public const string TableName = "Polygon";

            public const string PolyID = "PolyID";
            public const string Accuracy = "Accuracy";
            public const string Description = "Description";
            public const string Area = "Area";
            public const string Perimeter = "Perimeter";
            public const string IncrementBy = "Increment";
            public const string PointStartIndex = "PointStartIndex";
        }
        #endregion

        #region Group Table
        public class GroupSchema
        {
            public const string TableName = "GroupTable";

            public const string CN = "GroupCN";
            public const string Name = "Name";
            public const string Accuracy = "Accuracy";
            public const string Description = "Description";
            public const string Type = "Type";
        }
        #endregion


        #region TTNMEA table
        public class TtNmeaSchema
        {
            public const string TableName = "TTNMEA";

            public const string CN = "CN";
            public const string PointCN = "PointCN";
            public const string Used = "Used";
            public const string DateTimeZulu = "DateTimeZulu";
            public const string Longitude = "Longitude";
            public const string Latitude = "Latitude";
            public const string LatDir = "LatDir";
            public const string LonDir = "LonDir";
            public const string MagVar = "MagVar";
            public const string MagDir = "MagDir";
            public const string UtmZone = "UtmZone";
            public const string UtmX = "UtmX";
            public const string UtmY = "UtmY";
            public const string Altitude = "Altitude";
            public const string AltUnit = "AltUnit";
            public const string FixQuality = "FixQuality";
            public const string DiffType = "DiffType";
            public const string Mode = "Mode";
            public const string PDOP = "PDOP";
            public const string HDOP = "HDOP";
            public const string VDOP = "VDOP";
            public const string SatelliteCount = "SatelliteCount";
            public const string SatelliteUsed = "SatelliteUsed";
            public const string HAE = "HAE";
            public const string HAE_Unit = "HAE_Unit";
            public const string HDo_Position = "HDo_Position";
            public const string Speed = "Speed";
            public const string Track_Angle = "Track_Angle";
            public const string PRNS = "PRNS";

            public const String SelectItems =
                SharedSchema.CN + ", " +
                PointCN + ", " +
                Used + ", " +
                DateTimeZulu + ", " +
                Latitude + ", " +
                LatDir + ", " +
                Longitude + ", " +
                LonDir + ", " +
                Altitude + ", " +
                AltUnit + ", " +
                MagVar + ", " +
                MagDir + ", " +
                DiffType + ", " +
                FixQuality + ", " +
                Mode + ", " +
                PDOP + ", " +
                HDOP + ", " +
                VDOP + ", " +
                HAE + ", " +
                HDo_Position + ", " +
                HAE_Unit + ", " +
                Speed + ", " +
                Track_Angle + ", " +
                SatelliteUsed + ", " +
                SatelliteCount + ", " +
                PRNS;
        }
        #endregion

        #region MetaData Table
        public class MetaDataSchema
        {
            public const string TableName = "MetaData";

            public const string CN = "CN";
            public const string ID = "MetaID";
            public const string UomDistance = "Distance";
            public const string UomSlope = "Slope";
            public const string MagDec = "MagDec";           
            public const string DeclinationType = "DecType";
            public const string UomElevation = "Elevation";
            public const string Comment = "Comment";
            public const string Datum = "Datum";
            public const string Receiver = "Receiver";
            public const string Laser = "Laser";
            public const string Compass = "Compass";
            public const string Crew = "Crew";
            public const string UtmZone = "UtmZone";
        }
        #endregion

        #region Project Info Table
        public class ProjectInfoSchema
        {
            public const string TableName = "ProjectInfo";

            public const string CN = "CN";
            public const string DeviceID = "Device";
            public const string ID = "ID";
            public const string Region = "Region";
            public const string Forest = "Forest";
            public const string District = "District";
            public const string Year = "Year";
            public const string UtmZone = "UtmZone";
            public const string Description = "Description";
            public const string DropZeros = "DropZeros";
            public const string Round = "RoundPoint";
            public const string TtDbSchemaVersion = "TtDbSchemaVersion";
            public const string TtVersion = "TtVersion";
        }
        #endregion


        #region Pcitures Table
        public class PictureSchema
        {
            public const string TableName = "PicTable";

            public const string PicData = "PicData";
            public const string PicDataValue = "@PicData";
            public const string CN = "CN";
            public const string FileName = "FileName";
            public const string Time = "Time";
            public const string PicType = "PicType";
            public const string FileType = "FileType";
            public const string UtmX = "X";
            public const string UtmY = "Y";
            public const string Elev = "Elevation";
            public const string Comment = "Comment";
            public const string Az = "Azimuth";
            public const string Acc = "Accuracy";
        }

        #endregion
    }
}
