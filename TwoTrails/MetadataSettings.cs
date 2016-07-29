using System;
using FMSC.Core;
using TwoTrails.Core;

namespace TwoTrails
{
    public class MetadataSettings : IMetadataSettings
    {
        public Datum Datum { get; private set; } = Datum.NAD83;

        public DeclinationType DecType { get; private set; } = DeclinationType.MagDec;

        public Distance Distance { get; private set; } = Distance.FeetTenths;

        public Distance Elevation { get; private set; } = Distance.Meters;

        public double MagDec { get; private set; } = 0;

        public Slope Slope { get; private set; } = Slope.Degrees;

        public int Zone { get; private set; } = 13;


        public TtMetadata CreateDefaultMetadata()
        {
            return new TtMetadata(Consts.EmptyGuid, "Default Meta", String.Empty,
                Zone, DecType, MagDec, Datum, Distance, Elevation, Slope,
                String.Empty, String.Empty, String.Empty, String.Empty);
        }

        public void SetSettings(TtMetadata metadata)
        {
            Datum = metadata.Datum;
            DecType = metadata.DecType;
            Distance = metadata.Distance;
            Elevation = metadata.Elevation;
            MagDec = metadata.MagDec;
            Slope = metadata.Slope;
            Zone = metadata.Zone;
        }
    }
}
