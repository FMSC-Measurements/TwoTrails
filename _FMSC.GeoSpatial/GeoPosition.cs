using FMSC.Core;
using System;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class GeoPosition : Position
    {
        public Double Elevation { get; set; }
        public Distance UomElevation { get; private set; } = Distance.Meters;

        public GeoPosition() : base() { }

        public GeoPosition(Position position) : base(position) { }

        public GeoPosition(Position position, double elevation, Distance uomElevation) : base(position)
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

        public GeoPosition(double latitude, double longitude, double elevation, Distance uomElevation)
            : base(latitude, longitude)
        {
            this.Elevation = elevation;
            this.UomElevation = uomElevation;
        }

        public GeoPosition(double latitude, NorthSouth latDir, double longitude, EastWest lonDir)
            : base(latitude, latDir, longitude, lonDir) { }

        public GeoPosition(double latitude, NorthSouth latDir, double longitude, EastWest lonDir, double elevation, Distance uomElevation)
            : base(latitude, latDir, longitude, lonDir)
        {
            this.Elevation = elevation;
            this.UomElevation = uomElevation;
        }

        public GeoPosition(Latitude latitude, Longitude longitude) : base(latitude, longitude) { }

        public GeoPosition(Latitude latitude, Longitude longitude, double elevation, Distance uomElevation)
            : base(latitude, longitude)
        {
            this.Elevation = elevation;
            this.UomElevation = uomElevation;
        }


        public void SetUomElevation(Distance uomElevation)
        {
            UomElevation = uomElevation;
        }

        public void SetAndConvertElevation(Distance uomElevation)
        {
            Elevation = Core.Convert.Distance(Elevation, uomElevation, UomElevation);
        }

        public override string ToString()
        {
            return String.Format("{0} Elev: {1}({2})", base.ToString(), Elevation, UomElevation.ToStringAbv());
        }
    }
}
