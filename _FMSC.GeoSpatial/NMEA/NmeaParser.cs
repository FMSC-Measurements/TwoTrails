using FMSC.GeoSpatial.NMEA.Exceptions;
using FMSC.GeoSpatial.NMEA.Sentences.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FMSC.GeoSpatial.NMEA
{
    public delegate void BurstReceivedEvent(INmeaBurst burst);
    public delegate void NmeaReceivedEvent(NmeaSentence sentence);

    public class NmeaParser<TNmeaBurst> where TNmeaBurst : INmeaBurst, new()
    {
        public event BurstReceivedEvent BurstReceived;
        public event NmeaReceivedEvent NmeaReceived;

        public List<TalkerID> UsedTalkerIDs { get; private set; }
        private INmeaBurst burst;


        public NmeaParser() : this((TalkerID[])Enum.GetValues(typeof(TalkerID))) { }

        public NmeaParser(TalkerID talkerID)
        {
            UsedTalkerIDs = new List<TalkerID>();
            UsedTalkerIDs.Add(talkerID);
        }

        public NmeaParser(IEnumerable<TalkerID> talkerIDs)
        {
            UsedTalkerIDs = new List<TalkerID>(talkerIDs);
        }


        public bool Parse(String nmea)
        {
            bool usedNmea = false;

            if (burst == null)
                burst = new TNmeaBurst();

            try
            {
                TalkerID tid = NmeaIDTools.ParseTalkerID(nmea);

                if (UsedTalkerIDs.Contains(tid))
                {
                    NmeaSentence sentence = burst.AddNmeaSentence(nmea, tid);

                    if (sentence != null &&
                        (!sentence.IsMultiString || ((MultiSentence)sentence).HasAllMessages))
                            NmeaReceived?.Invoke(sentence);

                    usedNmea = true;
                }
            }
            catch (ExcessiveStringException e)
            {
                Debug.WriteLine(e.Message, "NmeaParser:Parse");
            }

            if (burst.IsFull)
            {
                burst.Validate();
                BurstReceived?.Invoke(burst);
                burst = null;
            }

            return usedNmea;
        }

        public void Reset()
        {
            burst = null;
        }
    }
}
