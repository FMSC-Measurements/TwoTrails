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

        public static EastWest Parse(string value)
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

            string[] split = value.Split(' ');
            if (split.Length > 1)
                return Parse(split[0]);

            throw new Exception("Unknown EastWest");
        }
    }

}
