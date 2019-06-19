using CSUtil.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using FMSC.Core.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Dialogs;
using TwoTrails.Utils;
using FMSC.Core.Windows.Utilities;

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
                    
                    if (value != null)
                    {
                        sbiInfoBinding = new Binding();
                        sbiInfoBinding.Source = value;
                        sbiInfoBinding.Path = new PropertyPath(nameof(value.TabInfo));
                        sbiInfoBinding.Mode = BindingMode.OneWay;
                        sbiInfoBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                        BindingOperations.SetBinding(MainWindow.sbiInfo, TextBlock.TextProperty, sbiInfoBinding);
                    }
                    else if (sbiInfoBinding != null)
                    {
                        BindingOperations.ClearBinding(MainWindow.sbiInfo, TextBlock.TextProperty);
                        sbiInfoBinding = null;
                    }

                    OnPropertyChanged(nameof(CurrentProject), nameof(HasOpenedProject), nameof(CurrentEditor));

                    MainWindow.Title = $"TwoTrails{(value != null ? $" - {value.Project.ProjectName}" : String.Empty)}";
                });
            }
        }


        public string StatusMessage { get { return Get<string>(); } private set { Set(value); } }
        private DelayActionHandler _EndMessageDelayHandler;

        public void PostMessage(string message, int delay = MESSAGE_DELAY)
        {
            StatusMessage = message;

            if (_EndMessageDelayHandler == null)
            {
                _EndMessageDelayHandler = new DelayActionHandler(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = String.Empty;
                    });
                }, MESSAGE_DELAY);
            }

             _EndMessageDelayHandler.DelayInvoke(delay);
        }


        public TtProject CurrentProject { get { return CurrentTab?.Project; } }
        
        public PointEditorModel CurrentEditor { get; private set; }

        public TtSettings Settings { get; }

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
        public ICommand ExportReportCommand { get; private set; }
        public ICommand CheckForUpdatesCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
        #endregion


        private TabControl _Tabs;
        public MainWindow MainWindow { get; }


        public MainWindowModel(MainWindow mainWindow)
        {
            Settings = App.Settings;

            StatusMessage = String.Empty;

            MainWindow = mainWindow;

            NewCommand = new RelayCommand(x => CreateProject());
            OpenCommand = new RelayCommand(x => BrowseProject());
            OpenProjectCommand = new RelayCommand(x => OpenProject(x as string));
            SaveCommand = new RelayCommand(x => SaveCurrentProject());
            SaveAsCommand = new RelayCommand(x => SaveCurrentProject(true));
            CloseProjectCommand = new RelayCommand(x => CurrentProject?.Close());
            ExitCommand = new RelayCommand(x => Exit());

            ImportCommand = new RelayCommand(x => ImportDialog.ShowDialog(CurrentProject, MainWindow));
            ExportCommand = new RelayCommand(x => ExportDialog.ShowDialog(CurrentProject, MainWindow));
            ViewPointDetailsCommand = new RelayCommand(x => PointDetailsDialog.ShowDialog(CurrentProject.Manager.GetPoints(), MainWindow));
            OpenInEarthCommand = new RelayCommand(x => OpenInGoogleEarth());

            SettingsCommand = new RelayCommand(x => SettingsWindow.ShowDialog(App.Settings, MainWindow));


            ViewLogCommand = new RelayCommand(x => Process.Start(App.LOG_FILE_PATH));
            ExportReportCommand = new RelayCommand(x => ExportReport());
            CheckForUpdatesCommand = new RelayCommand(x => CheckForUpdates());
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

#if DEBUG
            if (CurrentProject == null)
            {
                foreach (String filePath in App.Settings.GetRecentProjects())
                {
                    if (File.Exists(filePath))
                    {
                        OpenProject(filePath);
                        break;
                    }
                }
            }
#endif
        }


        private void ParseCommandLineArgs(String[] args)
        {
            foreach (String file in args.Skip(1).Select(f => f.Trim('"')))
            {
                if (file.EndsWith(Consts.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase) ||
                    file.EndsWith(Consts.FILE_EXTENSION_MEDIA, StringComparison.InvariantCultureIgnoreCase))
                    OpenProject(file);
            }
        }
        

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentTab = (_Tabs.SelectedIndex > -1) ?  (_Tabs.Items[_Tabs.SelectedIndex] as TabItem).DataContext as TtTabModel : null;
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
                if (filePath.EndsWith(Consts.FILE_EXTENSION, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_Projects.Keys.Contains(filePath, StringComparer.InvariantCultureIgnoreCase))
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

                            if (!TtUtils.CheckAndFixErrors(dal))
                                return;

                            if (dal.RequiresUpgrade)
                            {
                                if (MessageBox.Show(MainWindow, @"This is file needs to be upgraded to work with this version of TwoTrails. Upgrading will first create a backup of this file. Would you like to upgrade it now?", "Upgrade TwoTrails file",
                                   MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
                                {
                                    string oldFilePath = $"{dal.FilePath}.old";
                                    if (File.Exists(oldFilePath) &&
                                        MessageBox.Show($"There is already a filed named '{Path.GetFileName(oldFilePath)}'. Would you like to overwrite this file?",
                                        "File.Old Already Exists", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                                    {
                                        return;
                                    }

                                    if (Upgrade.DAL(dal, Settings))
                                    {
                                        AddProject(new TtProject(dal, mal, App.Settings, this));
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
                                AddProject(new TtProject(dal, mal, App.Settings, this));
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"{ ex.Message }\n\t{ ex.StackTrace }", "MWM:OpenProject");
                            MessageBox.Show(MainWindow, "There is an issue opening this Project. See log file for details.", "Open Project Error");
                        }
                    }
                    else
                    {
                        MessageBox.Show(MainWindow, "Project is already opened.");

                        SwitchToTab(_Projects[filePath].ProjectTab);
                    }
                }
                else if (filePath.EndsWith(Consts.FILE_EXTENSION_MEDIA, StringComparison.InvariantCultureIgnoreCase))
                {
                    string ttFile = filePath.Replace(Consts.FILE_EXTENSION_MEDIA, Consts.FILE_EXTENSION);
                    if (!_Projects.Keys.Contains(ttFile, StringComparer.InvariantCultureIgnoreCase))
                        OpenProject(ttFile);
                }
                else if (filePath.EndsWith(Consts.FILE_EXTENSION_V2, StringComparison.InvariantCultureIgnoreCase) && CurrentProject == null)
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

                            Upgrade.DAL(dal, App.Settings, dalv2);

                            AddProject(new TtProject(dal, mal, App.Settings, this));
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
                        if (MessageBox.Show($"Would you like to create a project from this {TtUtils.GetFileTypeName(filePath)} file?", "Create Project from importable file.",
                            MessageBoxButton.YesNo, MessageBoxImage.Hand, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            CreateAndOpenProjectFromImportable(null, filePath);
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
                MessageBox.Show(MainWindow, "File not found");

                App.Settings.RemoveRecentProject(filePath);
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


        public bool CreateProject(TtProjectInfo ttProjectInfo = null)
        {
            NewProjectDialog dialog = new NewProjectDialog(MainWindow, ttProjectInfo ?? App.Settings.CreateProjectInfo(AppInfo.Version));

            if (dialog.ShowDialog() == true)
            {
                TtProjectInfo info = dialog.ProjectInfo;

                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(dialog.FilePath, info);
                TtProject proj = new TtProject(dal, null, App.Settings, this);
                Trace.WriteLine($"Project Created ({dal.GetDataVersion()}): {dal.FilePath}");

                AddProject(proj);

                App.Settings.Region = info.Region;
                App.Settings.District = info.District;

                return true;
            }

            return false;
        }

        public bool CreateAndOpenProjectFromImportable(TtProjectInfo ttProjectInfo, params string[] files)
        {
            if (ttProjectInfo == null)
            {
                ttProjectInfo = App.Settings.CreateProjectInfo(AppInfo.Version);
                ttProjectInfo.Name = $"Import of {String.Join(", ", files.Select(f => Path.GetFileNameWithoutExtension(f)))}";
            }

            NewProjectDialog dialog = new NewProjectDialog(MainWindow, ttProjectInfo, Path.GetDirectoryName(files.First()));

            if (dialog.ShowDialog() == true)
            {
                TtProjectInfo info = dialog.ProjectInfo;

                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(dialog.FilePath, info);
                TtProject proj = new TtProject(dal, null, App.Settings, this);

                Trace.WriteLine($"Import Project Created ({dal.GetDataVersion()}): {dal.FilePath}");
                
                App.Settings.Region = info.Region;
                App.Settings.District = info.District;

                IReadOnlyTtDataLayer idal = null;

                try
                {
                    foreach (string file in files)
                    {
                        switch (Path.GetExtension(file).ToLower())
                        {
                            case Consts.FILE_EXTENSION: idal = new TtSqliteDataAccessLayer(file); break;
                            case Consts.FILE_EXTENSION_V2: idal = new TtV2SqliteDataAccessLayer(file); break;
                            case Consts.GPX_EXT:
                                idal = new TtGpxDataAccessLayer(
                                    new TtGpxDataAccessLayer.ParseOptions(file, proj.Manager.DefaultMetadata.Zone, startPolyNumber: proj.Manager.PolygonCount + 1)); break;
                            case Consts.KML_EXT:
                            case Consts.KMZ_EXT:
                                idal = new TtKmlDataAccessLayer(
                                    new TtKmlDataAccessLayer.ParseOptions(file, proj.Manager.DefaultMetadata.Zone, startPolyNumber: proj.Manager.PolygonCount + 1)); break;
                            case Consts.SHAPE_EXT:
                                idal = new TtShapeFileDataAccessLayer(
                                    new TtShapeFileDataAccessLayer.ParseOptions(file, proj.Manager.DefaultMetadata.Zone, proj.Manager.PolygonCount + 1)); break;
                            case Consts.TEXT_EXT:
                            case Consts.CSV_EXT: ImportDialog.ShowDialog(proj, this.MainWindow, file, true); break;
                            default: idal = null; break;
                        }

                        if (idal != null)
                        {
                            Import.DAL(proj.Manager, idal);
                            proj.Save();
                        }
                    }

                    AddProject(proj);

                    return true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Import Project failed to import data.");
                    Trace.WriteLine(ex.Message, "MainWindowModel:CreateProjectFromImportable");

                    File.Delete(dialog.FilePath);

                    MessageBox.Show("Error Converting File to TwoTrails Project. See Error Log for Details.");
                }
            }
            
            return false;
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

                    TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(nFile);

                    if (!TtUtils.CheckAndFixErrors(dal))
                    {
                        File.Delete(nFile);
                        PostMessage("Save Canceled");
                    }
                    else
                    {
                        project.ReplaceDAL(dal);

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
            }
            else
            {
                CurrentProject.Save();
            }

            MainWindow.Title = $"TwoTrails - {CurrentProject.ProjectName}";
        }

        
        private void AddProject(TtProject project)
        {
            _Tabs.Items.Add(project.ProjectTab.Tab);
            _Tabs.SelectedIndex = _Tabs.Items.Count - 1;

            _Projects.Add(project.FilePath, project);
            App.Settings.AddRecentProject(project.FilePath);
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
                tab.Close();
                _Tabs.Items.Remove(tab.Tab);
            }
        }
        

        private bool Exit(bool closeWindow = true)
        {
            try
            {
                foreach (TtProject proj in _Projects.Values.ToList())
                {
                    proj.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "MWM:Exit");
                MessageBox.Show(ex.Message);
                return false;
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

            foreach (String filePath in App.Settings.GetRecentProjects())
            {
                item = new MenuItem();

                item.Header = filePath;
                item.Command = OpenProjectCommand;
                item.CommandParameter = filePath;

                miRecent.Items.Add(item);
            }

            RecentItemsAvail = miRecent.Items.Count > 0;
        }
        
        private void OpenInGoogleEarth()
        {
            try
            {
                if (CoreUtils.IsExtensionOpenable("kmz"))
                {
                    try
                    {
                        if (!Directory.Exists(App.TEMP_DIR))
                            Directory.CreateDirectory(App.TEMP_DIR);

                        string file = Path.Combine(App.TEMP_DIR, $"{Guid.NewGuid().ToString()}.kmz");
                        Export.KMZ(CurrentProject.Manager, CurrentProject.ProjectInfo, file);

                        if (File.Exists(file))
                            Process.Start(file);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message, "MainWindowModel:OpenInGoogleEarth");
                        MessageBox.Show("An Error launching Google Earth has occured. Please see log file for details.");
                    }
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
                MessageBox.Show("Unable to open in Google Earth. See Log file for details.");
            }
        }

        private void ExportReport()
        {
            string name = $"ExportReport_{DateTime.Now.ToString().Replace(':', '.').Replace('/','-')}";
            
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.OverwritePrompt = true;
            sfd.DefaultExt = "zip";
            sfd.Filter =  "Zip files (*.zip)|*.zip|All files (*.*)|*.*";
            sfd.FileName = $"{name}.zip";
            sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (sfd.ShowDialog() == true)
            {
                string tempDirectory = Path.Combine(App.TEMP_DIR, name);
                Directory.CreateDirectory(tempDirectory);

                File.Copy(App.LOG_FILE_PATH, Path.Combine(tempDirectory, App.LOG_FILE_NAME));
                foreach (TtProject proj in _Projects.Values)
                {
                    File.Copy(proj.DAL.FilePath, Path.Combine(tempDirectory, Path.GetFileName(proj.DAL.FilePath)));
                    if (proj.MAL != null)
                        File.Copy(proj.MAL.FilePath, Path.Combine(tempDirectory, Path.GetFileName(proj.MAL.FilePath)));
                }

                using (StreamWriter sw = new StreamWriter(Path.Combine(tempDirectory, "App.Settings.txt")))
                {
                    sw.WriteLine("--Main App.Settings--");
                    sw.WriteLine($"{nameof(ITtSettings.UserName)}: {App.Settings.UserName}");
                    sw.WriteLine($"{nameof(ITtSettings.DeviceName)}: {App.Settings.DeviceName}");
                    sw.WriteLine($"{nameof(ITtSettings.Region)}: {App.Settings.Region}");
                    sw.WriteLine($"{nameof(ITtSettings.District)}: {App.Settings.District}");
                    sw.WriteLine($"{nameof(TtSettings.IsAdvancedMode)}: {App.Settings.IsAdvancedMode}");
                    sw.WriteLine("Recent Projects:");
                    foreach (string rp in App.Settings.GetRecentProjects())
                        sw.WriteLine(rp);
                    sw.WriteLine();

                    sw.WriteLine("--Device App.Settings--");
                    sw.WriteLine($"{nameof(App.Settings.DeviceSettings.DeleteExistingPlots)}: {App.Settings.DeviceSettings.DeleteExistingPlots}");
                    sw.WriteLine();

                    sw.WriteLine("--Metadata App.Settings--");
                    sw.WriteLine($"{nameof(MetadataSettings.Datum)}: {App.Settings.MetadataSettings.Datum}");
                    sw.WriteLine($"{nameof(MetadataSettings.DecType)}: {App.Settings.MetadataSettings.DecType}");
                    sw.WriteLine($"{nameof(MetadataSettings.Distance)}: {App.Settings.MetadataSettings.Distance}");
                    sw.WriteLine($"{nameof(MetadataSettings.Elevation)}: {App.Settings.MetadataSettings.Elevation}");
                    sw.WriteLine($"{nameof(MetadataSettings.MagDec)}: {App.Settings.MetadataSettings.MagDec}");
                    sw.WriteLine($"{nameof(MetadataSettings.Slope)}: {App.Settings.MetadataSettings.Slope}");
                    sw.WriteLine($"{nameof(MetadataSettings.Zone)}: {App.Settings.MetadataSettings.Zone}");
                }

                ZipFile.CreateFromDirectory(tempDirectory, Path.GetFullPath(sfd.FileName));

                if (Directory.Exists(tempDirectory))
                    Directory.Delete(tempDirectory, true);
            }
        }
    
        private void CheckForUpdates()
        {
            UpdateStatus status = TtUtils.CheckForUpdate();

            if (status.CheckStatus != null)
            {
                Settings.LastUpdateCheck = DateTime.Now;

                if (status.CheckStatus == true)
                {
                    if (MessageBox.Show($@"A new version of TwoTrails is ready for download.{
                        (status.UpdateType.HasFlag(UpdateType.CriticalBugFixes) ? " There are CRITICAL updates implemented that should be installed. " : String.Empty)
                    }Would you like to download it now?", "TwoTrails Update",
                        MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    {
                        Process.Start(Consts.URL_TWOTRAILS);
                    }
                }
                else if (status.CheckStatus == false)
                {
                    MessageBox.Show("You have the most recent version of TwoTrails", "TwoTrails Update");
                }
            }
            else
            {
                MessageBox.Show("Checking for updates was unsuccessful.", "TwoTrails Update");
            }
        }
    }
}
