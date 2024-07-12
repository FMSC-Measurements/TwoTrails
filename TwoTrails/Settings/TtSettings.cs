using FMSC.Core.ComponentModel;
using System;
using System.Collections.Specialized;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.DAL;
using Point = System.Drawing.Point;

namespace TwoTrails.Settings
{
    public class TtSettings : BaseModel, ITtSettings
    {
        private const string DISTRICT = "District";
        private const string REGION = "Region";
        private const string RECENT_PROJECTS = "RecentProjects";
        private const string ADVANCED_MODE = "AdvancedMode";
        private const string USE_ADVANCED_PROCESSING = "UseAdvancedProcessing";
        private const string OPEN_FOLDER_ON_EXPORT = "OpenFolderOnExport";
        private const string LAST_UPDATE_CHECK = "LastUpdateCheck";
        private const string UPGRADE_REQUIRED = "UpgradeRequired";
        private const string SORT_POLYS_BY_NAME = "SortPolysByName";
        private const string WINDOW_STARTUP_LOCATION = "WindowStartupLocation";
        private const string DISPLAY_MAP_BORDER = "DisplayMapBorder";

        public IMetadataSettings MetadataSettings { get; set; }
        public IDeviceSettings DeviceSettings { get; set; }
        public IPolygonGraphicSettings PolygonGraphicSettings { get; set; }

        public string UserName =>
#if DEBUG
            "-";
#else
            Environment.UserName;
# endif

        public string DeviceName =>
#if DEBUG
            "-";
#else
            Environment.MachineName;
#endif

        public string AppVersion => AppInfo.Version.ToString();


        private string _Region;
        public string Region
        {
            get => _Region ?? (_Region = Properties.Settings.Default[REGION] as string);

            set
            {
                SetField(ref _Region, value);
                Properties.Settings.Default[REGION] = value;
                Properties.Settings.Default.Save();
            }
        }


        private string _District;
        public string District
        {
            get => _District ?? (_District = Properties.Settings.Default[DISTRICT] as string);

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
            get => _OpenFolderOnExport ?? (bool)(_OpenFolderOnExport = (bool?)Properties.Settings.Default[OPEN_FOLDER_ON_EXPORT]);

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
            get => _IsAdvancedMode;

            set
            {
                SetField(ref _IsAdvancedMode, value);
                Properties.Settings.Default[ADVANCED_MODE] = value;
                Properties.Settings.Default.Save();
            }
        }


        private bool _UseAdvancedProcessing;
        public bool UseAdvancedProcessing
        {
            get => _UseAdvancedProcessing;

            set
            {
                SetField(ref _UseAdvancedProcessing, value);
                Properties.Settings.Default[USE_ADVANCED_PROCESSING] = value;
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
            get => _UpgradeRequired;

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
            get => _SortPolysByName;

            set
            {
                SetField(ref _SortPolysByName, value);
                Properties.Settings.Default[SORT_POLYS_BY_NAME] = value;
                Properties.Settings.Default.Save();
            }
        }


        private Point _WindowStartupLocation;
        public Point WindowStartupLocation
        {
            get => _WindowStartupLocation;

            set
            {
                SetField(ref _WindowStartupLocation, value);
                Properties.Settings.Default[WINDOW_STARTUP_LOCATION] = value;
                Properties.Settings.Default.Save();
            }
        }
        public bool HasValidateWindowsStartupLocation => _WindowStartupLocation.X != 0 || _WindowStartupLocation.Y != 0;


        private bool _DisplayMapBorder;
        public bool DisplayMapBorder
        {
            get => _DisplayMapBorder;

            set
            {
                SetField(ref _DisplayMapBorder, value);
                Properties.Settings.Default[DISPLAY_MAP_BORDER] = value;
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

            _UseAdvancedProcessing = (bool)Properties.Settings.Default[USE_ADVANCED_PROCESSING];

            _SortPolysByName = (bool)Properties.Settings.Default[SORT_POLYS_BY_NAME];
            _WindowStartupLocation = (Point)Properties.Settings.Default[WINDOW_STARTUP_LOCATION];
            _DisplayMapBorder = (bool)Properties.Settings.Default[DISPLAY_MAP_BORDER];
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
            string version = $"PC: {programVersion.GetVersionWithBuildType()}";
            return new TtProjectInfo("Unamed Project", string.Empty, Region, string.Empty, District,
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


        public void AddRecentProject(string filePath)
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

        public void RemoveRecentProject(string filePath)
        {
            _RecentProjects.Remove(filePath);
            Properties.Settings.Default[RECENT_PROJECTS] = _RecentProjects;
            Properties.Settings.Default.Save();
        }
    }
}
