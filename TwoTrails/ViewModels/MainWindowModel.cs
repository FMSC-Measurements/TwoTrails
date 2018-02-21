using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using FMSC.Core.Utilities;
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
        private const int MESSAGE_DELAY = 5000;

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
                        nameof(CurrentEditor)
                        );

                    MainWindow.Title = $"TwoTrails{(value != null ? $" - {value.Project.ProjectName}" : String.Empty)}";
                });
            }
        }


        public string StatusMessage { get { return Get<string>(); } private set { Set(value); } }
        private DelayActionHandler _EndMessageDelayHandler;
        private Action _EndMessage;

        public void PostMessage(string message, int delay = MESSAGE_DELAY)
        {
            StatusMessage = message;

            if (_EndMessageDelayHandler == null)
            {
                _EndMessage = () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = String.Empty;
                    });
                };

                _EndMessageDelayHandler = new DelayActionHandler(_EndMessage, MESSAGE_DELAY);
                _EndMessageDelayHandler.DelayInvoke();
            }
            else if (MESSAGE_DELAY != delay)
            {
                _EndMessageDelayHandler.DelayInvoke(_EndMessage, MESSAGE_DELAY);
            }
            else
            {
                _EndMessageDelayHandler.DelayInvoke();
            }
        }


        public TtProject CurrentProject { get { return CurrentTab?.Project; } }
        
        public PointEditorModel CurrentEditor { get; private set; }


        public bool HasOpenedProject { get { return CurrentTab != null; } }
        public bool HasPolygons { get { return CurrentEditor != null && CurrentEditor.Manager.Polygons.Count > 0; } }
        
        
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
        public ICommand SaveAsCommand { get; private set; }
        public ICommand CloseProjectCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public ICommand ImportCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand ViewPointDetailsCommand { get; private set; }
        public ICommand OpenInEarthCommand { get; private set; }

        public ICommand SettingsCommand { get; private set; }
        
        public ICommand ViewLogCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
        #endregion


        private TabControl _Tabs;
        public MainWindow MainWindow { get; }


        public MainWindowModel(MainWindow mainWindow)
        {
            StatusMessage = String.Empty;

            MainWindow = mainWindow;

            Settings = new TtSettings(new DeviceSettings(), new MetadataSettings(), new TtPolygonGraphicSettings());


            NewCommand = new RelayCommand(x => CreateProject());
            OpenCommand = new RelayCommand(x => BrowseProject());
            OpenProjectCommand = new RelayCommand(x => OpenProject(x as string));
            SaveCommand = new RelayCommand(x => SaveCurrentProject());
            SaveAsCommand = new RelayCommand(x => SaveCurrentProject(true));
            CloseProjectCommand = new RelayCommand(x => CurrentProject.Close());
            ExitCommand = new RelayCommand(x => Exit());

            ImportCommand = new RelayCommand(x => ImportData());
            ExportCommand = new RelayCommand(x => ExportProject());
            ViewPointDetailsCommand = new RelayCommand(x => ViewPointDetails());
            OpenInEarthCommand = new RelayCommand(x => OpenInGoogleEarth());

            SettingsCommand = new RelayCommand(x => OpenSettings());


            ViewLogCommand = new RelayCommand(x => Process.Start(App.LOG_FILE_PATH));
            AboutCommand = new RelayCommand(x => AboutWindow.ShowDialog(MainWindow));

            _Tabs = mainWindow.tabControl;
            _Tabs.SelectionChanged += Tabs_SelectionChanged;

            UpdateRecentProjectMenu();

            CurrentTab = null;


            string[] args = Environment.GetCommandLineArgs();

            if (args != null && args.Length > 1)
            {
                ParseCommandLineArgs(args);
            }
        }


        private void ParseCommandLineArgs(String[] args)
        {
            if (args[1].ToLower() == "--export-media" && args.Length > 2)
            {
                try
                {
                    string file = args[2];
                    if (file.EndsWith(Consts.FILE_EXTENSION_MEDIA) && File.Exists(file))
                        Export.MediaFiles(new TtSqliteMediaAccessLayer(file), Path.GetDirectoryName(file));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message, "MainWindowModel:ParseCommandLineArgs-ExportMedia");
                }

                Exit(true);
            }
            else
            {
                foreach (String file in args.Select(f => f.Trim('"')))
                {
                    if (file.EndsWith(Consts.FILE_EXTENSION))
                        OpenProject(file);
                }
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

            dialog.DefaultExt = Consts.FILE_EXTENSION;
            dialog.Filter = $"{Consts.FILE_EXTENSION_FILTER}|{Consts.FILE_EXTENSION_FILTER_V2}|All Files|*.*";
            
            if (dialog.ShowDialog() == true)
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
                    if (filePath.EndsWith(Consts.FILE_EXTENSION))
                    {
                        try
                        {
                            TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(filePath);
                            TtSqliteMediaAccessLayer mal = GetMalIfExists(filePath);

                            Trace.WriteLine($"DAL Opened ({dal.FilePath}): {dal.GetDataVersion()}");

                            if (mal != null)
                            {
                                Trace.WriteLine($"MAL Opened ({mal.FilePath}): {mal.GetDataVersion()}");
                            }

                            //todo upgrade
                            if (dal.RequiresUpgrade)
                            {
                                if (MessageBox.Show(MainWindow, @"This is file needs to be upgraded to work with this version of TwoTrails. Upgrading will first create a backup of this file. Would you like to upgrade it now?", "Upgrade TwoTrails file",
                                   MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
                                {
                                    if (Upgrade.DAL(dal))
                                    {
                                        AddProject(new TtProject(dal, mal, Settings, this));
                                        MessageBox.Show("Upgrade Successful");
                                    }
                                    else
                                    {
                                        MessageBox.Show("There has been an error and the Upgrade has failed. Please see log file for details");
                                    }
                                }
                            }
                            else
                            {
                                AddProject(new TtProject(dal, mal, Settings, this));
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"{ ex.Message }\n\t{ ex.StackTrace }", "MWM:OpenProject");
                            MessageBox.Show(MainWindow, "There is an issue opening this Project. See log file for details.", "Open Project Error");
                        }
                    }
                    else if (filePath.EndsWith(Consts.FILE_EXTENSION_V2))
                    {
                        TtV2SqliteDataAccessLayer dalv2 = new TtV2SqliteDataAccessLayer(filePath);

                        if (dalv2.RequiresUpgrade)
                        {
                            MessageBox.Show(MainWindow, @"This TwoTrails V2 file is too old to upgrade. 
Please Upgrade it to the most recent TT2 version before upgrading it here.", "Too old to upgrade",
                                MessageBoxButton.OK, MessageBoxImage.Stop);
                        }
                        else
                        {
                            if (MessageBox.Show(MainWindow, @"This is a TwoTrails V2 file that needs to be upgraded into the new TTX format. 
Upgrading will not delete this file. Would you like to upgrade it now?", "Upgrade TwoTrails file",
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

                                string upgradedFile = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}{Consts.FILE_EXTENSION}");

                                if (File.Exists(upgradedFile))
                                {
                                    if (MessageBox.Show(MainWindow, $"File '{Path.GetFileName(upgradedFile)}' already exists. Would you like to overwrite it?",
                                        "Overwrite File", MessageBoxButton.OKCancel, MessageBoxImage.Warning) != MessageBoxResult.OK)
                                    {
                                        return;
                                    }
                                }

                                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(upgradedFile, info);
                                TtSqliteMediaAccessLayer mal = GetMalIfExists(upgradedFile);

                                Upgrade.DAL(dal, Settings, dalv2);
                                    
                                AddProject(new TtProject(dal, mal, Settings, this));
                            }
                        }
                    }
                    else if (TtUtils.IsImportableFileType(filePath))
                    {
                        if (CurrentProject != null)
                        {
                            ImportDialog.ShowDialog(CurrentProject, MainWindow, filePath);
                        }
                        else
                        {
                            string fileType = TtUtils.GetFileTypeName(filePath);

                            if (MessageBox.Show($"Would you like to create a project from this {fileType} file?", "Create Project from importable file.",
                                MessageBoxButton.YesNo, MessageBoxImage.Hand, MessageBoxResult.No) == MessageBoxResult.Yes)
                            {
                                //todo create project from importable file

                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(MainWindow, $"File '{filePath}' is not a compatible project type.");
                    }
                }
                else
                {
                    MessageBox.Show(MainWindow, "Project is already opened.");

                    SwitchToTab(_Projects[filePath].ProjectTab);
                }
            }
            else
            {
                MessageBox.Show(MainWindow, "File not found");

                Settings.RemoveRecentProject(filePath);
                UpdateRecentProjectMenu();
            }
        }

        private TtSqliteMediaAccessLayer GetMalIfExists(string dalFilePath)
        {
            string malFilePath = Path.Combine(Path.GetDirectoryName(dalFilePath),
                                $"{Path.GetFileNameWithoutExtension(dalFilePath)}{Consts.FILE_EXTENSION_MEDIA}");

            if (File.Exists(malFilePath))
                return new TtSqliteMediaAccessLayer(malFilePath);

            return null;
        }


        public void CreateProject()
        {
            NewProjectDialog dialog = new NewProjectDialog(MainWindow, Settings.CreateProjectInfo(AppInfo.Version));

            if (dialog.ShowDialog() == true)
            {
                TtProjectInfo info = dialog.ProjectInfo;

                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(dialog.FilePath, info);
                TtProject proj = new TtProject(dal, null, Settings, this);
                Trace.WriteLine($"Project Created ({dal.GetDataVersion()}): {dal.FilePath}");

                AddProject(proj);

                Settings.Region = info.Region;
                Settings.District = info.District;
            }
        }



        public void SaveCurrentProject(bool rename = false)
        {
            if (rename)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.DefaultExt = Consts.FILE_EXTENSION;
                dialog.Filter = Consts.FILE_EXTENSION_FILTER;

                if (dialog.ShowDialog() == true)
                {
                    TtProject project = CurrentProject;
                    string nFile = dialog.FileName, oFile = project.DAL.FilePath;

                    File.Copy(oFile, nFile, true);

                    project.ReplaceDAL(new TtSqliteDataAccessLayer(nFile));

                    if (project.MAL != null)
                    {
                        string nmFile = dialog.FileName.Replace(Consts.FILE_EXTENSION, Consts.FILE_EXTENSION_MEDIA);

                        File.Copy(project.MAL.FilePath, nmFile, true);

                        project.ReplaceMAL(new TtSqliteMediaAccessLayer(nmFile));
                    }

                    Trace.WriteLine($"Project Copied: {nFile} from {oFile}");

                    _Projects.Remove(oFile);
                    _Projects.Add(nFile, project);

                    project.Save();
                }
            }
            else
            {
                CurrentProject.Save();
            }
        }



        private void AddProject(TtProject project)
        {
            _Tabs.Items.Add(project.ProjectTab.Tab);
            _Tabs.SelectedIndex = _Tabs.Items.Count - 1;

            _Projects.Add(project.FilePath, project);
            Settings.AddRecentProject(project.FilePath);
            UpdateRecentProjectMenu();

            project.MessagePosted += Project_MessagePosted;
        }

        public void RemoveProject(TtProject project)
        {
            if (project != null)
            {
                _Projects.Remove(project.FilePath);
                project.MessagePosted -= Project_MessagePosted;
            }
        }

        private void Project_MessagePosted(Object sender, String message)
        {
            PostMessage(message);
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


        public void CloseTab(TtTabModel tab)
        {
            if (tab != null)
            {
                _Tabs.Items.Remove(tab.Tab);
            }
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

        private void ViewPointDetails()
        {
            PointDetailsDialog.ShowDialog(CurrentProject.Manager.GetPoints(), MainWindow);
        }

        private void OpenInGoogleEarth()
        {
            Action createAndOpenKmz = () =>
            {
                try
                {
                    if (!Directory.Exists(App.TEMP_DIR))
                        Directory.CreateDirectory(App.TEMP_DIR);

                    string file = Path.Combine(App.TEMP_DIR, $"{Guid.NewGuid().ToString()}.kmz");
                    Export.KMZ(CurrentProject, file);

                    if (File.Exists(file))
                        Process.Start(file);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message, "MainWindowModel:OpenInGoogleEarth");
                    MessageBox.Show("An Error launching Google Earth has occured. Please see log file for details.");
                }
            };

            try
            {
                if (CoreUtils.IsApplictionInstalled("Google Earth") || CoreUtils.IsApplictionInstalled("Google Earth Pro"))
                {
                    createAndOpenKmz();
                }
                else
                {
                    if (MessageBox.Show("Google Earth is not installed. Would you like to install it now?", "Google Earth Not Found",
                        MessageBoxButton.OKCancel, MessageBoxImage.Hand) == MessageBoxResult.OK)
                    {
                        Process.Start("www.google.com/earth/");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "MainWindowModel:OpenInGoogleEarth");
                createAndOpenKmz();
            }
        }

        private void OpenSettings()
        {
            SettingsWindow.ShowDialog(Settings, MainWindow);
        }
    }
}
