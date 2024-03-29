﻿using System;
using System.Collections.Specialized;

namespace TwoTrails.Core
{
    public interface ITtSettings
    {
        IMetadataSettings MetadataSettings { get; }
        IDeviceSettings DeviceSettings { get; }
        IPolygonGraphicSettings PolygonGraphicSettings { get; }

        String UserName { get; }
        String DeviceName { get; }
        String AppVersion { get; }
        
        String Region { get; set; }
        String District { get; set; }

        TtGroup CreateDefaultGroup();

        TtProjectInfo CreateProjectInfo(Version programVersion);


        StringCollection GetRecentProjects();
    }
}
