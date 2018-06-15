using FMSC.GeoSpatial.Types;
using System;

namespace FMSC.GeoSpatial.NMEA.Sentences.Base
{
    [Serializable]
    public abstract class PositionSentence : NmeaSentence
    {
        public Latitude Latitude { get; protected set; }
        public Longitude Longitude { get; protected set; }

        private double? _Elevation;
        public Double Elevation
        {
            get { return _Elevation != null ? (double)_Elevation : 0; }
            protected set { _Elevation = value; }
        }

        public UomElevation UomElevation { get; protected set; } = UomElevation.Meters;


        private GeoPosition _Position;
        public GeoPosition Position
        {
            get
            {
                if (_Position == null && HasPosition)
                    _Position = new GeoPosition(Latitude, Longitude, Elevation, UomElevation);

                return _Position;
            }
        }


        public bool IsNortherHemisphere { get { return Latitude != null && Latitude.Hemisphere == NorthSouth.North; } }

        public bool IsWesternHemisphere { get { return Longitude != null && Longitude.Hemisphere == EastWest.West; } }

        public bool HasPosition { get { return Latitude != null && Longitude != null; } }

        public bool HasElevation { get { return _Elevation != null; } }



        public PositionSentence() { }

        public PositionSentence(String nmea) : base(nmea) { }
    }
}
