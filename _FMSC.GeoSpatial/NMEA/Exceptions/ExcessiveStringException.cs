using System;
using System.Collections.Generic;
using System.Text;

namespace FMSC.GeoSpatial.NMEA.Exceptions
{
    public class ExcessiveStringException : NmeaException
    {
        public ExcessiveStringException(SentenceID sentenceID) :
            base(String.Format("An Excessive amount of {0} type strings were added to the burst.", sentenceID)){ }
    }
}
