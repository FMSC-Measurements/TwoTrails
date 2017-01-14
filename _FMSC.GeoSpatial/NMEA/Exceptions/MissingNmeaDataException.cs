using System;
using System.Collections.Generic;
using System.Text;

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
            base(String.Format("Missing {0} sentence data.", sentenceID))
        { }
    }
}
