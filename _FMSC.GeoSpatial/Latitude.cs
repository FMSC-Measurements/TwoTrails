using System;

namespace FMSC.GeoSpatial
{
    [Serializable]
    public class Latitude : DMS
    {
        public NorthSouth Hemisphere { get; set; }


        public Latitude() : base() { }

        public Latitude(Latitude latitude) : base(latitude)
        {
            this.Hemisphere = latitude.Hemisphere;
        }

        public Latitude(DMS coord, NorthSouth hemisphere) : base(coord)
        {
            this.Hemisphere = hemisphere;
        }

        public Latitude(double dms) : base(dms)
        {
            this.Hemisphere = (dms < 0) ? NorthSouth.South : NorthSouth.North;
        }

        public Latitude(double dms, NorthSouth hemisphere) : base(dms)
        {
            this.Hemisphere = hemisphere;
        }

        public Latitude(int degrees, int minutes, NorthSouth hemisphere) : base(degrees, minutes)
        {
            this.Hemisphere = hemisphere;
        }

        public Latitude(int degrees, int minutes, double seconds, NorthSouth hemisphere) : base(degrees, minutes, seconds)
        {
            this.Hemisphere = hemisphere;
        }
        

        public static Latitude fromDecimalDMS(double dms)
        {
            return new Latitude(DMS.FromDecimalDMS(dms), dms < 0 ? NorthSouth.South : NorthSouth.North);
        }

        public static Latitude fromDecimalDMS(double dms, NorthSouth hemisphere)
        {
            return new Latitude(DMS.FromDecimalDMS(dms), hemisphere);
        }


        public double toSignedDecimal()
        {
            return ToDecimalDegrees() * ((Hemisphere == NorthSouth.South) ? -1 : 1);
        }
        

        public override string ToString()
        {
            return $"{base.ToString()} {Hemisphere.ToStringAbv()}";
        }
    }
}
