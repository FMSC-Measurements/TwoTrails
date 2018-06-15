using System;

namespace FMSC.Core.Xml.GPX
{
    public class Point
    {
        public string ID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        public DateTime? Time { get; set; }
        public double? MagVar { get; set; }
        public double? GeoidHeight { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public string Source { get; set; }
        public string Link { get; set; }
        public string Symmetry { get; set; }
        public int? Fix { get; set; }
        public int? SatteliteNum { get; set; }
        public double? HDOP { get; set; }
        public double? PDOP { get; set; }
        public double? VDOP { get; set; }
        public double? AgeOfData { get; set; }
        public string DGpsID { get; set; }
        public string Extensions { get; set; }


        public Point(double lat, double lon, double? alt = null)
        {
            ID = Guid.NewGuid().ToString();
            Latitude = lat;
            Longitude = lon;
            Altitude = alt;
        }
    }

    public enum PointType
    {
        WayPoint,
        RoutePoint,
        TrackPoint
    }
}
