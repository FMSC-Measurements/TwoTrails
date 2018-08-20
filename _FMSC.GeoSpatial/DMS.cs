using System;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class DMS
    {
        protected const String DEGREE_SYMBOL  = "\u00b0";

        public Int32 Degrees { get; set; }
        public Int32 Minutes { get; set; }
        public Double Seconds { get; set; }


        public DMS() { }

        public DMS(int degrees, int minutes, double seconds = 0)
        {
            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
        }

        public DMS(double dms)
        {
            dms = Math.Abs(dms);

            Degrees = (int)dms;

            double decMin = (dms - Degrees) * 60d;

            Minutes = (int)decMin;
            Seconds = (decMin - Minutes) * 60d;
        }

        public DMS(DMS dms)
        {
            Degrees = dms.Degrees;
            Minutes = dms.Minutes;
            Seconds = dms.Seconds;
        }


        public double ToDecimalDegrees()
        {
            return Degrees + Minutes / 60d + Seconds / 3600d;
        }


        public static DMS FromDecimal(double decDegrees)
        {
            decDegrees = Math.Abs(decDegrees);

            int deg = (int)decDegrees;

            double decMin = (decDegrees - deg) * 60d;

            int min = (int)decMin;
            double sec = (decMin - min) * 60d;

            return new DMS(deg, min, sec);
        }

        public static DMS FromDecimalDMS(double decDMS)
        {
            decDMS = Math.Abs(decDMS / 100);

            int deg = (int)decDMS;

            double decMin = (decDMS - deg) * 100.0;

            int min = (int)decMin;
            double sec = (decMin - min) * 60d;

            return new DMS(deg, min, sec);
        }


        public override string ToString()
        {
            return String.Format("{0}{3} {1}{3} {2:0.##}{3}",
                Degrees,
                Minutes,
                Seconds,
                DEGREE_SYMBOL);
        }
    }
}
