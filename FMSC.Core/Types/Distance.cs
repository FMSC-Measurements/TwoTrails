using System;

namespace FMSC.Core
{
    public enum Distance
    {
        FeetTenths = 0,
        Meters = 1,
        Chains = 2,
        Yards = 3
    }

    public static class DistanceExtensions
    {
        public static String ToStringAbv(this Distance dist)
        {
            switch (dist)
            {
                case Distance.FeetTenths: return "FtT";
                case Distance.Meters: return "M";
                case Distance.Chains: return "C";
                case Distance.Yards: return "Yd";
            }

            throw new ArgumentException();
        }
    }

    public static partial class Types
    {
        public static Distance ParseDistance(String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "ft":
                case "ftt":
                case "feet":
                case "feetin":      //legacy
                case "feetinches":  //legacy
                case "feettenths": return Distance.FeetTenths;
                case "1":
                case "m":
                case "mt":
                case "meters": return Distance.Meters;
                case "2":
                case "c":
                case "chains": return Distance.Chains;
                case "3":
                case "y":
                case "yd":
                case "yards": return Distance.Yards;
            }

            if (value.Length > 2 && value.Contains(" "))
                return ParseDistance(value.Split(' ')[0]);

            throw new Exception("Unknown Distance Type");
        }
    }
}
