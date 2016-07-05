using System;

namespace FMSC.GeoSpatial.UTM
{
    [Serializable]
    public class UTMCoords
    {
        public Double X { get; set; }
        public Double Y { get; set; }

        public Int32 Zone { get; set; }


        public UTMCoords(double x, double y, int zone)
        {
            X = x;
            Y = y;
            Zone = zone;
        }
    }
}
