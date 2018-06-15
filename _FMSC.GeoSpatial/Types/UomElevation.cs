using System;

namespace FMSC.GeoSpatial.Types
{
    public enum UomElevation
    {
        Feet = 0,
        Meters = 1
    }

    public static class UomElevationExtensions
    {
        public static string ToStringAbv(this UomElevation uomElevation)
        {
            switch (uomElevation)
            {
                case UomElevation.Feet: return "Ft";
                case UomElevation.Meters: return "M";
            }

            return String.Empty;
        }

        public static UomElevation Parse(string value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "f":
                case "ft":
                case "feet_inches":
                case "feet": return UomElevation.Feet;
                case "1":
                case "m":
                case "mt":
                case "meter":
                case "meters": return UomElevation.Meters;
            }

            string[] split = value.Split(' ');
            if (split.Length > 1)
                return Parse(split[0]);

            throw new Exception("Unknown EastWest");
        }
    }
}
