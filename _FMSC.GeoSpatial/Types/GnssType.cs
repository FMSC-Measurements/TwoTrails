namespace FMSC.GeoSpatial.Types
{
    public enum GnssType
    {
        Unknown = 0,
        GPS = 1,
        GLONASS = 2,
        GALILEO = 3,
        BEIDOU = 4,
        QZSS = 5,
        UnknownSBAS = 6,
        WAAS = 7,
        EGNOS = 8,
        SDCM = 9,
        GAGAN = 10,
        MSAS = 11,
        SNAS = 12
    }
    
    public static class GnssTypeExtentions
    {
        public static bool IsSBAS(this GnssType gnssType)
        {
            switch (gnssType)
            {
                case GnssType.UnknownSBAS:
                case GnssType.WAAS:
                case GnssType.EGNOS:
                case GnssType.SDCM:
                case GnssType.GAGAN:
                case GnssType.MSAS:
                case GnssType.SNAS: return true;
            }

            return false;
        }
    }
}
