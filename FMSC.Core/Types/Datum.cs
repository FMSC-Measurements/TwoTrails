using System;

namespace FMSC.Core
{
    public enum Datum
    {
        NAD83 = 0,
        WGS84 = 1,
        ITRF = 2,
        NAD27 = 3,
        Local = 4
    }

    public static partial class Types
    {
        public static Datum ParseDatum(String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "nad83": return Datum.NAD83;
                case "1":
                case "wgs84": return Datum.WGS84;
                case "2":
                case "itrf": return Datum.ITRF;
                case "3":
                case "nad27": return Datum.NAD27;
                case "4":
                case "local": return Datum.Local;
            }

            throw new Exception("Unknown Datum");
        }
    }
}
