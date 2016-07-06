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
            set { Set(value, () => OnPropertyChanged(nameof(CurrentProject), nameof(HasOpenedProject))); }
        }

        public TtProject CurrentProject { get { return CurrentTab?.Project; } }

        public bool HasOpenedProject { get { return CurrentTab != null; } }

        public bool RecentItemsAvail
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }



        public TtSettings Settings { get; private set; }



        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CloseProjectCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }


        private TabControl Tabs { get; }
        

        private MainWindow mainWindow;


        public MainWindowModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            Settings = new TtSettings(new DeviceSettings(), new MetadataSettings());

            NewCommand = new RelayCommand((x) => CreateProject());
            OpenCommand = new RelayCommand((x) => OpenProject());
            OpenProjectCommand = new RelayCommand((x) => OpenProject(x as string));
            SaveCommand = new RelayCommand((x) => SaveCurrentProject(), (x) => CanSaveCurrentProject());
            CloseProjectCommand = new RelayCommand((x) => CloseProject(x as TtProject), (x) => CanCloseProject(x as TtProject));
            ExitCommand = new RelayCommand((x) => Exit(), (x) => CanExit());

            Tabs = mainWindow.tabControl;
            Tabs.SelectionChanged += Tabs_SelectionChanged;

            UpdateRecentProjectMenu();

            CurrentTab = null;
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Tabs.SelectedIndex > -1)
                CurrentTab = (Tabs.Items[Tabs.SelectedIndex] as TabItem).DataContext as TtTabModel;
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

        private void OpenProject(String filePath)
        {
            if (filePath != null && File.Exists(filePath))
            {
                if (!_Projects.Keys.Contains(filePath, StringComparer.InvariantCultureIgnoreCase))
                {
                    try
                    {
                        TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(filePath);

                        TtProject project = new TtProject(dal);

                        AddProject(project);
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
                    for (int i = 0; i < Tabs.Items.Count; i++)
                    {
                        if (Tabs.Items[i] == tab)
                        {
                            Tabs.SelectedIndex = i;
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
            NewProjectDialog dialog = new NewProjectDialog(Settings.CreateProjectInfo(AppInfo.Version));

            if (dialog.ShowDialog() == true)
            {
                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(dialog.FilePath, dialog.ProjectInfo);
                TtProject proj = new TtProject(dal);

                AddProject(proj);
            }
        }



        public void SaveCurrentProject()
        {
            if (CurrentProject != null)
            {
                //save
            }
        }

        public bool CanSaveCurrentProject()
        {
            return CurrentProject != null && CurrentProject.RequiresSave;
        }



        private void AddProject(TtProject proj)
        {
            _Projects.Add(proj.FilePath, proj);

            ProjectTab tab = new ProjectTab(this, proj);
            _ProjectTabs.Add(proj.FilePath, tab);

            Tabs.Items.Add(tab.Tab);
            Tabs.SelectedIndex = Tabs.Items.Count - 1;

            Settings.AddRecentProject(proj.FilePath);
            UpdateRecentProjectMenu();
        }

        public void SaveSettings()
        {

        }

        
        public void OpenMapTab(TtProject project)
        {

        }

        public void OpenMapWindow(TtProject project)
        {

        }



        public bool CanCloseProject(TtProject project)
        {
            if (project != null)
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
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                    return false;
            }

            return true;
        }


        public void CloseProject(TtProject project)
        {
            if (project != null)
            {
                if (_ProjectTabs.ContainsKey(project.FilePath))
                {
                    Tabs.Items.Remove(_ProjectTabs[project.FilePath].Tab);
                    _ProjectTabs.Remove(project.FilePath);
                }

                CloseMap(project);

                _Projects.Remove(project.FilePath); 
            }
        }

        public void CloseMap(TtProject project)
        {
            if (project != null)
            {
                if (_MapTabs.ContainsKey(project.FilePath))
                {

                    Tabs.Items.Remove(_MapTabs[project.FilePath].Tab);
                    _MapTabs.Remove(project.FilePath);
                }

                if (_MapWindows.ContainsKey(project.FilePath))
                {
                    _MapWindows[project.FilePath].Close();
                    _MapWindows.Remove(project.FilePath);
                } 
            }
        }


        private void Exit()
        {
            mainWindow.Close(true);
        }

        public bool CanExit()
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
                            //save
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                    return false;
            }

            return true;
        }



        private void UpdateRecentProjectMenu()
        {
            MenuItem miRecent = mainWindow.miRecent, item;

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
