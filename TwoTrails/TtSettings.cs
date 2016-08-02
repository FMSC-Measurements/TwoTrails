using CSUtil.ComponentModel;
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
    public class TtSettings : NotifyPropertyChangedEx, ITtSettings
    {
        private static String DISTRICT = "District";
        private static String REGION = "Region";
        private static String RECENT_PROJECTS = "RecentProjects";
        private static String ADVANCED_MODE = "AdvancedMode";

        public IMetadataSettings MetadataSettings { get; set; }
        public IDeviceSettings DeviceSettings { get; set; }

        public String UserName { get { return Environment.UserName; } }

        public String DeviceName { get { return Environment.MachineName; } }

        private String _Region;
        public String Region
        {
            get {  return _Region ?? (_Region = Properties.Settings.Default[REGION] as string); }

            set
            {
                SetField(ref _Region, value);
                Properties.Settings.Default[REGION] = value;
                Properties.Settings.Default.Save();
            }
        }

        private String _District;
        public String District
        {
            get { return _District ?? (_District = Properties.Settings.Default[DISTRICT] as string); }

            set
            {
                SetField(ref _District, value);
                Properties.Settings.Default[DISTRICT] = value;
                Properties.Settings.Default.Save();
            }
        }

        private bool _IsAdvancedMode;
        public bool IsAdvancedMode
        {
            get { return _IsAdvancedMode; }

            set
            {
                SetField(ref _IsAdvancedMode, value);
                Properties.Settings.Default[ADVANCED_MODE] = value;
                Properties.Settings.Default.Save();
            }
        }
        

        public TtSettings(IDeviceSettings deviceSettings, IMetadataSettings metadataSettings)
        {
            DeviceSettings = deviceSettings;
            MetadataSettings = metadataSettings;

#if DEBUG
            _IsAdvancedMode = true;
#else
            _IsAdvancedMode = (bool)Properties.Settings.Default[ADVANCED_MODE];
#endif
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
                version, version, TwoTrailsSchema.SchemaVersion, DeviceName, DateTime.Now);
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
