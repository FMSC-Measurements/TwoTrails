using System;
using System.Collections.Generic;

namespace FMSC.Core.Xml.KML
{
    public struct Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        
        public Coordinates(double lat, double lon, double? alt = null)
        {
            Latitude = lat;
            Longitude = lon;
            Altitude = alt;
        }

        public override bool Equals(object obj)
        {
            if (obj is Coordinates)
                return this == (Coordinates)obj;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator==(Coordinates c1, Coordinates c2)
        {
            return c1.Latitude == c2.Latitude && c1.Longitude == c2.Longitude && c1.Altitude == c2.Altitude;
        }

        public static bool operator !=(Coordinates c1, Coordinates c2)
        {
            return c1.Latitude != c2.Latitude || c1.Longitude != c2.Longitude || c1.Altitude != c2.Altitude;
        }
    }

    public class CoordinalExtent
    {
        public double East;
        public double West;
        public double North;
        public double South;

        public CoordinalExtent(double e, double w, double n, double s)
        {
            East = e;
            West = w;
            North = n;
            South = s;
        }

        public class Builder
        {
            List<Coordinates> coords = new List<Coordinates>();

            public void Include(Coordinates position)
            {
                coords.Add(position);
            }

            public void Include(CoordinalExtent extent)
            {
                coords.Add(new Coordinates(extent.North, extent.West));
                coords.Add(new Coordinates(extent.South, extent.East));
            }

            public CoordinalExtent Build()
            {
                double north = -90, south = 90, east = -180, west = 180;

                if (coords.Count < 1)
                    throw new Exception("No Coordinates");

                foreach (Coordinates c in coords)
                {
                    if (c.Latitude > north)
                        north = c.Latitude;

                    if (c.Latitude < south)
                        south = c.Latitude;

                    if (c.Longitude > east)
                        east = c.Longitude;

                    if (c.Longitude < west)
                        west = c.Longitude;
                }

                return new CoordinalExtent(north, east, south, west);
            }
        }
    }
}
