using FMSC.Core.ComponentModel;
using System;

namespace TwoTrails.Core
{
    public class TtProjectInfo : BaseModel
    {
        public String Name { get { return Get<string>(); } set { Set(value); } }
        public String Description { get { return Get<string>(); } set { Set(value); } }
        public String Region { get { return Get<string>(); } set { Set(value); } }
        public String Forest { get { return Get<string>(); } set { Set(value); } }
        public String District { get { return Get<string>(); } set { Set(value); } }
        public Version DbVersion { get; }
        public String Version { get; }
        public String CreationVersion { get; }
        public String CreationDeviceID { get; }
        public DateTime CreationDate { get; }


        public TtProjectInfo(String name, String desc, String region, String forest, String district,
            String version, String creationVersion, Version dbVersion, String deviceID, DateTime date)
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

        public override bool Equals(object obj)
        {
            TtProjectInfo info = obj as TtProjectInfo;

            return info != null &&
                info.Name == Name &&
                info.Description == Description &&
                info.Region == Region &&
                info.Forest == Forest &&
                info.District == District;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                Region.GetHashCode() ^
                Forest.GetHashCode() ^
                District.GetHashCode();
        }
    }
}
