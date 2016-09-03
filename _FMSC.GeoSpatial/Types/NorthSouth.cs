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
}
