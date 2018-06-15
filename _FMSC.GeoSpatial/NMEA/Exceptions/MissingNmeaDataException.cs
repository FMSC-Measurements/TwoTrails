using System;

namespace FMSC.GeoSpatial.NMEA.Exceptions
{
    public class MissingNmeaDataException : NmeaException
    {
        public MissingNmeaDataException() :
            base("Missing Nmea sentence data.")
        { }

        public MissingNmeaDataException(String message) :
            base(message)
        { }

        public MissingNmeaDataException(SentenceID sentenceID) :
            base($"Missing {sentenceID} sentence data.")
        { }
    }
}
