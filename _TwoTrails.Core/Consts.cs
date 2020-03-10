using System;

namespace TwoTrails.Core
{
    public static partial class Consts
    {
        public const String DATE_FORMAT = "yyyy-MM-dd HH:mm:ss.FFF";

        public const String EmptyGuid = "00000000-0000-0000-0000-000000000000";
        public const String FullGuid = "11111111-1111-1111-1111-111111111111";

        public const double MINIMUM_POINT_DIGIT_ACCURACY = 0.000001d;
        public const double DEFAULT_POINT_ACCURACY = 6.01d;
        public const int DEFAULT_POINT_START_INDEX = 1010;
        public const int DEFAULT_POINT_INCREMENT = 10;

        public const String DefaultGroupName = "Main Group";
        public const String DefaultGroupDesc = "Default Group";

        public const String FILE_EXTENSION = ".ttx";
        public const String FILE_EXTENSION_FILTER = "TwoTrails Files (*.ttx)|*.ttx";

        public const String FILE_EXTENSION_MEDIA = ".ttmpx";
        public const String FILE_EXTENSION_FILTER_MEDIA = "TwoTrails Media Packages (*.ttmpx)|*.ttmpx";

        public const String FILE_EXTENSION_V2 = ".tt2";
        public const String FILE_EXTENSION_FILTER_V2 = "TwoTrails2 Files (*.tt2)|*.tt2";
        
        public const String FILE_EXTENSION_DATA_DICTIONARY = ".ddt";
        public const String FILE_EXTENSION_DATA_DICTIONARY_FILTER = "DataDictionary Template (*.ddt)|*.ddt";

        public const string TEXT_EXT = ".txt";
        public const string CSV_EXT = ".csv";
        public const string KML_EXT = ".kml";
        public const string KMZ_EXT = ".kmz";
        public const string GPX_EXT = ".gpx";
        public const string SHAPE_EXT = ".shp";
        public const string SHAPE_PRJ_EXT = ".prj";
        public const string SHAPE_SHX_EXT = ".shx";
        public const string SHAPE_DBF_EXT = ".dbf";


        public const string URL_TWOTRAILS_UPDATE = @"https://www.fs.fed.us/fmsc/ftp/measure/geospatial/TwoTrails/twotrails.version";
        public const string URL_TWOTRAILS = @"https://www.fs.fed.us/forestmanagement/products/measurement/area-determination/twotrails/";
        public const string URL_FMSC = @"https://www.fs.fed.us/forestmanagement/products/measurement/";

        public const String EMAIL_SUBJECT = "TwoTrails Error Report";
        public const String EMAIL_BODY = "I am experiencing issues in TwoTrails Android and would like to report it to the development team.%0A%0ANotes: ";
    }
}
