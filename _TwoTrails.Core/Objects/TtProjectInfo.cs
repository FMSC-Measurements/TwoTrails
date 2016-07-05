using System;

namespace TwoTrails.Core
{
    public class TtProjectInfo
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public String Region { get; set; }
        public String Forest { get; set; }
        public String District { get; set; }
        public Version Version { get; set; }
        public Version CreationVersion { get; }
        public Version DbVersion { get; }
        public String CreationDeviceID { get; }
        public DateTime CreationDate { get; }


        public TtProjectInfo(String name, String desc, String region, String forest, String district,
            Version version, Version creationVersion, Version dbVersion, String deviceID, DateTime date)
        {
            Name = name;
            Description = desc;
            Region = region;
            Forest = forest;
            District = district;
            Version = version;
            CreationVersion = creationVersion;
            DbVersion = dbVersion;
            CreationDeviceID = deviceID;
            CreationDate = date;
        }

        public TtProjectInfo(TtProjectInfo info)
        {
            Name = info.Name;
            Description = info.Description;
            Region = info.Region;
            Forest = info.Forest;
            District = info.District;
            Version = info.Version;
            CreationVersion = info.CreationVersion;
            DbVersion = info.DbVersion;
            CreationDeviceID = info.CreationDeviceID;
            CreationDate = info.CreationDate;
        }
    }
}
