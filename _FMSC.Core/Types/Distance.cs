using System;

namespace FMSC.Core
{
    public enum Distance
    {
        FeetTenths = 0,
        FeethInches = 1,
        Meters = 2,
        Chains = 3,
        Yards = 4
    }

    public static class DistanceExtensions
    {
        public static String ToStringAbv(this Distance dist)
        {
            switch (dist)
            {
                case Distance.FeetTenths: return "FtT";
                case Distance.FeethInches: return "FtIn";
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
                case "ftt":
                case "feettenths": return Distance.FeetTenths;
                case "1":
                case "fti":
                case "ftin":
                case "feetinches": return Distance.FeethInches;
                case "2":
                case "m":
                case "mt":
                case "meters": return Distance.Meters;
                case "3":
                case "c":
                case "chains": return Distance.Chains;
                case "4":
                case "y":
                case "yd":
                case "yards": return Distance.Yards;
            }

            throw new Exception("Unknown Distance Type");
        }
    }
}
