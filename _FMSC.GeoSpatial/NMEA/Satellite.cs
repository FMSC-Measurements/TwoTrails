using FMSC.GeoSpatial.Types;

namespace FMSC.GeoSpatial.NMEA
{
    public class Satellite
    {
        public int NmeaID { get; }
        public float? Elevation { get; }
        public float? Azimuth { get; }
        public float? SRN { get; }
        public GnssType GnssType { get; }
        public bool IsSBAS { get { return GnssType.IsSBAS(); } }

        public Satellite(int nmeaId, float? elevation, float? aziumuth, float? srn)
        {
            NmeaID = nmeaId;
            Elevation = elevation;
            Azimuth = aziumuth;
            SRN = srn;
            this.GnssType = GeoSpatialTypes.ParseNmeaId(NmeaID);
        }

        public Satellite(int nmeaId, string elevation, string aziumuth, string srn)
        {
            NmeaID = nmeaId;
            Elevation = string.IsNullOrEmpty(elevation) ? null : (float?)float.Parse(elevation);
            Azimuth = string.IsNullOrEmpty(aziumuth) ? null : (float?)float.Parse(aziumuth);
            SRN = string.IsNullOrEmpty(srn) ? null : (float?)float.Parse(srn);
            this.GnssType = GeoSpatialTypes.ParseNmeaId(NmeaID);
        }
    }
}
