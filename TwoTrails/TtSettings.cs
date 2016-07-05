using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails
{
    public class TtSettings : ITtSettings
    {
        public IMetadataSettings MetadataSettings { get; set; }
        public IDeviceSettings DeviceSettings { get; set; }

        public String DeviceID
        {
            get
            {
                return "DeviceID";
            }
        }

        public String Region { get; set; } = String.Empty;

        public String District { get; set; } = String.Empty;


        public TtSettings(IDeviceSettings deviceSettings, IMetadataSettings metadataSettings)
        {
            DeviceSettings = deviceSettings;
            MetadataSettings = metadataSettings;
        }


        public TtMetadata CreateDefaultMetadata()
        {
            return MetadataSettings.CreateDefaultMetadata();
        }

        public TtGroup CreateDefaultGroup()
        {
            return new TtGroup(Consts.EmptyGuid, "Main Group", "Default Group", GroupType.General);
        }

        public TtProjectInfo CreateProjectInfo(Version programVersion)
        {
            return new TtProjectInfo("Unamed Project", String.Empty, Region, String.Empty, District,
                programVersion, programVersion, TwoTrailsSchema.SchemaVersion, DeviceID, DateTime.Now);
        }

    }
}
