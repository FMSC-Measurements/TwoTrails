using System;
using System.Collections.Generic;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class Extent
    {
        public Position NorthEast { get; set; }
        public Position SouthWest { get; set; }

        public Double North { get { return NorthEast.Latitude.toSignedDecimal(); } }
        public Double South { get { return SouthWest.Latitude.toSignedDecimal(); } }
        public Double East { get { return NorthEast.Longitude.toSignedDecimal(); } }
        public Double West { get { return SouthWest.Longitude.toSignedDecimal(); } }

        public Extent(Position northEast, Position southWest)
        {
            this.NorthEast = northEast;
            this.SouthWest = southWest;
        }

        public Extent(double north, double east, double south, double west)
        {
            this.NorthEast = new Position(north, east);
            this.SouthWest = new Position(south, west);
        }


        public class Builder
        {
            List<Position> positions = new List<Position>();

            public void Include(Position position)
            {
                positions.Add(position);
            }

            public void Include(Extent extent)
            {
                positions.Add(extent.NorthEast);
                positions.Add(extent.SouthWest);
            }

            public Extent Build()
            {
                double north = -90, south = 90, east = -180, west = 180;
                double lat, lon;

                if (positions.Count < 1)
                    throw new Exception("No positions");

                foreach (Position pos in positions)
                {
                    lat = pos.Latitude.toSignedDecimal();
                    lon = pos.Longitude.toSignedDecimal();

                    if (lat > north)
                        north = lat;

                    if (lat < south)
                        south = lat;

                    if (lon > east)
                        east = lon;

                    if (lon < west)
                        west = lon;
                }

                return new Extent(north, east, south, west);
            }
        }
    }
}
