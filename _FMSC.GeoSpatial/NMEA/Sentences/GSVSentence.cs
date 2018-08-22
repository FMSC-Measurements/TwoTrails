using FMSC.GeoSpatial.NMEA.Sentences.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FMSC.GeoSpatial.NMEA.Sentences
{
    public class GSVSentence : MultiSentence
    {
        public override SentenceID SentenceID { get; } = SentenceID.GSV;

        public int NumberOfSatellitesInView { get; protected set; }

        private List<Satellite> _Satellites = new List<Satellite>();
        public ReadOnlyCollection<Satellite> Satellites { get; protected set; }

        public GSVSentence() { }

        public GSVSentence(string nmea) : base(nmea) { }


        public override bool Parse(string nmea)
        {
            if (Satellites == null)
                Satellites = new ReadOnlyCollection<Satellite>(_Satellites);

            if (IsNmeaValid(SentenceID, null, nmea) && base.Parse(nmea))
            {
                String[] tokens = nmea.Substring(0, nmea.IndexOf("*")).Split(',');

                if (tokens.Length > 3)
                {
                    try
                    {
                        int tMessageCount = Int32.Parse(tokens[1]);

                        if (this.TotalMessageCount == 0)
                        {
                            TotalMessageCount = tMessageCount;
                        }
                        else if (this.TotalMessageCount != tMessageCount)
                        {
                            throw new MismatchMessageCountException();
                        }

                        this.MessageCount++;

                        //ignore message id, assuming there are no duplicate messages

                        if (NumberOfSatellitesInView == 0)
                        {
                            NumberOfSatellitesInView = Int32.Parse(tokens[3]);
                        }


                        Satellite satellite;
                        for (int current = 4; current < 16 && current + 3 < tokens.Length; current += 4)
                        {
                            if (tokens[current] != null)
                            {
                                satellite = new Satellite(
                                        int.Parse(tokens[current]),
                                        tokens[current + 1],
                                        tokens[current + 2],
                                        tokens[current + 3]
                                );

                                if (!_Satellites.Any(s => s.NmeaID == satellite.NmeaID))
                                    _Satellites.Add(satellite);
                            }
                        }

                        IsValid = true;
                    }
                    catch (Exception ex)
                    {
                        IsValid = false;
                    }
                }
            }

            return IsValid;
        }
    }
}
