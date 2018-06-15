using System;
using FMSC.Core;
using TwoTrails.Core;

namespace TwoTrails
{
    public class MetadataSettings : IMetadataSettings
    {
        private const String DATUM = "Datum";
        private const String DEC_TYPE = "DecType";
        private const String DISTANCE = "Distance";
        private const String ELEVATION = "Elevation";
        private const String MAG_DEC = "MagDec";
        private const String SLOPE = "Slope";
        private const String ZONE = "Zone";

        public Datum Datum { get; private set; } = Datum.NAD83;

        public DeclinationType DecType { get; private set; } = DeclinationType.MagDec;

        public Distance Distance { get; private set; } = Distance.FeetTenths;

        public Distance Elevation { get; private set; } = Distance.Meters;

        public double MagDec { get; private set; } = 0;

        public Slope Slope { get; private set; } = Slope.Degrees;

        public int Zone { get; private set; } = 13;


        public MetadataSettings()
        {
            Datum = (Datum)Properties.Settings.Default[DATUM];
            DecType = (DeclinationType)Properties.Settings.Default[DEC_TYPE];
            Distance = (Distance)Properties.Settings.Default[DISTANCE];
            Elevation = (Distance)Properties.Settings.Default[ELEVATION];
            MagDec = (double)Properties.Settings.Default[MAG_DEC];
            Slope = (Slope)Properties.Settings.Default[SLOPE];
            Zone = (int)Properties.Settings.Default[ZONE];
        }


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

            Properties.Settings.Default[DATUM] = (int)Datum;
            Properties.Settings.Default[DEC_TYPE] = (int)DecType;
            Properties.Settings.Default[DISTANCE] = (int)Distance;
            Properties.Settings.Default[ELEVATION] = (int)Elevation;
            Properties.Settings.Default[MAG_DEC] = (int)MagDec;
            Properties.Settings.Default[SLOPE] = (int)Slope;
            Properties.Settings.Default[ZONE] = (int)Zone;
            
            Properties.Settings.Default.Save();
        }
    }
}
