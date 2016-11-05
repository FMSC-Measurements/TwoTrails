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
            public static string CN = "CN";
        }

        public class PointSchema
        {
            public static string TableName = "PointInfo";

            public static string Order = "PointIndex";
            public static string ID = "PID";
            public static string Polygon = "PolyID";
            public static string PolyCN = "PolyCN";
            public static string OnBoundary = "Boundary";
            public static string Comment = "Comment";
            public static string Operation = "Operation";
            public static string MetaDataID = "MetaID";
            public static string Time = "CreationTime";
            public static string AdjX = "AdjX";
            public static string AdjY = "AdjY";
            public static string AdjZ = "AdjZ";
            public static string UnAdjX = "UnAdjX";
            public static string UnAdjY = "UnAdjY";
            public static string UnAdjZ = "UnAdjZ";
            public static string QuondamLinks = "QuondamLinks";
            public static string GroupName = "GroupName";
            public static string GroupCN = "GroupCN";
        }
        #endregion
        
        #region Point Info GPS/Waypoint table
        public class GpsPointSchema
        {
            public static string TableName = "GpsPointData";

            public static string X = "X";
            public static string Y = "Y";
            public static string Z = "Z";
            public static string UserAccuracy = "UserAccuracy";
            public static string RMSEr = "RMSEr";
        }
        #endregion

        #region Point Info Traverse/SideShot table
        public class TravPointSchema
        {
            public static string TableName = "TravPointData";

            public static string ForwardAz = "ForwardAz";
            public static string BackAz = "BackAz";
            public static string SlopeDistance = "SlopeDistance";
            public static string VerticalAngle = "VerticalAngle";
            public static string HorizDistance = "HorizontalDistance";
            public static string Accuracy = "Accuracy";
        }
        #endregion

        #region Point Info Quondam Table
        public class QuondamPointSchema
        {
            public static string TableName = "QuondamPointData";

            public static string ParentPointCN = "ParentPointCN";
            public static string UserAccuracy = "UserAccuracy";
        }
        #endregion


        #region Polygon Table
        public class PolygonSchema
        {
            public static string TableName = "Polygon";

            public static string PolyID = "PolyID";
            public static string Accuracy = "Accuracy";
            public static string Description = "Description";
            public static string Area = "Area";
            public static string Perimeter = "Perimeter";
            public static string IncrementBy = "Increment";
            public static string PointStartIndex = "PointStartIndex";
        }
        #endregion

        #region Group Table
        public class GroupSchema
        {
            public static string TableName = "GroupTable";

            public static string CN = "GroupCN";
            public static string Name = "Name";
            public static string Accuracy = "Accuracy";
            public static string Description = "Description";
            public static string Type = "Type";
        }
        #endregion


        #region TTNMEA table
        public class TtnmeaSchema
        {
            public static string TableName = "TTNMEA";

            public static string CN = "CN";
            public static string PointCN = "PointCN";
            public static string Used = "Used";
            public static string DateTimeZulu = "DateTimeZulu";
            public static string Longitude = "Longitude";
            public static string Latitude = "Latitude";
            public static string LatDir = "LatDir";
            public static string LonDir = "LonDir";
            public static string MagVar = "MagVar";
            public static string MagDir = "MagDir";
            public static string UtmZone = "UtmZone";
            public static string UtmX = "UtmX";
            public static string UtmY = "UtmY";
            public static string Altitude = "Altitude";
            public static string AltUnit = "AltUnit";
            public static string FixQuality = "FixQuality";
            public static string DiffType = "DiffType";
            public static string Mode = "Mode";
            public static string PDOP = "PDOP";
            public static string HDOP = "HDOP";
            public static string VDOP = "VDOP";
            public static string SatelliteCount = "SatelliteCount";
            public static string SatelliteUsed = "SatelliteUsed";
            public static string HAE = "HAE";
            public static string HAE_Unit = "HAE_Unit";
            public static string HDo_Position = "HDo_Position";
            public static string Speed = "Speed";
            public static string Track_Angle = "Track_Angle";
            public static string PRNS = "PRNS";

            #region Satellite Information Fields 1-12
            public static string PRN1ID = "PRN1ID";
            public static string PRN1Elev = "PRN1Elev";
            public static string PRN1Az = "PRN1Az";
            public static string PRN1SRN = "PRN1SRN";
            public static string PRN2ID = "PRN2ID";
            public static string PRN2Elev = "PRN2Elev";
            public static string PRN2Az = "PRN2Az";
            public static string PRN2SRN = "PRN2SRN";
            public static string PRN3ID = "PRN3ID";
            public static string PRN3Elev = "PRN3Elev";
            public static string PRN3Az = "PRN3Az";
            public static string PRN3SRN = "PRN3SRN";
            public static string PRN4ID = "PRN4ID";
            public static string PRN4Elev = "PRN4Elev";
            public static string PRN4Az = "PRN4Az";
            public static string PRN4SRN = "PRN4SRN";
            public static string PRN5ID = "PRN5ID";
            public static string PRN5Elev = "PRN5Elev";
            public static string PRN5Az = "PRN5Az";
            public static string PRN5SRN = "PRN5SRN";
            public static string PRN6ID = "PRN6ID";
            public static string PRN6Elev = "PRN6Elev";
            public static string PRN6Az = "PRN6Az";
            public static string PRN6SRN = "PRN6SRN";
            public static string PRN7ID = "PRN7ID";
            public static string PRN7Elev = "PRN7Elev";
            public static string PRN7Az = "PRN7Az";
            public static string PRN7SRN = "PRN7SRN";
            public static string PRN8ID = "PRN8ID";
            public static string PRN8Elev = "PRN8Elev";
            public static string PRN8Az = "PRN8Az";
            public static string PRN8SRN = "PRN8SRN";
            public static string PRN9ID = "PRN9ID";
            public static string PRN9Elev = "PRN9Elev";
            public static string PRN9Az = "PRN9Az";
            public static string PRN9SRN = "PRN9SRN";
            public static string PRN10ID = "PRN10ID";
            public static string PRN10Elev = "PRN10Elev";
            public static string PRN10Az = "PRN10Az";
            public static string PRN10SRN = "PRN10SRN";
            public static string PRN11ID = "PRN11ID";
            public static string PRN11Elev = "PRN11Elev";
            public static string PRN11Az = "PRN11Az";
            public static string PRN11SRN = "PRN11SRN";
            public static string PRN12ID = "PRN12ID";
            public static string PRN12Elev = "PRN12Elev";
            public static string PRN12Az = "PRN12Az";
            public static string PRN12SRN = "PRN12SRN";
            #endregion
        }
        #endregion

        #region MetaData Table
        public class MetaDataSchema
        {
            public static string TableName = "MetaData";

            public static string CN = "CN";
            public static string ID = "MetaID";
            public static string UomDistance = "Distance";
            public static string UomSlope = "Slope";
            public static string MagDec = "MagDec";           
            public static string DeclinationType = "DecType";
            public static string UomElevation = "Elevation";
            public static string Comment = "Comment";
            public static string Datum = "Datum";
            public static string Receiver = "Receiver";
            public static string Laser = "Laser";
            public static string Compass = "Compass";
            public static string Crew = "Crew";
            public static string UtmZone = "UtmZone";
        }
        #endregion

        #region Project Info Table
        public class ProjectInfoSchema
        {
            public static string TableName = "ProjectInfo";

            public static string CN = "CN";
            public static string DeviceID = "Device";
            public static string ID = "ID";
            public static string Region = "Region";
            public static string Forest = "Forest";
            public static string District = "District";
            public static string Year = "Year";
            public static string UtmZone = "UtmZone";
            public static string Description = "Description";
            public static string DropZeros = "DropZeros";
            public static string Round = "RoundPoint";
            public static string TtDbSchemaVersion = "TtDbSchemaVersion";
            public static string TtVersion = "TtVersion";
        }
        #endregion


        #region Pcitures Table
        public class PictureSchema
        {
            public static string TableName = "PicTable";

            public static string PicData = "PicData";
            public static string PicDataValue = "@PicData";
            public static string CN = "CN";
            public static string FileName = "FileName";
            public static string Time = "Time";
            public static string PicType = "PicType";
            public static string FileType = "FileType";
            public static string UtmX = "X";
            public static string UtmY = "Y";
            public static string Elev = "Elevation";
            public static string Comment = "Comment";
            public static string Az = "Azimuth";
            public static string Acc = "Accuracy";
        }

        #endregion
    }
}
