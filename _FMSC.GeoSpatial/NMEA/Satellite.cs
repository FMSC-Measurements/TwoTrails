using FMSC.GeoSpatial.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace FMSC.GeoSpatial.NMEA
{
    public class Satellite
    {
        public int NmeaID { get; }
        public float Elevation { get; }
        public float Azimuth { get; }
        public float SRN { get; }
        public GnssType GnssType { get; }
        public bool IsSBAS { get { return GnssType.IsSBAS(); } }

        public Satellite(int nmeaId, float elevation, float aziumuth, float srn)
        {
            NmeaID = nmeaId;
            Elevation = elevation;
            Azimuth = aziumuth;
            SRN = srn;
            this.GnssType = GeoSpatialTypes.ParseNmeaId(NmeaID);
        }
    }
}
