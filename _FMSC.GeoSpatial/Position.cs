using System;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class Position
    {
        public Latitude Latitude { get; set; } = new Latitude(0);
        public Longitude Longitude { get; set; } = new Longitude(0);

        public bool IsNorth { get { return Latitude.Hemisphere == NorthSouth.North; } }
        public bool IsWest { get { return Longitude.Hemisphere == EastWest.West; } }


        public Position() { }

        public Position(Position position)
        {
            this.Latitude = position.Latitude;
            this.Longitude = position.Longitude;
        }

        public Position(Latitude latitude, Longitude longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public Position(double latitude, double longitude)
        {
            SetPosition(latitude, null, longitude, null);
        }

        public Position(double latitude, NorthSouth latDir, double longitude, EastWest lonDir)
        {
            SetPosition(latitude, latDir, longitude, lonDir);
        }


        protected void SetPosition(Double latitude, NorthSouth? latDir, Double longitude, EastWest? lonDir)
        {
            if (latDir != null)
            {
                this.Latitude = new Latitude(latitude, (NorthSouth)latDir);
            }
            else
            {
                this.Latitude = new Latitude(latitude);
            }

            if (lonDir != null)
            {
                this.Longitude = new Longitude(longitude, (EastWest)lonDir);
            }
            else
            {
                this.Longitude = new Longitude(longitude);
            }
        }

        public override string ToString()
        {
            return $"Lat: {Latitude} Lon: {Longitude}";
        }
    }
}
