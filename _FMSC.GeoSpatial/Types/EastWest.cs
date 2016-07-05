using System;

namespace FMSC.GeoSpatial
{
    public enum EastWest
    {
        East = 0,
        West = 1
    }

    public static class EastWestExtentions
    {
        public static String ToStringAbv(this EastWest eastWest)
        {
            switch (eastWest)
            {
                case EastWest.East: return "E";
                case EastWest.West: return "W";
            }

            throw new ArgumentException();
        }
    }

    public static partial class GeoSpatialTypes
    {
        public static EastWest ParseEastWest(String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "e":
                case "east": return EastWest.East;
                case "1":
                case "w":
                case "west": return EastWest.West;
            }

            throw new Exception("Unknown EastWest");
        }
    }
}
