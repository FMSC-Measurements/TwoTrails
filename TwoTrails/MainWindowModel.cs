using CSUtil.ComponentModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Commands;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Dialogs;

namespace TwoTrails
{
    public class MainWindowModel : NotifyPropertyChangedEx
    {
        private Dictionary<String, ProjectTab> _ProjectTabs = new Dictionary<String, ProjectTab>();
        private Dictionary<String, MapTab> _MapTabs = new Dictionary<String, MapTab>();
        private Dictionary<String, MapWindow> _MapWindows = new Dictionary<String, MapWindow>();

        private Dictionary<String, TtProject> _Projects = new Dictionary<String, TtProject>();

        
        public TtTabModel CurrentTab
        {
            get { return Get<TtTabModel>(); }
            set { Set(value, () =>
                    OnPropertyChanged(
                        nameof(CurrentProject),
                        nameof(HasOpenedProject),
                        nameof(CanSaveCurrentProject)
                        )
                    );
            }
        }

        public TtProject CurrentProject { get { return CurrentTab?.Project; } }

        public bool HasOpenedProject { get { return CurrentTab != null; } }


        public bool CanSaveCurrentProject
        {
            get { return CurrentProject != null && CurrentProject.RequiresSave; }
        }
        
        public bool HasUnsavedProjects
        {
            get { return _Projects.Values.Any(p => p.RequiresSave); }
        }


        public bool RecentItemsAvail
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }

        
        private bool exiting = false;
        public bool CanExit
        {
            get { return exiting || !HasUnsavedProjects || Exit(false); }
        }


        public TtSettings Settings { get; private set; }


        #region Commands
        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CloseProjectCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        #endregion


        private TabControl _Tabs;
        private MainWindow _MainWindow;




        public MainWindowModel(MainWindow mainWindow)
        {
            _MainWindow = mainWindow;

            Settings = new TtSettings(new DeviceSettings(), new MetadataSettings());

            NewCommand = new RelayCommand((x) => CreateProject());
            OpenCommand = new RelayCommand((x) => OpenProject());
            OpenProjectCommand = new RelayCommand((x) => OpenProject(x as string));
            SaveCommand = new RelayCommand((x) => SaveCurrentProject());
            CloseProjectCommand = new RelayCommand((x) => CloseProject(x as TtProject));
            ExitCommand = new RelayCommand((x) => Exit());

            UndoCommand = new RelayCommand((x) => CurrentProject.Manager.Undo());
            RedoCommand = new RelayCommand((x) => CurrentProject.Manager.Redo());

            _Tabs = mainWindow.tabControl;
            _Tabs.SelectionChanged += Tabs_SelectionChanged;

            UpdateRecentProjectMenu();

            CurrentTab = null;


            string[] args = Environment.GetCommandLineArgs();

            if (args != null && args.Length > 0)
            {
                ParseCommandLineArgs(args);
            }
        }


        private void ParseCommandLineArgs(String[] args)
        {
            foreach (String file in args.Select(f => f.Trim('"')))
            {
                if (file.EndsWith(".tt"))
                    OpenProject(file);
            }
        }



        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Tabs.SelectedIndex > -1)
                CurrentTab = (_Tabs.Items[_Tabs.SelectedIndex] as TabItem).DataContext as TtTabModel;
            else
                CurrentTab = null;
        }




        public void OpenProject()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.DefaultExt = ".tt";
            dialog.Filter = "TwoTrails Files (*.tt)|*.tt";
            
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                OpenProject(dialog.FileName);
            }
        }

        public void OpenProject(String filePath)
        {
            if (filePath != null && File.Exists(filePath))
            {
                if (!_Projects.Keys.Contains(filePath, StringComparer.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(filePath);

                        if (dal.RequiresUpgrade)
                        {
                            //ask to upgrade
                        }
                        else
                        {
                            AddProject(new TtProject(dal, Settings));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Project is already opened.");

                    TabItem tab = _ProjectTabs[filePath].Tab;
                    for (int i = 0; i < _Tabs.Items.Count; i++)
                    {
                        if (_Tabs.Items[i] == tab)
                        {
                            _Tabs.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("File not found");

                Settings.RemoveRecentProject(filePath);
                UpdateRecentProjectMenu();
            }
        }




        public void CreateProject()
        {
            NewProjectDialog dialog = new NewProjectDialog(_MainWindow, Settings.CreateProjectInfo(AppInfo.Version));

            if (dialog.ShowDialog() == true)
            {
                TtProjectInfo info = dialog.ProjectInfo;

                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(dialog.FilePath, info);
                TtProject proj = new TtProject(dal, Settings);

                AddProject(proj);

                Settings.Region = info.Region;
                Settings.District = info.District;
            }
        }



        public void SaveCurrentProject()
        {
            CurrentProject.Save();
        }



        private void AddProject(TtProject proj)
        {
            _Projects.Add(proj.FilePath, proj);

            ProjectTab tab = new ProjectTab(this, proj);
            _ProjectTabs.Add(proj.FilePath, tab);

            _Tabs.Items.Add(tab.Tab);
            _Tabs.SelectedIndex = _Tabs.Items.Count - 1;

            Settings.AddRecentProject(proj.FilePath);
            UpdateRecentProjectMenu();
        }

        
        public void OpenMapTab(TtProject project)
        {

        }

        public void OpenMapWindow(TtProject project)
        {

        }

        public void CloseProject(TtProject project)
        {
            if (project != null)
            {
                if (project.RequiresSave)
                {
                    MessageBoxResult result = MessageBox.Show("Would you like to save before closing this project?",
                                                 String.Empty,
                                                 MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            project.Save();
                        }
                        catch
                        {
                            //Error
                            return;
                        }
                    }
                    else if (result == MessageBoxResult.Cancel)
                        return;
                }

                CloseProjectTab(project);
            }
        }

        public void CloseProjectTab(TtProject project)
        {
            if (_ProjectTabs.ContainsKey(project.FilePath))
            {
                _Tabs.Items.Remove(_ProjectTabs[project.FilePath].Tab);
                _ProjectTabs.Remove(project.FilePath);
            }

            CloseMap(project);

            _Projects.Remove(project.FilePath);
        }

        public void CloseMap(TtProject project)
        {
            if (project != null)
            {
                if (_MapTabs.ContainsKey(project.FilePath))
                {

                    _Tabs.Items.Remove(_MapTabs[project.FilePath].Tab);
                    _MapTabs.Remove(project.FilePath);
                }

                if (_MapWindows.ContainsKey(project.FilePath))
                {
                    _MapWindows[project.FilePath].Close();
                    _MapWindows.Remove(project.FilePath);
                } 
            }
        }

        
        private bool Exit(bool closeWindow = true)
        {
            IEnumerable<TtProject> openProjects = _Projects.Values.Where(p => p.RequiresSave);

            if (openProjects.Any())
            {
                MessageBoxResult result = MessageBox.Show(
                        openProjects.Count() > 1 ?
                            "You have open projects. Would you like to save them before closing?" :
                            "You have an open project. Would you like to save it before closing?",
                        String.Empty,
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        foreach (TtProject proj in openProjects)
                        {
                            CloseProject(proj);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                    return false;
            }

            exiting = true;

            if (closeWindow)
                _MainWindow.Close();

            return true;
        }



        private void UpdateRecentProjectMenu()
        {
            MenuItem miRecent = _MainWindow.miRecent, item;

            miRecent.Items.Clear();

            foreach (String filePath in Settings.GetRecentProjects())
            {
                item = new MenuItem();

                item.Header = filePath;
                item.Command = OpenProjectCommand;
                item.CommandParameter = filePath;

                miRecent.Items.Add(item);
            }

            RecentItemsAvail = miRecent.Items.Count > 0;
        }
    }
}
