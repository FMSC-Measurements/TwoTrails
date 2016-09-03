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
}
