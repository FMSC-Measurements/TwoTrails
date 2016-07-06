using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails
{
    public class TtSettings : ITtSettings
    {
        private static String DISTRICT = "District";
        private static String REGION = "Region";
        private static String RECENT_PROJECTS = "RecentProjects";

        public IMetadataSettings MetadataSettings { get; set; }
        public IDeviceSettings DeviceSettings { get; set; }

        public String DeviceID
        {
            get
            {
                return "DeviceID";
            }
        }

        private String _Region;
        public String Region
        {
            get
            {
                if (_Region == null)
                {
                    _Region = Properties.Settings.Default[REGION] as string;
                }

                return _Region;
            }

            set
            {
                _Region = value;
                Properties.Settings.Default[REGION] = value;
                Properties.Settings.Default.Save();
            }
        }

        private String _District;
        public String District
        {
            get
            {
                if (_District == null)
                {
                    _District = Properties.Settings.Default[DISTRICT] as string;
                }

                return _District;
            }

            set
            {
                _District = value;
                Properties.Settings.Default[DISTRICT] = value;
                Properties.Settings.Default.Save();
            }
        }


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
            String version = String.Format("PC: {0}", programVersion);
            return new TtProjectInfo("Unamed Project", String.Empty, Region, String.Empty, District,
                version, version, TwoTrailsSchema.SchemaVersion, DeviceID, DateTime.Now);
        }




        private StringCollection _RecentProjects;
        public StringCollection GetRecentProjects()
        {
            if (_RecentProjects == null)
            {
                _RecentProjects = Properties.Settings.Default[RECENT_PROJECTS] as StringCollection;

                if (_RecentProjects == null)
                    _RecentProjects = new StringCollection();
            }

            return _RecentProjects;
        }


        public void AddRecentProject(String filePath)
        {
            _RecentProjects.Remove(filePath);
            _RecentProjects.Insert(0, filePath);

            if (_RecentProjects.Count > 9)
            {
                _RecentProjects.RemoveAt(9);
            }

            Properties.Settings.Default[RECENT_PROJECTS] = _RecentProjects;
            Properties.Settings.Default.Save();
        }

        public void RemoveRecentProject(String filePath)
        {
            _RecentProjects.Remove(filePath);
            Properties.Settings.Default[RECENT_PROJECTS] = _RecentProjects;
            Properties.Settings.Default.Save();
        }
    }
}
