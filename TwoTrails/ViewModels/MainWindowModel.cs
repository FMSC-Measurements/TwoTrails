using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Dialogs;
using TwoTrails.Utils;
using TwoTrails.ViewModels;

namespace TwoTrails.ViewModels
{
    public class MainWindowModel : NotifyPropertyChangedEx
    {
        private Dictionary<String, TtProject> _Projects = new Dictionary<String, TtProject>();

        private Binding sbiInfoBinding;
        public TtTabModel CurrentTab
        {
            get { return Get<TtTabModel>(); }
            set {
                
                Set(value, () =>
                {
                    CurrentEditor = value?.Project.DataEditor;

                    if (sbiInfoBinding != null && value == null)
                    {
                        BindingOperations.ClearBinding(MainWindow.sbiInfo, TextBlock.TextProperty);
                        sbiInfoBinding = null;
                    }
                    else if (value != null)
                    {
                        sbiInfoBinding = new Binding();
                        sbiInfoBinding.Source = value;
                        sbiInfoBinding.Path = new PropertyPath(nameof(value.TabInfo));
                        sbiInfoBinding.Mode = BindingMode.OneWay;
                        sbiInfoBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(MainWindow.sbiInfo, TextBlock.TextProperty, sbiInfoBinding);
                    }

                    OnPropertyChanged(
                        nameof(CurrentProject),
                        nameof(HasOpenedProject),
                        nameof(CanSaveCurrentProject),
                        nameof(CurrentEditor)
                        );

                    MainWindow.Title = String.Format("{0}TwoTrails",
                        value != null ? String.Format("{0} - ", value.Project.ProjectName) : null);
                });
            }
        }

        public TtProject CurrentProject { get { return CurrentTab?.Project; } }
        
        public DataEditorModel CurrentEditor { get; private set; }


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
            get { return exiting || Exit(false); }
        }


        public TtSettings Settings { get; private set; }


        #region Commands
        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CloseProjectCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public ICommand ImportCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }

        public ICommand SettingsCommand { get; private set; }
        
        public ICommand ViewLogCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
        #endregion


        private TabControl _Tabs;
        public MainWindow MainWindow { get; }


        public MainWindowModel(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            Settings = new TtSettings(new DeviceSettings(), new MetadataSettings(), new TtPolygonGraphicSettings());


            NewCommand = new RelayCommand(x => CreateProject());
            OpenCommand = new RelayCommand(x => BrowseProject());
            OpenProjectCommand = new RelayCommand(x => OpenProject(x as string));
            SaveCommand = new RelayCommand(x => SaveCurrentProject());
            CloseProjectCommand = new RelayCommand(x => CurrentProject.Close());
            ExitCommand = new RelayCommand(x => Exit());

            ImportCommand = new RelayCommand(x => ImportData());
            ExportCommand = new RelayCommand(x => ExportProject());

            SettingsCommand = new RelayCommand(x => OpenSettings());


            ViewLogCommand = new RelayCommand(x => Process.Start(App.LOG_FILE_PATH));
            AboutCommand = new RelayCommand(x => AboutWindow.ShowDialog(MainWindow));

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




        public void BrowseProject()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.DefaultExt = ".tt";
            dialog.Filter = "TwoTrails Files (*.tt)|*.tt|TwoTrails V2 Files (*.tt2)|*.tt2";
            
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
                    if (filePath.EndsWith(".tt"))
                    {
                        try
                        {
                            TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(filePath);
                            Trace.WriteLine(String.Format("DAL Opened ({1}): {0}", dal.FilePath, dal.GetDataVersion()));

                            if (dal.RequiresUpgrade)
                            {
                                Trace.WriteLine("DAL Requires Upgrade");
                                //ask to upgrade
                            }
                            else
                            {
                                AddProject(new TtProject(dal, Settings, this));
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message, "MWM:OpenProject");
                        }
                    }
                    else
                    {
                        if (filePath.EndsWith(".tt2"))
                        {
                            TtV2SqliteDataAccessLayer dalv2 = new TtV2SqliteDataAccessLayer(filePath);

                            if (dalv2.RequiresUpgrade)
                            {
                                MessageBox.Show(@"This TwoTrails V2 file is too old to upgrade. 
Please Upgrade it with TwoTrails V2 before upgrading it here.", "Too old to upgrade",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);
                            }
                            else
                            {
                                if (MessageBox.Show(@"This is a TwoTrails V2 file that needs to be upgraded. 
Would you like to upgrade it now?", "Upgrade TwoTrails file",
                                    MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
                                {
                                    TtProjectInfo oinfo = dalv2.GetProjectInfo();
                                    TtProjectInfo info = new TtProjectInfo(
                                        oinfo.Name,
                                        oinfo.Description,
                                        oinfo.Region,
                                        oinfo.Forest,
                                        oinfo.District,
                                        AppInfo.Version.ToString(),
                                        oinfo.CreationVersion,
                                        TwoTrailsSchema.SchemaVersion,
                                        oinfo.CreationDeviceID,
                                        oinfo.CreationDate);

                                    string upgradedFile = Path.Combine(Path.GetDirectoryName(filePath),
                                        String.Format("{0}.tt", Path.GetFileNameWithoutExtension(filePath)));

                                    if (File.Exists(upgradedFile))
                                    {
                                        if (MessageBox.Show(String.Format("File '{0}' already exists. Would you like to overwrite it?", Path.GetFileName(upgradedFile)),
                                            "Overwrite File", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                                        {
                                            return;
                                        }
                                    }

                                    TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(upgradedFile, info);

                                    Upgrade.DAL(dal, Settings, dalv2);
                                    
                                    AddProject(new TtProject(dal, Settings, this));
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("File '{0}' is not a compatible project type.", filePath);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Project is already opened.");

                    SwitchToTab(_Projects[filePath].ProjectTab);
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
            NewProjectDialog dialog = new NewProjectDialog(MainWindow, Settings.CreateProjectInfo(AppInfo.Version));

            if (dialog.ShowDialog() == true)
            {
                TtProjectInfo info = dialog.ProjectInfo;

                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(dialog.FilePath, info);
                TtProject proj = new TtProject(dal, Settings, this);
                Trace.WriteLine(String.Format("Project Created ({1}): {0}", dal.FilePath, dal.GetDataVersion()));

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
            _Tabs.Items.Add(proj.ProjectTab.Tab);
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
        
        public void SwitchToTab(TtTabModel tab)
        {
            _Tabs.SelectedItem = tab.Tab;
            tab.Tab.Focus();
        }

        public void RemoveProject(TtProject project)
        {
            if (project != null)
                _Projects.Remove(project.FilePath);
        }


        public void CloseTab(TtTabModel tab)
        {
            if (tab != null)
                _Tabs.Items.Remove(tab.Tab);
        }



        private bool Exit(bool closeWindow = true)
        {
            IEnumerable<TtProject> unsavedProjects = _Projects.Values.Where(p => p.RequiresSave);

            if (unsavedProjects.Any())
            {
                MessageBoxResult result = MessageBox.Show(
                        unsavedProjects.Count() > 1 ?
                            "You have open projects. Would you like to save them before closing?" :
                            "You have an open project. Would you like to save it before closing?",
                        String.Empty,
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        foreach (TtProject proj in unsavedProjects.ToList())
                        {
                            proj.Save();
                            proj.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message, "MWM:Exit");
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                    return false;
            }
            else
            {
                foreach (TtProject proj in _Projects.Values.ToList())
                {
                    proj.Close();
                }
            }

            exiting = true;

            if (closeWindow)
                MainWindow.Close();

            return true;
        }



        private void UpdateRecentProjectMenu()
        {
            MenuItem miRecent = MainWindow.miRecent, item;

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


        private void ImportData()
        {
            ImportDialog.ShowDialog(CurrentProject, MainWindow);
        }

        private void ExportProject()
        {
            ExportDialog.ShowDialog(CurrentProject, MainWindow);
        }

        private void OpenSettings()
        {
            SettingsWindow.ShowDialog(Settings, MainWindow);
        }
    }
}
