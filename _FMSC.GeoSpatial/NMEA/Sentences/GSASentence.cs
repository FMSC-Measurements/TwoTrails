using FMSC.GeoSpatial.NMEA.Sentences.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FMSC.GeoSpatial.NMEA.Sentences
{
    [Serializable]
    public class GSASentence : NmeaSentence
    {
        public override SentenceID SentenceID { get; } = SentenceID.GSA;

        public Mode Mode { get; protected set; }
        public Fix Fix { get; protected set; }
        public ReadOnlyCollection<Int32> SatellitesUsed { get; protected set; }
        public float PDOP { get; protected set; }
        public float HDOP { get; protected set; }
        public float VDOP { get; protected set; }


        public GSASentence() { }

        public GSASentence(string nmea) : base(nmea) { }

        public override bool Parse(string nmea)
        {
            List<int> satellitesUsed = new List<int>();

            if (base.Parse(nmea))
            {
                IsValid = false;
                String[] tokens = nmea.Substring(0, nmea.IndexOf("*")).Split(',');

                if (tokens.Length > 17)
                {
                    try
                    {
                        Mode = ModeExtensions.Parse(tokens[1]);

                        Fix = FixExtensions.Parse(tokens[2]);

                        String token;
                        for (int i = 3; i < 15; i++)
                        {
                            token = tokens[i];
                            if (!String.IsNullOrEmpty(token))
                                satellitesUsed.Add(Int32.Parse(token));
                        }

                        SatellitesUsed = new ReadOnlyCollection<int>(satellitesUsed);

                        token = tokens[15];
                        if (!String.IsNullOrEmpty(token))
                            PDOP = float.Parse(token);

                        token = tokens[16];
                        if (!String.IsNullOrEmpty(token))
                            HDOP = float.Parse(token);

                        token = tokens[17];
                        if (!String.IsNullOrEmpty(token))
                            VDOP = float.Parse(token);

                        IsValid = true;
                    }
                    catch
                    {
                        //
                    }
                }
            }

            return IsValid;
        }
    }


    public enum Mode
    {
        Auto = 0,
        Manual = 1
    }

    public static class ModeExtensions
    {
        public static Mode Parse(String str)
        {
            switch (str.ToLower())
            {
                case "0":
                case "a":
                case "auto": return Mode.Auto;
                case "1":
                case "m":
                case "manual": return Mode.Manual;
                default: throw new Exception("InIsValid Mode Name: " + str);
            }
        }

        public static String ToStringF(this Mode Mode)
        {
            switch (Mode)
            {
                case Mode.Auto: return " 0 (Auto)";
                case Mode.Manual: return "1 (Manual)";
                default: throw new Exception();
            }
        }
    }
    

    public enum Fix
    {
        NoFix = 0,
        _2D = 1,
        _3D = 2
    }

    public static class FixExtensions
    {
        public static Fix Parse(String str)
        {
            switch (str.ToLower())
            {
                case "1":
                case "noFix":
                case "no Fix":
                case "no": return Fix.NoFix;
                case "2":
                case "2d": return Fix._2D;
                case "3":
                case "3d": return Fix._3D;
                default: throw new Exception("InIsValid Fix Name: " + str);
            }
        }

        public static String ToString(this Fix Fix)
        {
            switch (Fix)
            {
                case Fix.NoFix: return "No Fix";
                case Fix._2D: return "2D";
                case Fix._3D: return "3D";
                default: throw new Exception();
            }
        }

        public static String ToStringF(this Fix Fix)
        {
            switch (Fix)
            {
                case Fix.NoFix: return "0 (No Fix)";
                case Fix._2D: return "1 (2D)";
                case Fix._3D: return "2 (3D)";
                default: throw new Exception();
            }
        }
    }
}
