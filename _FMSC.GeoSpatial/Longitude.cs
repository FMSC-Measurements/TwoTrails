using System;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class Longitude : DMS
    {
        public EastWest Hemisphere { get; set; }


        public Longitude() : base() { }

        public Longitude(Longitude Longitude) : base(Longitude)
        {
            this.Hemisphere = Longitude.Hemisphere;
        }

        public Longitude(DMS coord, EastWest hemisphere) : base(coord)
        {
            this.Hemisphere = hemisphere;
        }

        public Longitude(double dms) : base(dms)
        {
            this.Hemisphere = (dms < 0) ? EastWest.West : EastWest.East;
        }

        public Longitude(double dms, EastWest hemisphere) : base(dms)
        {
            this.Hemisphere = hemisphere;
        }

        public Longitude(int degrees, int minutes, EastWest hemisphere) : base(degrees, minutes)
        {
            this.Hemisphere = hemisphere;
        }

        public Longitude(int degrees, int minutes, double seconds, EastWest hemisphere) : base(degrees, minutes, seconds)
        {
            this.Hemisphere = hemisphere;
        }


        public static Longitude fromDecimalDMS(double dms)
        {
            return new Longitude(DMS.FromDecimalDMS(dms), dms < 0 ? EastWest.East : EastWest.West);
        }

        public static Longitude fromDecimalDMS(double dms, EastWest hemisphere)
        {
            return new Longitude(DMS.FromDecimalDMS(dms), hemisphere);
        }


        public double toSignedDecimal()
        {
            return ToDecimalDegrees() * ((Hemisphere == EastWest.West) ? -1 : 1);
        }


        public override string ToString()
        {
            return $"{base.ToString()} {Hemisphere.ToStringAbv()}";
        }
    }
}
