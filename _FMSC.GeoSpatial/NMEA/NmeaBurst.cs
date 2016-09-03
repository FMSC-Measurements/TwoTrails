﻿using FMSC.GeoSpatial.NMEA.Exceptions;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.NMEA.Sentences.Base;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMSC.GeoSpatial.NMEA
{
    public class NmeaBurst : INmeaBurst
    {
        //GGA Sentence
        private GGASentence gga;

        //GSA Sentence
        private GSASentence gsa;

        //RMC Sentence
        private RMCSentence rmc;

        //GSV Sentence
        private GSVSentence gsv;


        public NmeaBurst() { }

        public NmeaBurst(NmeaBurst burst)
        {
            gga = burst.gga;
            gsa = burst.gsa;
            rmc = burst.rmc;
            gsv = burst.gsv;
        }

        public NmeaSentence AddNmeaSentence(String sentence, TalkerID? tid = null)
        {
            if (sentence == null)
                throw new ArgumentNullException();

            if (!IsFull && NmeaSentence.validateChecksum(sentence))
            {
                switch (NmeaIDTools.ParseSentenceID(sentence))
                {
                    case SentenceID.GGA:
                        {
                            if (gga == null || !gga.IsValid)
                            {
                                gga = new GGASentence(sentence);
                                return gga;
                            }
                            else
                                throw new ExcessiveStringException(SentenceID.GGA);
                        }
                    case SentenceID.GSA:
                        {
                            if (gsa == null)
                            {
                                gsa = new GSASentence(sentence);
                                return gsa;
                            }
                            else
                            {
                                gsa.Parse(sentence);
                                return gsa;
                            }
                        }
                    case SentenceID.RMC:
                        {
                            if (rmc == null)
                            {
                                rmc = new RMCSentence(sentence);
                                return rmc;
                            }
                            else
                                throw new ExcessiveStringException(SentenceID.RMC);
                        }
                    case SentenceID.GSV:
                        {
                            if (gsv == null || gsv.TotalMessageCount == 0)
                            {
                                gsv = new GSVSentence(sentence);
                                return gsv;
                            }
                            else
                            {
                                if (gsv.MessageCount< gsv.TotalMessageCount)
                                {
                                    gsv.Parse(sentence);
                                    return gsv;
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

        

        public bool IsValid
        {
            get
            {
                return (gga != null && gsa != null && rmc != null && gsv != null &&
                  gga.IsValid && gsa.IsValid && rmc.IsValid && gsv.IsValid);
            }
        }
        
        public bool Validate(SentenceID id)
        {
            switch (id)
            {
                case SentenceID.GGA: return gga != null && gga.IsValid;
                case SentenceID.GSA: return gsa != null && gsa.IsValid;
                case SentenceID.GSV: return gsv != null && gsv.IsValid;
                case SentenceID.RMC: return rmc != null && rmc.IsValid;
            }

            return false;
        }

        public bool IsFull { get { return (gga != null && gsa != null && rmc != null && gsv != null && gsv.HasAllMessages); } }

        private bool Validate(NmeaSentence sentence)
        {
            return sentence != null && sentence.IsValid;
        }


        public DateTime FixTime
        {
            get
            {
                if (Validate(rmc))
                    return rmc.FixTime;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }

        public double MagVar
        {
            get
            {
                if (Validate(rmc))
                    return rmc.MagVar;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }

        public EastWest MagVarDir
        {
            get
            {
                if (Validate(rmc))
                    return rmc.MagVarDir;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }

        public double TrackAngle
        {
            get
            {
                if (Validate(rmc))
                    return rmc.TrackAngle;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }

        public double GroundSpeed
        {
            get
            {
                if (Validate(rmc))
                    return rmc.GroundSpeed;
                throw new MissingNmeaDataException(SentenceID.RMC);
            }
        }


        public GeoPosition Position
        {
            get
            {
                if (Validate(gga))
                    return gga.Position;
                else if (Validate(rmc))
                    return rmc.Position;

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }

        public bool HasPosition { get { return Validate(gga) && gga.HasPosition || Validate(rmc) && rmc.HasPosition; } }

        public double Latitude
        {
            get
            {
                if (Validate(gga))
                    return gga.Latitude.toSignedDecimal();
                else if (Validate(rmc))
                    return rmc.Latitude.toSignedDecimal();

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }

        public NorthSouth LatDir
        {
            get
            {
                if (Validate(gga))
                    return gga.Latitude.Hemisphere;
                else if (Validate(rmc))
                    return rmc.Latitude.Hemisphere;

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }

        public double Longitude
        {
            get
            {
                if (Validate(gga))
                    return gga.Longitude.toSignedDecimal();
                else if (Validate(rmc))
                    return rmc.Longitude.toSignedDecimal();

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }

        public EastWest LongDir
        {
            get
            {
                if (Validate(gga))
                    return gga.Longitude.Hemisphere;
                else if (Validate(rmc))
                    return rmc.Longitude.Hemisphere;

                throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
            }
        }


        public double Elevation
        {
            get
            {
                if (Validate(gga))
                    return gga.Elevation;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public UomElevation UomElevation
        {
            get
            {
                if (Validate(gga))
                    return gga.UomElevation;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        
        public UTMCoords GetUTM(int utmZone = 0)
        {
            if (Validate(gga))
                return UTMTools.convertLatLonToUTM(gga.Position, utmZone);
            else if (Validate(rmc))
                return UTMTools.convertLatLonToUTM(rmc.Position, utmZone);

            throw new MissingNmeaDataException("Missing RMC and GGA Sentences");
        }


        public double HorizDilution
        {
            get
            {
                if (Validate(gga))
                    return gga.HorizDilution;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public double GeoidHeight
        {
            get
            {
                if (Validate(gga))
                    return gga.GeoidHeight;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public UomElevation GeoUom
        {
            get
            {
                if (Validate(gga))
                    return gga.GeoUom;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public GpsFixType FixQuality
        {
            get
            {
                if (Validate(gga))
                    return gga.FixQuality;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public int TrackedSatellitesCount
        {
            get
            {
                if (Validate(gga))
                    return gga.TrackedSatellitesCount;
                throw new MissingNmeaDataException(SentenceID.GGA);
            }
        }

        public IEnumerable<Satellite> SatellitesInView
        {
            get
            {
                if (Validate(gsv))
                    return gsv.Satellites;
                throw new MissingNmeaDataException(SentenceID.GSV);
            }
        }

        public int SatellitesInViewCount
        {
            get
            {
                if (Validate(gsv))
                    return gsv.NumberOfSatellitesInView;
                throw new MissingNmeaDataException(SentenceID.GSV);
            }
        }

        public IEnumerable<int> UsedSatelliteIDs
        {
            get {
                if (Validate(gsa))
                    return gsa.SatellitesUsed;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }

        public int UsedSatellitesCount
        {
            get
            {
                if (Validate(gsa))
                    return gsa.SatellitesUsed.Count;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }


        public Fix Fix
        {
            get
            {
                if (Validate(gsa))
                    return gsa.Fix;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }

        public Mode Mode
        {
            get
            {
                if (Validate(gsa))
                    return gsa.Mode;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }

        public double HDOP
        {
            get {
                if (Validate(gsa))
                    return gsa.HDOP;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }

        public double PDOP
        {
            get
            {
                if (Validate(gsa))
                    return gsa.PDOP;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }

        public double VDOP
        {
            get
            {
                if (Validate(gsa))
                    return gsa.VDOP;
                throw new MissingNmeaDataException(SentenceID.GSA);
            }
        }

        public override string ToString()
        {
            if (IsValid)
            {
                return String.Format("[{0}] Valid: True | Lat: {1} | Lon: {2} | Elev: {3}",
                        FixTime, Latitude, Latitude, Elevation);
            }
            else
            {
                return String.Format("[{0}] Valid: False |{1} rmc: {2} | gga: {3} | gsa: {4} | gsv: {5}",
                        DateTime.Now,
                        HasPosition ? String.Format(" (Lat: {0} | Lon: {1} | Elev: {2}) |", Latitude, Latitude, Elevation) :
                                "No Position |",
                        Validate(SentenceID.RMC), Validate(SentenceID.GGA), Validate(SentenceID.GSA), Validate(SentenceID.GSV));
            }
        }
    }
}