using CSUtil.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Dialogs;

namespace TwoTrails.ViewModels
{
    public class TtProject : NotifyPropertyChangedEx
    {
        public event EventHandler<string> MessagePosted;

        #region Commands
        public ICommand OpenMapCommand { get; }
        public ICommand OpenMapWindowCommand { get; }
        public ICommand ViewUserActivityCommand { get; set; }

        public ICommand EditProjectCommand { get; }
        public ICommand EditPointsCommand { get; }
        public ICommand EditPolygonsCommand { get; }
        public ICommand EditGroupsCommand { get; }
        public ICommand EditMetadataCommand { get; }

        public ICommand RecalculateAllPolygonsCommand { get; }
        public ICommand CalculateLogDeckCommand { get; }

        public ICommand UndoCommand { get; set; }
        public ICommand RedoCommand { get; set; }

        public ICommand DiscardChangesCommand { get; }
        #endregion

        #region Properties
        public String FilePath { get { return DAL.FilePath; } }

        public ProjectTab ProjectTab { get; private set; }
        private UserActivityTab UserActivityTab { get; set; }
        private MapTab MapTab { get; set; }
        private MapWindow MapWindow { get; set; }
        
        //TODO remove mapwindow from TtProject
        public bool ProjectTabIsOpen { get { return ProjectTab != null; } }
        public bool MapTabIsOpen { get { return MapTab != null; } }
        public bool UserActivityTabIsOpen { get { return UserActivityTab != null; } }
        public bool MapWindowIsOpen { get { return MapWindow != null; } }
        

        private TtProjectInfo _ProjectInfo;
        public TtProjectInfo ProjectInfo { get; }
        
        public String ProjectName
        {
            get { return _ProjectInfo.Name; }
        }
        
        public bool RequiresSave
        {
            get { return Get<bool>() || DataChanged || ProjectChanged; }
            private set { Set(value); }
        }


        public bool RequiresUpgrade { get; private set; }

        //property changes to polys/meta/groups
        public bool DataChanged
        {
            get { return Get<bool>(); }
            private set { Set(value, () => OnPropertyChanged(nameof(RequiresSave))); }
        }

        //changes to project fields
        public bool ProjectChanged
        {
            get { return Get<bool>(); }
            private set { Set(value, () => OnPropertyChanged(nameof(RequiresSave))); }
        }


        public ITtDataLayer DAL { get; private set; }
        public ITtMediaLayer MAL { get; private set; }

        public TtSettings Settings { get; private set; }

        private TtManager _BaseManager { get; }
        public TtHistoryManager Manager { get; }

        private PointEditorModel _DataEditor;
        public PointEditorModel DataEditor
        {
            get { return _DataEditor ?? (_DataEditor = new PointEditorModel(this)); }
        }

        public MainWindowModel MainModel { get; private set; }
        #endregion


        public TtProject(ITtDataLayer dal, ITtMediaLayer mal, TtSettings settings, MainWindowModel mainModel)
        {
            DAL = dal;
            MAL = mal;
            Settings = settings;

            MainModel = mainModel;

            ProjectInfo = dal.GetProjectInfo();
            _ProjectInfo = new TtProjectInfo(ProjectInfo);

            ProjectInfo.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                ProjectChanged = !_ProjectInfo.Equals(ProjectInfo);
            };

            RequiresSave = false;

            _BaseManager = new TtManager(dal, mal, settings);
            Manager = new TtHistoryManager(_BaseManager);
            Manager.HistoryChanged += Manager_HistoryChanged;
            
            UndoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Manager.Undo(), x => Manager.CanUndo, Manager, x => x.CanUndo);

            RedoCommand = new BindedRelayCommand<TtHistoryManager>(
                x => Manager.Redo(), x => Manager.CanRedo, Manager, x => x.CanRedo);

            DiscardChangesCommand = new RelayCommand(x => _BaseManager.Reset());

            ViewUserActivityCommand = new RelayCommand(x => ViewUserActivityTab());
            
            EditProjectCommand = new RelayCommand(x => OpenProjectTab());
            EditPointsCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Points));
            EditPolygonsCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Polygons));
            EditMetadataCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Metadata));
            EditGroupsCommand = new RelayCommand(x => OpenProjectTab(ProjectStartupTab.Groups));
            
            RecalculateAllPolygonsCommand = new RelayCommand(x => { Manager.RecalculatePolygons(); DataChanged |= true; });
            CalculateLogDeckCommand = new RelayCommand(x =>
            {
                if (_BaseManager.PolygonCount > 0)
                {
                    MainModel.MainWindow.IsEnabled = false;
                    LogDeckCalculatorDialog.Show(this, this.MainModel.MainWindow, () =>
                    {
                        MainModel.MainWindow.IsEnabled = true;
                    });
                }
                else
                    MessageBox.Show("No Polygons in Project.", String.Empty, MessageBoxButton.OK, MessageBoxImage.Stop);
            });

            OpenMapCommand = new RelayCommand(x => OpenMapTab());
            OpenMapWindowCommand = new RelayCommand(x => OpenMapWindow());

            ProjectTab = new ProjectTab(this);
        }

        private void Manager_HistoryChanged(object sender, EventArgs e)
        {
            RequiresSave = Manager.CanUndo;
        }


        public void ProjectUpdated()
        {
            DataChanged |= true;
        }

        public void Save()
        {
            if (RequiresSave)
            {
                try
                {
                    if (ProjectChanged)
                    {
                        ProjectInfo.Name = ProjectInfo.Name.Trim();
                        DAL.UpdateProjectInfo(ProjectInfo);
                        _ProjectInfo = new TtProjectInfo(ProjectInfo);
                    }

                    _BaseManager.Save();
                    Manager.ClearHistory();
                    RequiresSave = DataChanged = ProjectChanged = false;
                    MessagePosted?.Invoke(this, $"Project '{ProjectName}' Saved");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message, "TtProject:Save");
                    MessagePosted?.Invoke(this, "Error Saving Project");
                    MessageBox.Show("Error Saving Project. See Log for details.", "Project Save Error");
                }
            }
        }

        public void ReplaceDAL(ITtDataLayer dal)
        {
            DAL = dal;
            _BaseManager.ReplaceDAL(DAL);
            OnPropertyChanged(nameof(DAL), nameof(FilePath));
        }

        public void ReplaceMAL(ITtMediaLayer mal)
        {
            MAL = mal;
            _BaseManager.ReplaceMAL(MAL);
            OnPropertyChanged(nameof(MAL));
        }

        public void Close()
        {
            if (RequiresSave)
            {
                MessageBoxResult result = MessageBox.Show("Would you like to save before closing this project?",
                                             String.Empty,
                                             MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Save();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            CloseViews();
            MainModel.RemoveProject(this);
        }

        private void CloseViews()
        {
            if (ProjectTab != null)
                MainModel.CloseTab(ProjectTab);

            if (MapTab != null)
                MainModel.CloseTab(MapTab);

            if (UserActivityTab != null)
                MainModel.CloseTab(UserActivityTab);

            MapWindow?.Close();
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
            if (MapTab == null)
            {
                if (MapWindow != null)
                {
                    MapTab = new MapTab(this, MapWindow);
                    MapWindow.Close();
                    MapWindow = null;
                }
                else
                {
                    MapTab = new MapTab(this);
                }

                MainModel.AddTab(MapTab);
            }

            MainModel.SwitchToTab(MapTab);
        }

        private void OpenMapWindow()
        {
            if (MapWindow == null)
            {

                if (MapTab != null)
                {
                    MapWindow = new MapWindow(_ProjectInfo.Name, MapTab.MapControl);
                    CloseTab(MapTab);
                }
                else
                {
                    MapWindow = new MapWindow(this);
                }
            }
            
            MapWindow.Show();
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
