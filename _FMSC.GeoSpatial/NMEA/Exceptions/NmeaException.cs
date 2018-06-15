using System;

namespace FMSC.GeoSpatial.NMEA.Exceptions
{
    public class NmeaException : Exception
    {
        public NmeaException() : base("Unknown NMEA Exception") { }

        public NmeaException(String message) : base(message) { }
    }
}
