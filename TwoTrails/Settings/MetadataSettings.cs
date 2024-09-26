using FMSC.Core;
using FMSC.GeoSpatial;
using TwoTrails.Core;

namespace TwoTrails.Settings
{
    public class MetadataSettings : IMetadataSettings
    {
        private const string DATUM = "Datum";
        private const string DEC_TYPE = "DecType";
        private const string DISTANCE = "Distance";
        private const string ELEVATION = "Elevation";
        private const string MAG_DEC = "MagDec";
        private const string SLOPE = "Slope";
        private const string ZONE = "Zone";

        public Datum Datum { get; set; } = Datum.WGS84;

        public DeclinationType DecType { get; set; } = DeclinationType.MagDec;

        public Distance Distance { get; set; } = Distance.FeetTenths;

        public Distance Elevation { get; set; } = Distance.Meters;

        public double MagDec { get; set; } = 0;

        public Slope Slope { get; set; } = Slope.Percent;

        public int Zone { get; set; } = 13;


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
            return new TtMetadata(Consts.EmptyGuid, "Default Meta", string.Empty,
                Zone, DecType, MagDec, Datum, Distance, Elevation, Slope,
                string.Empty, string.Empty, string.Empty, string.Empty);
        }

        public void SetMetadataSettings(TtMetadata metadata)
        {
            Datum = metadata.Datum;
            DecType = metadata.DecType;
            Distance = metadata.Distance;
            Elevation = metadata.Elevation;
            MagDec = metadata.MagDec;
            Slope = metadata.Slope;
            Zone = metadata.Zone;

            SaveSettings();
        }

        public void SaveSettings()
        {
            Properties.Settings.Default[DATUM] = (int)Datum;
            Properties.Settings.Default[DEC_TYPE] = (int)DecType;
            Properties.Settings.Default[DISTANCE] = (int)Distance;
            Properties.Settings.Default[ELEVATION] = (int)Elevation;
            Properties.Settings.Default[MAG_DEC] = MagDec;
            Properties.Settings.Default[SLOPE] = (int)Slope;
            Properties.Settings.Default[ZONE] = Zone;

            Properties.Settings.Default.Save();
        }
    }
}
