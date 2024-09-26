using System;
using System.Reflection;

namespace TwoTrails.Core
{
    public static class MetadataProperties
    {
        public static readonly Type DataType = typeof(TtMetadata);

        public static readonly PropertyInfo NAME;
        public static readonly PropertyInfo COMMENT;
        public static readonly PropertyInfo ZONE;
        public static readonly PropertyInfo DEC_TYPE;
        public static readonly PropertyInfo MAG_DEC;
        public static readonly PropertyInfo DATUM;
        public static readonly PropertyInfo DISTANCE;
        public static readonly PropertyInfo ELEVATION;
        public static readonly PropertyInfo SLOPE;
        public static readonly PropertyInfo GPS_RECEIVER;
        public static readonly PropertyInfo RANGE_FINDER;
        public static readonly PropertyInfo COMPASS;
        public static readonly PropertyInfo CREW;

        static MetadataProperties()
        {
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            NAME = DataType.GetProperty(nameof(TtMetadata.Name), bf);
            COMMENT = DataType.GetProperty(nameof(TtMetadata.Comment), bf);
            ZONE = DataType.GetProperty(nameof(TtMetadata.Zone), bf);
            DEC_TYPE = DataType.GetProperty(nameof(TtMetadata.DecType), bf);
            MAG_DEC = DataType.GetProperty(nameof(TtMetadata.MagDec), bf);
            DATUM = DataType.GetProperty(nameof(TtMetadata.Datum), bf);
            DISTANCE = DataType.GetProperty(nameof(TtMetadata.Distance), bf);
            ELEVATION = DataType.GetProperty(nameof(TtMetadata.Elevation), bf);
            SLOPE = DataType.GetProperty(nameof(TtMetadata.Slope), bf);
            GPS_RECEIVER = DataType.GetProperty(nameof(TtMetadata.GpsReceiver), bf);
            RANGE_FINDER = DataType.GetProperty(nameof(TtMetadata.RangeFinder), bf);
            COMPASS = DataType.GetProperty(nameof(TtMetadata.Compass), bf);
            CREW = DataType.GetProperty(nameof(TtMetadata.Crew), bf);
        }

        public static PropertyInfo GetPropertyByName(string name)
        {
            switch (name)
            {
                case nameof(TtMetadata.Name): return NAME;
                case nameof(TtMetadata.Comment): return COMMENT;
                case nameof(TtMetadata.Zone): return ZONE;
                case nameof(TtMetadata.DecType): return DEC_TYPE;
                case nameof(TtMetadata.MagDec): return MAG_DEC;
                case nameof(TtMetadata.Datum): return DATUM;
                case nameof(TtMetadata.Distance): return DISTANCE;
                case nameof(TtMetadata.Elevation): return ELEVATION;
                case nameof(TtMetadata.Slope): return SLOPE;
                case nameof(TtMetadata.GpsReceiver): return GPS_RECEIVER;
                case nameof(TtMetadata.RangeFinder): return RANGE_FINDER;
                case nameof(TtMetadata.Compass): return COMPASS;
                case nameof(TtMetadata.Crew): return CREW;
            }

            return null;
        }
    }
}
