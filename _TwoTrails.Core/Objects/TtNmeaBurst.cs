using FMSC.GeoSpatial;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core
{
    public class TtNmeaBurst : TtObject
    {
        public String PointCN { get; }

        public bool IsUsed { get { return Get<bool>(); } set { Set(value); } }

        public DateTime TimeCreated { get; }
        public DateTime FixTime { get; }

        public double MagVar { get; }
        public EastWest MagVarDir { get; }

        public double TrackAngle { get; }
        public double GroundSpeed { get; }


        private GeoPosition _Position;
        public GeoPosition Position { get { return new GeoPosition(_Position); } }
        public bool HasPosition { get { return Position != null; } }

        public double Latitude { get { return _Position.Latitude.toSignedDecimal(); } }
        public NorthSouth LatDir { get { return _Position.Latitude.Hemisphere; } }

        public double Longitude { get { return _Position.Longitude.toSignedDecimal(); } }
        public EastWest LonDir { get { return _Position.Longitude.Hemisphere; } }

        public double Elevation { get { return _Position.Elevation; } }
        public UomElevation UomElevation { get { return _Position.UomElevation; } }


        public double HorizDultion { get; }
        public double GeoidHeight { get; }
        public UomElevation GeoUom { get; }

        public GpsFixType FixQuality { get; }

        public int TrackedSatellitesCount { get; }
        public int SatellitesInViewCount { get; }

        public List<int> UsedSatelliteIDs { get; }

        private string _UsedSatelliteIDsString;
        public string UsedSatelliteIDsString
        {
            get
            {
                if (_UsedSatelliteIDsString == null)
                {
                    if (UsedSatelliteIDs.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (int prn in UsedSatelliteIDs)
                            sb.AppendFormat("{0}_", prn);

                        _UsedSatelliteIDsString = sb.ToString();
                    }
                    else
                        _UsedSatelliteIDsString = String.Empty;
                }

                return _UsedSatelliteIDsString;
            }
        }

        public int UsedSatelliteIDsCount { get { return UsedSatelliteIDs.Count; } }

        public Fix Fix { get; }
        public Mode Mode { get; }

        public double HDOP { get; }
        public double PDOP { get; }
        public double VDOP { get; }

        public bool HasDifferential { get { return FixQuality == GpsFixType.DGPS; } }

        private double? _X = null, _Y = null;
        private int? _Zone;




        public TtNmeaBurst(String cn, DateTime timeCreated, String pointCN, bool used,
                       GeoPosition position, DateTime fixTime, double groundSpeed, double trackAngle,
                       double magVar, EastWest magVarDir, Mode mode, Fix fix,
                       List<int> satsUsed, double pdop, double hdop, double vdop, GpsFixType fixQuality,
                       int trackedSatellites, double horizDilution, double geoidHeight, UomElevation geoUom,
                       int numberOfSatellitesInView) : base(cn)
        {
            TimeCreated = timeCreated;
            PointCN = pointCN;
            IsUsed = used;

            _Position = position;

            FixTime = fixTime;
            GroundSpeed = groundSpeed;
            TrackAngle = trackAngle;
            MagVar = magVar;
            MagVarDir = magVarDir;

            Mode = mode;
            Fix = fix;
            UsedSatelliteIDs = satsUsed;
            PDOP = pdop;
            HDOP = hdop;
            VDOP = vdop;

            FixQuality = fixQuality;
            TrackedSatellitesCount = trackedSatellites;
            HorizDultion = horizDilution;
            GeoidHeight = geoidHeight;
            GeoUom = geoUom;

            SatellitesInViewCount = numberOfSatellitesInView;
        }




        public double GetX(int zone)
        {
            if (_Zone == null || _X == null || _Zone != zone)
                GetUTM(zone);

            return (double)_X;
        }


        public double GetY(int zone)
        {
            if (_Zone == null || _Y == null || _Zone != zone)
                GetUTM(zone);

            return (double)_Y;
        }


        public UTMCoords GetTrueUTM()
        {
            return UTMTools.convertLatLonToUTM(Position);
        }

        public UTMCoords GetUTM(int zone)
        {
            UTMCoords coords = UTMTools.convertLatLonToUTM(Position, zone);

            _X = coords.X;
            _Y = coords.Y;
            _Zone = zone;

            return coords;
        }
    }
}
