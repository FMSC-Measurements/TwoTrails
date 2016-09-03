using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
