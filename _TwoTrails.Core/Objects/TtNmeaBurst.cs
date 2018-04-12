using FMSC.Core.Utilities;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.NMEA;
using FMSC.GeoSpatial.NMEA.Sentences;
using FMSC.GeoSpatial.Types;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwoTrails.Core
{
    public class TtNmeaBurst : TtObject
    {
        public String PointCN { get; }

        public bool IsUsed { get { return Get<bool>(); } set { Set(value); } }

        public DateTime TimeCreated { get; }
        public DateTime FixTime { get; }

        public double? MagVar { get; }
        public EastWest? MagVarDir { get; }

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


        public double HorizDilution { get; }
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
                            sb.Append($"{prn}_");

                        _UsedSatelliteIDsString = sb.ToString();
                    }
                    else
                        _UsedSatelliteIDsString = String.Empty;
                }

                return _UsedSatelliteIDsString;
            }
        }

        public int UsedSatelliteIDsCount { get { return UsedSatelliteIDs.Count; } }

        public List<Satellite> SatellitesInView { get; private set; }

        private string _SatellitesInViewString;
        public string SatellitesInViewString
        {
            get
            {
                return _SatellitesInViewString ?? (_SatellitesInViewString = String.Join(
                    "_", SatellitesInView.Select(sat => $"{ sat.NmeaID };{ sat.Elevation };{ sat.Azimuth };{ sat.SRN };{ (int)sat.GnssType }_")));
            }

            set
            {
                if (_SatellitesInViewString != value && !String.IsNullOrEmpty(value))
                {
                    SatellitesInView = value.Split('_')
                        .Select(satstr =>
                        {
                            if (!String.IsNullOrWhiteSpace(satstr))
                            {
                                String[] tokens = satstr.Split(';');
                                if (tokens.Length > 4)
                                {
                                    if (Int32.TryParse(tokens[0], out int nmeaID))
                                    {
                                        return new Satellite(nmeaID,
                                                ParseEx.ParseFloatN(tokens[1]),
                                                ParseEx.ParseFloatN(tokens[2]),
                                                ParseEx.ParseFloatN(tokens[3]));
                                    }
                                } 
                            }

                            return null;
                        }).Where(sat => sat != null).ToList();

                    _SatellitesInViewString = value;
                }
            }
        }

        public Fix Fix { get; }
        public Mode Mode { get; }

        public double HDOP { get; }
        public double PDOP { get; }
        public double VDOP { get; }

        public bool HasDifferential { get { return FixQuality == GpsFixType.DGPS; } }

        private double? _X = null, _Y = null;
        private int? _Zone;



        public TtNmeaBurst(TtNmeaBurst burst)
        {
            TimeCreated = burst.TimeCreated;
            PointCN = burst.PointCN;
            IsUsed = burst.IsUsed;

            _Position = new GeoPosition(burst.Position);

            FixTime = burst.FixTime;
            GroundSpeed = burst.GroundSpeed;
            TrackAngle = burst.TrackAngle;
            MagVar = burst.MagVar;
            MagVarDir = burst.MagVarDir;

            Mode = burst.Mode;
            Fix = burst.Fix;
            UsedSatelliteIDs = new List<int>(burst.UsedSatelliteIDs);
            PDOP = burst.PDOP;
            HDOP = burst.HDOP;
            VDOP = burst.VDOP;

            FixQuality = burst.FixQuality;
            TrackedSatellitesCount = burst.TrackedSatellitesCount;
            HorizDilution = burst.HorizDilution;
            GeoidHeight = burst.GeoidHeight;
            GeoUom = burst.GeoUom;

            SatellitesInViewCount = burst.SatellitesInViewCount;

            if (burst.SatellitesInView != null && burst.SatellitesInView.Count > 0)
                SatellitesInView = burst.SatellitesInView;
            else if (burst.SatellitesInViewString != null)
                SatellitesInViewString = burst.SatellitesInViewString;
        }

        private TtNmeaBurst(String cn, DateTime timeCreated, String pointCN, bool used,
                       GeoPosition position, DateTime fixTime, double groundSpeed, double trackAngle,
                       double? magVar, EastWest? magVarDir, Mode mode, Fix fix,
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
            HorizDilution = horizDilution;
            GeoidHeight = geoidHeight;
            GeoUom = geoUom;

            SatellitesInViewCount = numberOfSatellitesInView;
        }

        public TtNmeaBurst(String cn, DateTime timeCreated, String pointCN, bool used,
                       GeoPosition position, DateTime fixTime, double groundSpeed, double trackAngle,
                       double? magVar, EastWest? magVarDir, Mode mode, Fix fix,
                       List<int> satsUsed, double pdop, double hdop, double vdop, GpsFixType fixQuality,
                       int trackedSatellites, double horizDilution, double geoidHeight, UomElevation geoUom,
                       int numberOfSatellitesInView, List<Satellite> satellitesInView) : this(
                            cn, timeCreated, pointCN, used, position, fixTime, groundSpeed, trackAngle,
                            magVar, magVarDir, mode, fix, satsUsed, pdop, hdop, vdop, fixQuality,
                            trackedSatellites, horizDilution, geoidHeight, geoUom, numberOfSatellitesInView)
        {
            SatellitesInView = satellitesInView;
        }

        public TtNmeaBurst(String cn, DateTime timeCreated, String pointCN, bool used,
                       GeoPosition position, DateTime fixTime, double groundSpeed, double trackAngle,
                       double? magVar, EastWest? magVarDir, Mode mode, Fix fix,
                       List<int> satsUsed, double pdop, double hdop, double vdop, GpsFixType fixQuality,
                       int trackedSatellites, double horizDilution, double geoidHeight, UomElevation geoUom,
                       int numberOfSatellitesInView, string satellitesInViewString) : this(
                            cn, timeCreated, pointCN, used, position, fixTime, groundSpeed, trackAngle,
                            magVar, magVarDir, mode, fix, satsUsed, pdop, hdop, vdop, fixQuality,
                            trackedSatellites, horizDilution, geoidHeight, geoUom, numberOfSatellitesInView)
        {
            SatellitesInViewString = satellitesInViewString;
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
            return UTMTools.ConvertLatLonToUTM(Position);
        }

        public UTMCoords GetUTM(int zone)
        {
            UTMCoords coords = UTMTools.ConvertLatLonToUTM(Position, zone);

            _X = coords.X;
            _Y = coords.Y;
            _Zone = zone;

            return coords;
        }
    }
}
