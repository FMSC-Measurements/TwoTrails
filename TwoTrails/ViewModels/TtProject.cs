using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.ViewModels;

namespace TwoTrails.ViewModels
{
    public class TtProject : NotifyPropertyChangedEx
    {
        public String FilePath { get { return DAL.FilePath; } }
        
        public ICommand OpenMapCommand { get; }
        public ICommand OpenMapWindowCommand { get; }
        public ICommand ViewUserActivityCommand { get; set; }

        public ICommand EditProjectCommand { get; }
        public ICommand EditPointsCommand { get; }
        public ICommand EditPolygonsCommand { get; }
        public ICommand EditGroupsCommand { get; }
        public ICommand EditMetadataCommand { get; }

        public ICommand UndoCommand { get; set; }
        public ICommand RedoCommand { get; set; }

        public ICommand DiscardChangesCommand { get; }
        
        public ProjectTab ProjectTab { get; private set; }
        private MapTab MapTab { get; set; }
        private UserActivityTab UserActivityTab { get; set; }
        private MapWindow MapWindow { get; set; }
        
        public bool ProjectTabIsOpen { get { return ProjectTab != null; } }
        public bool MapTabIsOpen { get { return MapTab != null; } }
        public bool UserActivityTabIsOpen { get { return UserActivityTab != null; } }
        public bool MapWindowIsOpen { get { return MapWindow != null; } }
        

        private TtProjectInfo _ProjectInfo;
        public TtProjectInfo ProjectInfo { get; }
        
        public String ProjectName
        {
            get { return Get<String>(); }
            private set { Set(value); }
        }
        
        public bool RequiresSave
        {
            get { return Get<bool>() || DataChanged || ProjectChanged; }
            private set { Set(value); }
        }


        public bool RequiresUpgrade { get; private set; }


        public bool DataChanged
        {
            get { return Get<bool>(); }
            private set { Set(value, () => OnPropertyChanged(nameof(RequiresSave))); }
        }

        public bool ProjectChanged
        {
            get { return Get<bool>(); }
            private set { Set(value, () => OnPropertyChanged(nameof(RequiresSave))); }
        }


        public ITtDataLayer DAL { get; }

        public ITtSettings Settings { get; private set; }

        public TtManager Manager { get; }
        public TtHistoryManager HistoryManager { get; }

        private DataEditorModel _DataEditor;
        public DataEditorModel DataEditor
        {
            get { return _DataEditor ?? (_DataEditor = new DataEditorModel(this)); }
        }

        public MainWindowModel MainModel { get; private set; }


        public void ProjectUpdated()
        {
            DataChanged |= true;
        }


        public TtProject(ITtDataLayer dal, ITtSettings settings, MainWindowModel mainModel)
        {
            DAL = dal;
            Settings = settings;

            MainModel = mainModel;

            ProjectInfo = dal.GetProjectInfo();
            _ProjectInfo = new TtProjectInfo(ProjectInfo);

            ProjectInfo.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                ProjectChanged = !_ProjectInfo.Equals(ProjectInfo);
            };

            ProjectName = ProjectInfo.Name;

            RequiresSave = false;

            Manager = new TtManager(dal, settings);
            HistoryManager = new TtHistoryManager(Manager);
            HistoryManager.HistoryChanged += Manager_HistoryChanged;
            
            UndoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => HistoryManager.Undo(), x => HistoryManager.CanUndo, HistoryManager, x => x.CanUndo);

            RedoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => HistoryManager.Redo(), x => HistoryManager.CanRedo, HistoryManager, x => x.CanRedo);

            DiscardChangesCommand = new RelayCommand(x => Manager.Reset());

            ViewUserActivityCommand = new RelayCommand(x => ViewUserActivityTab());
            
            EditProjectCommand = new RelayCommand(x => OpenProjectTab());
            EditPointsCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Points));
            EditPolygonsCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Polygons));
            EditMetadataCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Metadata));
            EditGroupsCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Groups));

            OpenMapWindowCommand = new RelayCommand(x => OpenMapTab());

            ProjectTab = new ProjectTab(this);
        }

        private void Manager_HistoryChanged(object sender, EventArgs e)
        {
            Set(HistoryManager.CanUndo || DataChanged, nameof(RequiresSave));
        }


        public void Save()
        {
            if (RequiresSave)
            {
                try
                {
                    if (ProjectChanged)
                    {
                        DAL.UpdateProjectInfo(ProjectInfo);
                        _ProjectInfo = new TtProjectInfo(ProjectInfo);
                        ProjectName = ProjectInfo.Name;
                    }

                    Manager.Save();
                    RequiresSave = DataChanged = ProjectChanged = false;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message, "TtProject:Save");
                }
            }
        }

        public void Close()
        {
            if (MainModel.CloseProject(this))
            {
                if (ProjectTab != null)
                    MainModel.CloseTab(ProjectTab);

                if (MapTab != null)
                    MainModel.CloseTab(MapTab);
                
                if (UserActivityTab != null)
                    MainModel.CloseTab(UserActivityTab);

                MapWindow?.Close();
            }
        }

        public void CloseTab(TtTabModel tab)
        {
            if (tab is ProjectTab)
            {
                //warn closing project
                Close();
            }
            else
            {
                if (tab is MapTab)
                {
                    MainModel.CloseTab(tab);
                    MapTab = null;
                }
                else if (tab is UserActivityTab)
                {
                    MainModel.CloseTab(tab);
                    UserActivityTab = null;
                }
            }
        }

        public void CloseWindow(TtWindow window)
        {
            if (window is MapWindow)
            {

            }
        }

        private void OpenProjectTab(ProjectStartupTab tab = ProjectStartupTab.Project)
        {
            if (ProjectTab == null)
            {
                ProjectTab = new ProjectTab(this);
                MainModel.AddTab(ProjectTab);
            }
            else
            {
                MainModel.SwitchToTab(ProjectTab);
            }

            ProjectTab.SwitchToTab(tab);
        }

        private void OpenMapTab()
        {
            MapWindow m = new MapWindow(this);
            m.Show();
        }

        private void DetachMapTab()
        {

        }

        private void ViewUserActivityTab()
        {
            if (UserActivityTab == null)
            {
                UserActivityTab = new UserActivityTab(this);
                MainModel.AddTab(UserActivityTab);
            }
            else
            {
                MainModel.SwitchToTab(UserActivityTab);
            }
        }
    }
}
