using FMSC.GeoSpatial.NMEA.Sentences.Base;
using FMSC.GeoSpatial.Types;
using System;
using System.Globalization;

namespace FMSC.GeoSpatial.NMEA.Sentences
{
    [Serializable]
    public class GGASentence : PositionSentence
    {
        protected static readonly string[] GGATimeFormatters = new string[] { "HHmmss.fff", "HHmmss.ff", "HHmmss.f", "HHmmss" };

        public override SentenceID SentenceID { get; } = SentenceID.GGA;

        public DateTime FixTime { get; protected set; }
        public GpsFixType FixQuality { get; protected set; }
        public int TrackedSatellitesCount { get; protected set; }
        public double HorizDilution { get; protected set; }
        public double GeoidHeight { get; protected set; }
        public UomElevation GeoUom { get; protected set; }


        public GGASentence() { }

        public GGASentence(String nmea) : base(nmea) { }
        
        public override bool Parse(String nmea)
        {
            if (base.Parse(nmea))
            {
                IsValid = false;
                String[] tokens = nmea.Substring(0, nmea.IndexOf("*")).Split(',');

                if (tokens.Length > 14 && tokens[1].Length > 0)
                {
                    try
                    {
                        FixTime = DateTime.ParseExact(tokens[1], GGATimeFormatters, CultureInfo.InvariantCulture, DateTimeStyles.None);

                        Latitude = Latitude.fromDecimalDMS(
                                Double.Parse(tokens[2]),
                                GeoSpatialTypes.ParseNorthSouth(tokens[3])
                        );

                        Longitude = Longitude.fromDecimalDMS(
                                Double.Parse(tokens[4]),
                                GeoSpatialTypes.ParseEastWest(tokens[5])
                        );

                        FixQuality = GpsFixTypeExtensions.Parse(tokens[6]);

                        TrackedSatellitesCount = Int32.Parse(tokens[7]);

                        HorizDilution = Double.Parse(tokens[8]);

                        Elevation = Double.Parse(tokens[9]);
                        UomElevation = GeoSpatialTypes.ParseUomElevation(tokens[10]);

                        GeoidHeight = Double.Parse(tokens[11]);
                        GeoUom = GeoSpatialTypes.ParseUomElevation(tokens[12]);

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


    public enum GpsFixType
    {
        NoFix = 0,
        GPS = 1,
        DGPS = 2,
        PPS = 3,
        FloatRTK = 5,
        RTK = 4,
        Estimated = 6,
        Manual = 7,
        Simulation = 8
    }


    public static class GpsFixTypeExtensions
    {
        public static GpsFixType Parse(String value)
        {
            switch (value.ToLower())
            {
                case "nofix":
                case "no fix":
                case "0": return GpsFixType.NoFix;
                case "gps":
                case "1": return GpsFixType.GPS;
                case "dgps":
                case "2": return GpsFixType.DGPS;
                case "pps":
                case "3": return GpsFixType.PPS;
                case "realtime":
                case "realtimekinematic":
                case "4": return GpsFixType.RTK;
                case "float":
                case "floatrtk":
                case "5": return GpsFixType.FloatRTK;
                case "estimate":
                case "estimated":
                case "6": return GpsFixType.Estimated;
                case "manual":
                case "7": return GpsFixType.Manual;
                case "simulate":
                case "simulation":
                case "8": return GpsFixType.Simulation;
                default: throw new Exception("Invalid GpsFixType Name: " + value);
            }
        }
        
        public static String ToStringX(this GpsFixType type)
        {
            switch (type)
            {
                case GpsFixType.NoFix: return "No Fix";
                case GpsFixType.GPS: return "GPS";
                case GpsFixType.DGPS: return "GPS (DIFF)";
                case GpsFixType.PPS: return "PPS";
                case GpsFixType.RTK: return "Real Time Kinematic";
                case GpsFixType.FloatRTK: return "Float RTK";
                case GpsFixType.Estimated: return "Estimated";
                case GpsFixType.Manual: return "Manual";
                case GpsFixType.Simulation: return "Simulation";
                default: throw new Exception("Unknown Type");
            }
        }

        public static String ToStringF(this GpsFixType type)
        {
            switch (type)
            {
                case GpsFixType.NoFix: return "0 (No Fix)";
                case GpsFixType.GPS: return "1 (GPS)";
                case GpsFixType.DGPS: return "2 (DGPS)";
                case GpsFixType.PPS: return "3 (PPS)";
                case GpsFixType.RTK: return "5 (RTK)";
                case GpsFixType.FloatRTK: return "4 (Float RTK)";
                case GpsFixType.Estimated: return "6 (Estimated)";
                case GpsFixType.Manual: return "7 (Manual)";
                case GpsFixType.Simulation: return "8 (Simulation)";
                default: throw new Exception("Unknown Type");
            }
        }
    }
}
