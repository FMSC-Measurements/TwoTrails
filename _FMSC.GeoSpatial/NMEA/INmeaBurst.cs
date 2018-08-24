using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.NMEA.Sentences.Base;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;

namespace FMSC.GeoSpatial.NMEA
{
    public interface INmeaBurst
    {
        NmeaSentence AddNmeaSentence(string sentence, TalkerID? tid = null);

        bool IsValid { get; }
        bool Validate();
        bool Validate(SentenceID id);

        bool IsFull { get; }

        DateTime FixTime { get; }

        double MagVar { get; }
        EastWest MagVarDir { get; }

        double TrackAngle { get; }

        double GroundSpeed { get; }


        GeoPosition Position { get; }
        bool HasPosition { get; }

        double Latitude { get; }
        NorthSouth LatDir { get; }

        double Longitude { get; }
        EastWest LongDir { get; }

        double Elevation { get; }
        UomElevation UomElevation { get; }
        
        UTMCoords GetUTM(int utmZone = 0);


        double HorizDilution { get; }
        double GeoidHeight { get; }

        UomElevation GeoUom { get; }

        GpsFixType FixQuality { get; }

        int TrackedSatellitesCount { get; }

        IEnumerable<Satellite> SatellitesInView { get; }
        int SatellitesInViewCount { get; }

        IEnumerable<int> UsedSatelliteIDs { get; }
        int UsedSatellitesCount { get; }

        Fix Fix { get; }

        Mode Mode { get; }

        double HDOP { get; }
        double PDOP { get; }
        double VDOP { get; }
    }
}
