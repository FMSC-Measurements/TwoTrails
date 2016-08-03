using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
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
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Dialogs;

namespace TwoTrails
{
    public class MainWindowModel : NotifyPropertyChangedEx
    {
        private Dictionary<String, TtProject> _Projects = new Dictionary<String, TtProject>();
        
        public TtTabModel CurrentTab
        {
            get { return Get<TtTabModel>(); }
            set {
                Set(value, () =>
                {
                    CurrentEditor = value?.Project.DataEditor;

                    EditorVisible = value is DataEditorTab;

                    OnPropertyChanged(
                        nameof(CurrentProject),
                        nameof(HasOpenedProject),
                        nameof(CanSaveCurrentProject),
                        nameof(CurrentEditor)
                        );

                    _MainWindow.Title = String.Format("{0}TwoTrails",
                        value != null ? String.Format("{0} - ", value.Project.ProjectName) : null);
                });
            }
        }

        public TtProject CurrentProject { get { return CurrentTab?.Project; } }
        
        public DataEditorModel CurrentEditor { get; private set; }

        public bool EditorVisible { get; private set; }


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

        public ICommand EditProjectCommand { get; private set; }
        public ICommand EditPolygonsCommand { get; private set; }
        public ICommand EditGroupsCommand { get; private set; }
        public ICommand EditMetadataCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }

        public ICommand SettingsCommand { get; private set; }
        
        public ICommand ViewLogCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
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
            CloseProjectCommand = new RelayCommand((x) => CurrentProject.Close());
            ExitCommand = new RelayCommand((x) => Exit());

            EditProjectCommand = new RelayCommand((x) => EditProject());
            EditPolygonsCommand = new RelayCommand((x) => EditProject());
            EditMetadataCommand = new RelayCommand((x) => EditProject());
            EditGroupsCommand = new RelayCommand((x) => EditProject());

            ImportCommand = new RelayCommand((x) => ImportData());
            ExportCommand = new RelayCommand((x) => ExportProject());

            SettingsCommand = new RelayCommand((x) => EditSettings());

            ViewLogCommand = new RelayCommand((x) => MessageBox.Show("log file"));
            AboutCommand = new RelayCommand((x) => new AboutWindow().ShowDialog());

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
                            AddProject(new TtProject(dal, Settings, this));
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
                    
                    for (int i = 0; i < _Tabs.Items.Count; i++)
                    {
                        if (String.Equals((_Tabs.Items[i] as DataEditorTab)?.Project.FilePath, filePath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _Tabs.SelectedIndex = i;
                            break;
                        }
                        else
                        {
                            AddTab(_Projects[filePath].DataEditorTab);
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
                TtProject proj = new TtProject(dal, Settings, this);

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
            _Tabs.Items.Add(proj.DataEditorTab.Tab);
            _Tabs.SelectedIndex = _Tabs.Items.Count - 1;

            _Projects.Add(proj.FilePath, proj);
            Settings.AddRecentProject(proj.FilePath);
            UpdateRecentProjectMenu();
        }

        public void AddTab(TtTabModel tabModel)
        {
            _Tabs.Items.Add(tabModel.Tab);
            _Tabs.SelectedIndex = _Tabs.Items.Count - 1;
        }
        

        public bool CloseProject(TtProject project)
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
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return false;
                        }
                    }
                    else if (result == MessageBoxResult.Cancel)
                        return false;
                }

                _Projects.Remove(project.FilePath);

                return true;
            }

            return false;
        }


        public void CloseTab(TtTabModel tab)
        {
            if (tab != null)
                _Tabs.Items.Remove(tab.Tab);
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



        private void EditProject()
        {

        }

        private void ImportData()
        {

        }

        private void ExportProject()
        {

        }

        private void EditSettings()
        {

        }
    }
}
