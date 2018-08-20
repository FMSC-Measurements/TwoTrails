using FMSC.GeoSpatial.NMEA.Exceptions;
using System;

namespace FMSC.GeoSpatial.NMEA.Sentences.Base
{
    [Serializable]
    public abstract class MultiSentence : NmeaSentence
    {
        public int TotalMessageCount { get; protected set; }
        public int MessageCount { get; protected set; }

        sealed public override bool IsMultiString { get; } = true;


        public override bool IsValid
        {
            get { return base.IsValid && TotalMessageCount > 0 && (TotalMessageCount == MessageCount); }
        }
        
        public bool HasAllMessages
        {
            get { return TotalMessageCount > 0 && TotalMessageCount == MessageCount; }
        }


        public MultiSentence() { }

        public MultiSentence(String nmea) : base(nmea) { }


        public override bool Parse(String nmea)
        {
            if (nmea != null)
            {
                if (RawNmea == null)
                {
                    RawNmea = nmea;
                }
                else
                {
                    RawNmea = $"{RawNmea}\n{nmea}";
                }

                return true;
            }

            return false;
        }


        public class MismatchMessageCountException : NmeaException
        {
            public MismatchMessageCountException() :
                base ("Current GSV string total message count does not equal previous message's count number.")
            { }
        }
    }
}
