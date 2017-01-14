using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

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
            return talkerID == TalkerID.Unknown ? "$??" : String.Format("${0}", talkerID);
        }

        public static TalkerID ParseTalkerID(string value)
        {
            if (value == null)
                throw new ArgumentNullException();

            if (value.StartsWith("$"))
                value = value.Substring(1, 3);

            return (TalkerID)Enum.Parse(typeof(TalkerID), value);
        }

        public static SentenceID ParseSentenceID(string value)
        {
            if (value == null)
                throw new ArgumentNullException();

            if (value.StartsWith("$"))
                value = value.Substring(3, 6);

            return (SentenceID)Enum.Parse(typeof(SentenceID), value);
        }
    }
}
