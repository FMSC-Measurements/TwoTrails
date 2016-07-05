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
    }

    public static partial class GeoSpatialTypes
    {
        public static NorthSouth ParseNorthSouth(String value)
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

            throw new Exception("Unknown NorthSouth");
        }
    }
}
