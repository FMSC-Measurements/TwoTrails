using System;

namespace FMSC.GeoSpatial
{
    public enum NorthSouth
    {
        North = 0,
        South = 1
    }

    public static class NorthSouthExtentions
    {
        public static String ToStringAbv(this NorthSouth northSouth)
        {
            switch (northSouth)
            {
                case NorthSouth.North: return "N";
                case NorthSouth.South: return "S";
            }

            throw new ArgumentException();
        }

        public static NorthSouth Parse(string value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "n":
                case "north": return NorthSouth.North;
                case "1":
                case "s":
                case "south": return NorthSouth.South;
            }

            string[] split = value.Split(' ');
            if (split.Length > 1)
                return Parse(split[0]);

            throw new Exception("Unknown NorthSouth");
        }
    }
}
