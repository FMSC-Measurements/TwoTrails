using FMSC.GeoSpatial.NMEA.Sentences.Base;
using FMSC.GeoSpatial.Types;
using System;
using System.Globalization;

namespace FMSC.GeoSpatial.NMEA.Sentences
{
    public class RMCSentence : PositionSentence
    {
        protected static readonly string[] RMCTimeFormatters = new string[] { "HHmmss.fff ddMMyy", "HHmmss.ff ddMMyy", "HHmmss.f ddMMyy", "HHmmss ddMMyy" };

        public override SentenceID SentenceID { get; } = SentenceID.RMC;

        public DateTime FixTime { get; protected set; }
        public Status Status { get; protected set; }
        public double GroundSpeed { get; protected set; } //groud speed in knots
        public double TrackAngle { get; protected set; }  //in degrees, true
        public double MagVar { get; protected set; }
        public EastWest MagVarDir { get; protected set; }



        public RMCSentence() { }

        public RMCSentence(string nmea) : base(nmea) { }


        public override bool Parse(string nmea)
        {
            if (base.Parse(nmea))
            {
                IsValid = false;
                String[] tokens = nmea.Substring(0, nmea.IndexOf("*")).Split(',');

                if (tokens.Length > 12 && tokens[1].Length > 0)
                {
                    try
                    {
                        FixTime = DateTime.ParseExact(
                            $"{tokens[1]} {tokens[9]}",
                            RMCTimeFormatters, CultureInfo.InvariantCulture,
                            DateTimeStyles.None);

                        Status = StatusExtensions.Parse(tokens[2]);

                        Latitude = Latitude.fromDecimalDMS(
                                Double.Parse(tokens[3]),
                                GeoSpatialTypes.ParseNorthSouth(tokens[4])
                        );

                        Longitude = Longitude.fromDecimalDMS(
                                Double.Parse(tokens[5]),
                                GeoSpatialTypes.ParseEastWest(tokens[6])
                        );

                        String token = tokens[7];
                        if (!String.IsNullOrEmpty(token))
                            GroundSpeed = Double.Parse(token);

                        token = tokens[8];
                        if (!String.IsNullOrEmpty(token))
                            TrackAngle = Double.Parse(token);

                        token = tokens[10];
                        if (!String.IsNullOrEmpty(token))
                            MagVar = Double.Parse(token);

                        token = tokens[11];
                        if (!String.IsNullOrEmpty(token))
                            MagVarDir = GeoSpatialTypes.ParseEastWest(token);

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

    public enum Status
    {
        Active = 0,
        Void = 1
    }

    public static class StatusExtensions
    {
        public static Status Parse(string value)
        {
            switch (value.ToLower())
            {
                case "a":
                case "active": return Status.Active;
                case "v":
                case "void": return Status.Void;
                default: throw new Exception("InIsValid Status Name: " + value);
            }
        }
    }
}
