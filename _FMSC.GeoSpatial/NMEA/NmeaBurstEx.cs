using System;
using System.Collections.Generic;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.NMEA.Sentences.Base;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using System.Linq;
using FMSC.GeoSpatial.NMEA.Exceptions;

namespace FMSC.GeoSpatial.NMEA
{
    public class NmeaBurstEx : INmeaBurst
    {
        public static readonly TalkerID[] priorityIds = new TalkerID[] {
            TalkerID.GP,
            TalkerID.GN,
            TalkerID.GL,
            TalkerID.GA,
            TalkerID.GB,
            TalkerID.BD,
            TalkerID.QZ,
            TalkerID.LC,
            TalkerID.II,
            TalkerID.IN,
            TalkerID.EC,
            TalkerID.CD
        };

        //GGA Sentence
        private Dictionary<TalkerID, GGASentence> gga = new Dictionary<TalkerID, GGASentence>();
        private bool ggaIsValidated = false, ggaIsValid = false;

        //GSA Sentence
        private Dictionary<TalkerID, GSASentence> gsa = new Dictionary<TalkerID, GSASentence>();
        private bool gsaIsValidated = false, gsaIsValid = false;

        //RMC Sentence
        private Dictionary<TalkerID, RMCSentence> rmc = new Dictionary<TalkerID, RMCSentence>();
        private bool rmcIsValidated = false, rmcIsValid = false;

        //GSV Sentence
        private Dictionary<TalkerID, GSVSentence> gsv = new Dictionary<TalkerID, GSVSentence>();
        private bool gsvIsValidated = false, gsvIsValid = false;

        private List<NmeaSentence> allSenteneces = new List<NmeaSentence>();



        public bool IsValid
        {
            get
            {
                return ggaIsValidated && ggaIsValid &&
                    gsaIsValidated && gsaIsValid &&
                    rmcIsValidated && rmcIsValid &&
                    gsvIsValidated && gsvIsValid;
            }
        }
        public bool IsFull { get { return rmc.Count > 0 && gsa.Count > 0 && gga.Count > 0 && GsvIsFull; } }
        private bool GsvIsFull { get { return gsv.Count > 0 && gsv.Values.All(s => s.HasAllMessages); } }


        public bool HasPosition
        {
            get
            {
                return Validate(SentenceID.GGA) && ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).HasPosition ||
                    Validate(SentenceID.RMC) && ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).HasPosition;
            }
        }
        public GeoPosition Position
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).Position;
                else if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).Position;

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }

        public NorthSouth LatDir
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).Latitude.Hemisphere;
                else if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).Latitude.Hemisphere;

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }
        public double Latitude
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).Latitude.toSignedDecimal();
                else if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).Latitude.toSignedDecimal();

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }

        public EastWest LongDir
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).Longitude.Hemisphere;
                else if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).Longitude.Hemisphere;

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }
        public double Longitude
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).Longitude.toSignedDecimal();
                else if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).Longitude.toSignedDecimal();

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }

        public UomElevation UomElevation
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).UomElevation;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }
        public double Elevation
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).Elevation;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }


        public DateTime FixTime
        {
            get
            {
                if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).FixTime;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }

        public double MagVar
        {
            get
            {
                if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).MagVar;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }
        public EastWest MagVarDir
        {
            get
            {
                if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).MagVarDir;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }

        public double TrackAngle
        {
            get
            {
                if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).TrackAngle;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }
        public double GroundSpeed
        {
            get
            {
                if (Validate(SentenceID.RMC))
                    return ((RMCSentence)GetSentenceByPriority(SentenceID.RMC)).GroundSpeed;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }
        

        public double GeoidHeight
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).GeoidHeight;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public UomElevation GeoUom
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).GeoUom;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }
        public GpsFixType FixQuality
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).FixQuality;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public double HorizDilution
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return ((GGASentence)GetSentenceByPriority(SentenceID.GGA)).HorizDilution;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public IEnumerable<Satellite> SatellitesInView
        {
            get
            {
                if (Validate(SentenceID.GSV))
                    return gsv.Values.SelectMany(g => g.Satellites);
                else
                    throw new MissingNmeaDataException(SentenceID.GSV);
            }
        }
        public int SatellitesInViewCount
        {
            get
            {
                if (Validate(SentenceID.GSV))
                    return gsv.Values.Sum(g => g.NumberOfSatellitesInView);
                else
                    throw new MissingNmeaDataException(SentenceID.GSV);
            }
        }
        public int TrackedSatellitesCount
        {
            get
            {
                if (Validate(SentenceID.GGA))
                    return gga.Values.Sum(g => g.TrackedSatellitesCount);
                else
                    throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }
        public IEnumerable<int> UsedSatelliteIDs
        {
            get
            {
                if (Validate(SentenceID.GSA))
                    return gsa.Values.SelectMany(g => g.SatellitesUsed);
                else
                    throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }
        public int UsedSatellitesCount
        {
            get
            {
                if (Validate(SentenceID.GSA))
                    return gsa.Values.Sum(g => g.SatellitesUsed.Count);
                else
                    throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }


        public Fix Fix
        {
            get
            {
                if (Validate(SentenceID.GSA))
                    return ((GSASentence)GetSentenceByPriority(SentenceID.GSA)).Fix;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }
        public Mode Mode
        {
            get
            {
                if (Validate(SentenceID.GSA))
                    return ((GSASentence)GetSentenceByPriority(SentenceID.GSA)).Mode;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }

        public double HDOP
        {
            get
            {
                if (Validate(SentenceID.GSA))
                    return ((GSASentence)GetSentenceByPriority(SentenceID.GSA)).HDOP;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }
        public double PDOP
        {
            get
            {
                if (Validate(SentenceID.GSA))
                    return ((GSASentence)GetSentenceByPriority(SentenceID.GSA)).PDOP;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }
        public double VDOP
        {
            get
            {
                if (Validate(SentenceID.GSA))
                    return ((GSASentence)GetSentenceByPriority(SentenceID.GSA)).VDOP;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }


        public NmeaBurstEx() { }

        
        public NmeaSentence GetSentenceByPriority(SentenceID id)
        {
            switch (id)
            {
                case SentenceID.GGA:
                    foreach (TalkerID tid in priorityIds)
                        if (gga.ContainsKey(tid))
                            return gga[tid];
                    break;
                case SentenceID.GSA:
                    foreach (TalkerID tid in priorityIds)
                        if (gsa.ContainsKey(tid))
                            return gsa[tid];
                    break;
                case SentenceID.RMC:
                    foreach (TalkerID tid in priorityIds)
                        if (rmc.ContainsKey(tid))
                            return rmc[tid];
                    break;
                case SentenceID.GSV:
                    foreach (TalkerID tid in priorityIds)
                        if (gsv.ContainsKey(tid))
                            return gsv[tid];
                    break;
            }

            throw new Exception("No Sentence found");
        }
        
        public NmeaSentence AddNmeaSentence(string sentence, TalkerID? tid = null)
        {
            if (sentence == null)
                throw new ArgumentNullException();

            if (!IsFull && NmeaSentence.validateChecksum(sentence))
            {
                TalkerID talkerID = tid != null ? (TalkerID)tid : NmeaIDTools.ParseTalkerID(sentence);

                switch (NmeaIDTools.ParseSentenceID(sentence))
                {
                    case SentenceID.GGA:
                        {
                            if (!gga.ContainsKey(talkerID))
                            {
                                ggaIsValidated = false;

                                GGASentence ggaSentence = new GGASentence(sentence);
                                gga.Add(talkerID, ggaSentence);
                                allSenteneces.Add(ggaSentence);
                                return ggaSentence;
                            }
                            else
                                throw new ExcessiveStringException(SentenceID.GGA);
                        }
                    case SentenceID.GSA:
                        {
                            gsaIsValidated = false;

                            GSASentence gsaSentence;
                            if (!gsa.ContainsKey(talkerID))
                            {
                                gsaSentence = new GSASentence(sentence);
                                gsa.Add(talkerID, gsaSentence);
                                allSenteneces.Add(gsaSentence);
                                return gsaSentence;
                            }
                            else
                            {
                                gsaSentence = gsa[talkerID];
                                gsaSentence.Parse(sentence);
                                return gsaSentence;
                            }
                        }
                    case SentenceID.RMC:
                        {
                            if (!rmc.ContainsKey(talkerID))
                            {
                                rmcIsValidated = false;

                                RMCSentence rmcSentence = new RMCSentence(sentence);
                                rmc.Add(talkerID, rmcSentence);
                                allSenteneces.Add(rmcSentence);
                                return rmcSentence;
                            }
                            else
                            {
                                throw new ExcessiveStringException(SentenceID.RMC);
                            }
                        }
                    case SentenceID.GSV:
                        {
                            gsvIsValidated = false;

                            GSVSentence gsvSentence;
                            if (!gsv.ContainsKey(talkerID) || gsv[talkerID].TotalMessageCount == 0)
                            {
                                gsvSentence = new GSVSentence(sentence);
                                gsv.Add(talkerID, gsvSentence);
                                allSenteneces.Add(gsvSentence);
                                return gsvSentence;
                            }
                            else
                            {
                                gsvSentence = gsv[talkerID];
                                if (gsvSentence.MessageCount < gsvSentence.TotalMessageCount)
                                {
                                    gsvSentence.Parse(sentence);
                                    return gsvSentence;
                                }
                                else
                                {
                                    throw new ExcessiveStringException(SentenceID.GSV);
                                }
                            }
                        }
                }
            }

            return null;
        }
        

        public UTMCoords GetUTM(int zone = 0)
        {
            if (Validate(SentenceID.GGA))
                return UTMTools.ConvertLatLonToUTM(((PositionSentence)GetSentenceByPriority(SentenceID.GGA)).Position, zone);
            else if (Validate(SentenceID.RMC))
                return UTMTools.ConvertLatLonToUTM(((PositionSentence)GetSentenceByPriority(SentenceID.RMC)).Position, zone);

            throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
        }



        public bool Validate()
        {
            return Validate(SentenceID.GGA) &&
                Validate(SentenceID.GSA) &&
                Validate(SentenceID.GSV) &&
                Validate(SentenceID.RMC);
        }

        public bool Validate(SentenceID id)
        {
            switch (id)
            {
                case SentenceID.GGA:
                    if (ggaIsValidated)
                        return ggaIsValid;
                    else
                    {
                        if (ggaIsValidated = gga.Count > 0)
                        {
                            ggaIsValid = gga.Values.All(s => s.IsValid);
                            return ggaIsValid;
                        }
                    }
                    break;
                case SentenceID.GSA:
                    if (gsaIsValidated)
                        return gsaIsValid;
                    else
                    {
                        if (gsaIsValidated = gsa.Count > 0)
                        {
                            gsaIsValid = gsa.Values.All(s => s.IsValid);
                            return gsaIsValid;
                        }
                    }
                    break;
                case SentenceID.GSV:
                    if (gsvIsValidated)
                        return gsvIsValid;
                    else
                    {
                        if (gsvIsValidated = gsv.Count > 0)
                        {
                            gsvIsValid = gsv.Values.All(s => s.IsValid);
                            return gsvIsValid;
                        }
                    }
                    break;
                case SentenceID.RMC:
                    if (rmcIsValidated)
                        return rmcIsValid;
                    else
                    {
                        if (rmcIsValidated = rmc.Count > 0)
                        {
                            rmcIsValid = rmc.Values.All(s => s.IsValid);
                            return rmcIsValid;
                        }
                    }
                    break;
            }

            return false;
        }

        public override string ToString()
        {
            if (IsValid)
            {
                return $"[{FixTime}] Valid: True | Lat: {Latitude} | Lon: {Latitude} | Elev: {Elevation}";
            }
            else
            {
                return $@"[{DateTime.Now}] Valid: False | {(HasPosition ? $"(Lat: {Latitude} | Lon: {Latitude} | Elev: {Elevation}) |" : "No Position |")} 
rmc: {Validate(SentenceID.RMC)} | gga: {Validate(SentenceID.GGA)} | gsa: {Validate(SentenceID.GSA)} | gsv: {Validate(SentenceID.GSV)}";
            }
        }
    }
}
