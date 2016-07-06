using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TwoTrails.Core
{
    public interface ITtSettings
    {
        IMetadataSettings MetadataSettings { get; }
        IDeviceSettings DeviceSettings { get; }

        String DeviceID { get; }
        
        String Region { get; set; }
        String District { get; set; }

        TtGroup CreateDefaultGroup();

        TtProjectInfo CreateProjectInfo(Version programVersion);


        StringCollection GetRecentProjects();
    }
}
