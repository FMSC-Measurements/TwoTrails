using FMSC.GeoSpatial.Types;
using System;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class GeoPosition : Position
    {
        public Double Elevation { get; set; }
        public UomElevation UomElevation { get; private set; } = UomElevation.Meters;

        public GeoPosition() : base() { }

        public GeoPosition(Position position) : base(position) { }

        public GeoPosition(Position position, double elevation, UomElevation uomElevation) : base(position)
        {
            this.Elevation = elevation;
            this.UomElevation = uomElevation;
        }

        public GeoPosition(GeoPosition position) : base(position)
        {
            Elevation = position.Elevation;
            UomElevation = position.UomElevation;
        }

        public GeoPosition(double latitude, double longitude) : base(latitude, longitude) { }

        public GeoPosition(double latitude, double longitude, double elevation, UomElevation uomElevation)
            : base(latitude, longitude)
        {
            this.Elevation = elevation;
            this.UomElevation = uomElevation;
        }

        public GeoPosition(double latitude, NorthSouth latDir, double longitude, EastWest lonDir)
            : base(latitude, latDir, longitude, lonDir) { }

        public GeoPosition(double latitude, NorthSouth latDir, double longitude, EastWest lonDir, double elevation, UomElevation uomElevation)
            : base(latitude, latDir, longitude, lonDir)
        {
            this.Elevation = elevation;
            this.UomElevation = uomElevation;
        }

        public GeoPosition(Latitude latitude, Longitude longitude) : base(latitude, longitude) { }

        public GeoPosition(Latitude latitude, Longitude longitude, double elevation, UomElevation uomElevation)
            : base(latitude, longitude)
        {
            this.Elevation = elevation;
            this.UomElevation = uomElevation;
        }


        public void SetUomElevation(UomElevation uomElevation)
        {
            UomElevation = uomElevation;
        }

        public void SetAndConvertElevation(UomElevation uomElevation)
        {
            Elevation = GeoTools.ConvertElevation(Elevation, uomElevation, UomElevation);
            UomElevation = uomElevation;
        }

        public override string ToString()
        {
            return $"{base.ToString()} Elev: {Elevation}({UomElevation.ToStringAbv()})";
        }
    }
}
