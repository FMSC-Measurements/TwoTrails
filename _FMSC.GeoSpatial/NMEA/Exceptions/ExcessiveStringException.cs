namespace FMSC.GeoSpatial.NMEA.Exceptions
{
    public class ExcessiveStringException : NmeaException
    {
        public ExcessiveStringException(SentenceID sentenceID) :
            base($"An Excessive amount of {sentenceID} type strings were added to the burst."){ }
    }
}
