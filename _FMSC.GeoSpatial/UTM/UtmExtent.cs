using System;
using System.Collections.Generic;
using System.Windows;

namespace FMSC.GeoSpatial.UTM
{
    [Serializable]
    public class UtmExtent
    {
        public UTMCoords NorthEast { get; set; }
        public UTMCoords SouthWest { get; set; }
        public Int32 Zone { get; private set; }

        public Double North { get { return NorthEast.Y; } }
        public Double South { get { return SouthWest.Y; } }
        public Double East { get { return NorthEast.X; } }
        public Double West { get { return SouthWest.X; } }

        public UtmExtent(UTMCoords northEast, UTMCoords southWest)
        {
            if (northEast.Zone != southWest.Zone)
                throw new Exception("Mismatched Zone");

            this.NorthEast = northEast;
            this.SouthWest = southWest;
            this.Zone = northEast.Zone;
        }

        public UtmExtent(double north, double east, double south, double west, int zone)
        {
            this.NorthEast = new UTMCoords(east, north, zone);
            this.SouthWest = new UTMCoords(west, south, zone);
            this.Zone = zone;
        }


        public class Builder
        {
            List<double> xpos = new List<double>();
            List<double> ypos = new List<double>();
            int zone;

            public Builder(int zone)
            {
                this.zone = zone;
            }

            public void Include(double x, double y)
            {
                xpos.Add(x);
                ypos.Add(y);
            }

            public void Include(Point point)
            {
                xpos.Add(point.X);
                ypos.Add(point.Y);
            }

            public void Include(IEnumerable<Point> points)
            {
                foreach (Point p in points)
                    Include(p);
            }

            public void Include(UTMCoords position)
            {
                if (position.Zone != zone)
                    throw new Exception("Mismatched Zone");

                xpos.Add(position.X);
                ypos.Add(position.Y);
            }

            public void Include(IEnumerable<UTMCoords> coords)
            {
                foreach (UTMCoords c in coords)
                    Include(c);
            }

            public void Include(UtmExtent extent)
            {
                if (extent.NorthEast.Zone != zone || extent.SouthWest.Zone != zone)
                    throw new Exception("Mismatched Zone");

                xpos.Add(extent.East);
                xpos.Add(extent.West);
                ypos.Add(extent.North);
                ypos.Add(extent.South);
            }

            public void Include(IEnumerable<UtmExtent> extents)
            {
                foreach (UtmExtent e in extents)
                    Include(e);
            }

            public UtmExtent Build()
            {
                double north = double.NegativeInfinity,
                    south = double.PositiveInfinity,
                    east = double.NegativeInfinity,
                    west = double.PositiveInfinity;

                if (xpos.Count < 1)
                    throw new Exception("No positions");

                foreach (double y in ypos)
                {
                    if (y > north)
                        north = y;

                    if (y < south)
                        south = y;
                }

                foreach (double x in xpos)
                {
                    if (x > east)
                        east = x;

                    if (x < west)
                        west = x;
                }

                return new UtmExtent(north, east, south, west, zone);
            }
        }
    }
}
