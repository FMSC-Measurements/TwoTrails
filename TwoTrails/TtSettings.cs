using CSUtil.ComponentModel;
using System;
using System.Collections.Specialized;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails
{
    public class TtSettings : NotifyPropertyChangedEx, ITtSettings
    {
        private const String DISTRICT = "District";
        private const String REGION = "Region";
        private const String RECENT_PROJECTS = "RecentProjects";
        private const String ADVANCED_MODE = "AdvancedMode";
        private const String OPEN_FOLDER_ON_EXPORT = "OpenFolderOnExport";
        private const String LAST_UPDATE_CHECK = "LastUpdateCheck";
        private const String UPGRADE_REQUIRED = "UpgradeRequired";
        private const String SORT_POLYS_BY_NAME = "SortPolysByName";

        public IMetadataSettings MetadataSettings { get; set; }
        public IDeviceSettings DeviceSettings { get; set; }
        public IPolygonGraphicSettings PolygonGraphicSettings { get; set; }

        public String UserName => Environment.UserName;

        public String DeviceName => Environment.MachineName;

        public String AppVersion => AppInfo.Version.ToString();

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

        private bool? _OpenFolderOnExport;
        public bool OpenFolderOnExport
        {
            get { return _OpenFolderOnExport ?? (bool)(_OpenFolderOnExport = (bool?)Properties.Settings.Default[OPEN_FOLDER_ON_EXPORT]); }

            set
            {
                SetField(ref _OpenFolderOnExport, value);
                Properties.Settings.Default[OPEN_FOLDER_ON_EXPORT] = value;
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

        private DateTime? _LastUpdateCheck;
        public DateTime? LastUpdateCheck
        {
            get
            {
                if (_LastUpdateCheck == null && DateTime.TryParse(Properties.Settings.Default[LAST_UPDATE_CHECK] as string, out DateTime time))
                {
                    _LastUpdateCheck = time;
                }

                return _LastUpdateCheck;
            }

            set
            {
                SetField(ref _LastUpdateCheck, value);
                Properties.Settings.Default[LAST_UPDATE_CHECK] = value.ToString();
                Properties.Settings.Default.Save();
            }
        }

        private bool _UpgradeRequired = true;
        public bool UpgradeRequired
        {
            get { return _UpgradeRequired; }

            set
            {
                SetField(ref _UpgradeRequired, value);
                Properties.Settings.Default[UPGRADE_REQUIRED] = value;
                Properties.Settings.Default.Save();
            }
        }

        private bool _SortPolysByName;
        public bool SortPolysByName
        {
            get { return _SortPolysByName; }

            set
            {
                SetField(ref _SortPolysByName, value);
                Properties.Settings.Default[SORT_POLYS_BY_NAME] = value;
                Properties.Settings.Default.Save();
            }
        }


        public TtSettings(IDeviceSettings deviceSettings, IMetadataSettings metadataSettings, IPolygonGraphicSettings polyGraphicSettings)
        {
            DeviceSettings = deviceSettings;
            MetadataSettings = metadataSettings;
            PolygonGraphicSettings = polyGraphicSettings;

            _UpgradeRequired = (bool)Properties.Settings.Default[UPGRADE_REQUIRED];

            if (UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                UpgradeRequired = false;
            }

#if DEBUG
            _IsAdvancedMode = true;
#else
            _IsAdvancedMode = (bool)Properties.Settings.Default[ADVANCED_MODE];
#endif

            _SortPolysByName = (bool)Properties.Settings.Default[SORT_POLYS_BY_NAME];
        }


        public TtMetadata CreateDefaultMetadata()
        {
            return MetadataSettings.CreateDefaultMetadata();
        }

        public TtGroup CreateDefaultGroup()
        {
            return new TtGroup(Consts.EmptyGuid, Consts.DefaultGroupName, Consts.DefaultGroupDesc, GroupType.General);
        }

        public TtProjectInfo CreateProjectInfo(Version programVersion)
        {
            String version = $"PC: {programVersion}";
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
