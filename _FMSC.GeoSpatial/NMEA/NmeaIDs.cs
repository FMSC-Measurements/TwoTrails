using System;
using System.ComponentModel;
using System.Reflection;

namespace FMSC.GeoSpatial.NMEA
{
    public enum TalkerID
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("GPS")]
        GP = 1,
        [Description("Loran-C")]
        LC = 2,
        [Description("Integrated Instrumentation")]
        II = 3,
        [Description("Integrated Navigation")]
        IN = 4,
        [Description("ECDIS")]
        EC = 5,
        [Description("DSC")]
        CD = 6,
        [Description("Galileo")]
        GA = 7,
        [Description("GLONASS")]
        GL = 8,
        [Description("GPS + GLONASS")]
        GN = 9,
        [Description("BeiDou")]
        GB = 10,
        [Description("BeiDou")]
        BD = 11,
        [Description("QZSS")]
        QZ = 12
    }

    public enum SentenceID
    {
        Unknown = 0,
        BOD = 1,
        BWC = 2,
        GGA = 3,
        GLL = 4,
        GNS = 5,
        GSA = 6,
        GSV = 7,
        HDT = 8,
        R00 = 9,
        RMA = 10,
        RMB = 11,
        RMC = 12,
        RTE = 13,
        TRF = 14,
        STN = 15,
        VBW = 16,
        VTG = 17,
        WPL = 18,
        XTE = 19,
        ZDA = 20
    }


    public static class NmeaIDTools
    {
        public static String GetName(this TalkerID talkerID)
        {
            FieldInfo fi = talkerID.GetType().GetField(talkerID.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return talkerID.ToString();
            }
        }

        public static String ToStringCode(this TalkerID talkerID)
        {
            return talkerID == TalkerID.Unknown ? "$??" : $"${talkerID}";
        }

        public static TalkerID ParseTalkerID(string value)
        {
            if (value == null)
                throw new ArgumentNullException();

            if (value.StartsWith("$"))
                value = value.Substring(1, 2);

            TalkerID tid = TalkerID.Unknown;
            Enum.TryParse(value, true, out tid);

            return tid;
        }

        public static SentenceID ParseSentenceID(string value)
        {
            if (value == null)
                throw new ArgumentNullException();

            if (value.StartsWith("$"))
                value = value.Substring(3, 3);

            SentenceID sid = SentenceID.Unknown;
            Enum.TryParse(value, true, out sid);

            return sid;
        }
    }
}
