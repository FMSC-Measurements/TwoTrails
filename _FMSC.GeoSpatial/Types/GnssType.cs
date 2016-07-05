using System;

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

    public static partial class GeoSpatialTypes
    {
        public static GnssType ParseGnssType(String value)
        {
            switch (value.ToUpper())
            {
                case "GPS": return GnssType.GPS;
                case "GLONASS": return GnssType.GLONASS;
                case "GALILEO": return GnssType.GALILEO;
                case "BEIDOU": return GnssType.BEIDOU;
                case "QZSS": return GnssType.QZSS;
                case "SBAS": return GnssType.UnknownSBAS;
                case "UNKNOWNSBAS": return GnssType.UnknownSBAS;
                case "WAAS": return GnssType.WAAS;
                case "EGNOS": return GnssType.EGNOS;
                case "SDCM": return GnssType.SDCM;
                case "GAGAN": return GnssType.GAGAN;
                case "MSAS": return GnssType.MSAS;
                case "SNAS": return GnssType.SNAS;
                default: return GnssType.Unknown;
            }

            throw new Exception("Unknown GnssType");
        }

        public static GnssType ParseNmeaId(int prn)
        {
            if (prn > 0 && prn < 33)
            {
                return GnssType.GPS;
            }
            else if (prn > 32 && prn < 55)
            {
                if (prn == 33 || prn == 37 || prn == 39 || prn == 44)
                {
                    return GnssType.EGNOS;
                }
                else if (prn == 35 || prn == 51 || (prn > 45 && prn < 49))
                {
                    return GnssType.WAAS;
                }
                else if (prn == 38 || (prn > 52 && prn < 55))
                {
                    return GnssType.SDCM;
                }
                else if (prn == 40 || prn == 41)
                {
                    return GnssType.GAGAN;
                }
                else if (prn == 42 || prn == 50)
                {
                    return GnssType.MSAS;
                }
                else
                {
                    return GnssType.UnknownSBAS;
                }
            }
            else if (prn > 64 && prn < 97)
            {
                return GnssType.GLONASS;
            }
            else if (prn > 192 && prn < 201)
            {
                return GnssType.QZSS;
            }
            else if (prn >= 200 && prn < 236)
            {
                return GnssType.BEIDOU;
            }
            else
            {
                return GnssType.Unknown;
            }
        }
    }
}
