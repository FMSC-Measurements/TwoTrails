using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Utils;

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
    public partial class App : Application, ISingleInstanceApp
    {
        public event EventHandler<IEnumerable<string>> ExternalInstanceArgs;

        private const string ID = "TwoTrailsApp";

        private static String _TwoTrailsAppDataDir = null;
        public static String TwoTrailsAppDataDir { get; } = _TwoTrailsAppDataDir ?? (_TwoTrailsAppDataDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TwoTrails"));

        private static String _TEMP_DIR = null;
        public static String TEMP_DIR { get; } = _TEMP_DIR ?? (_TEMP_DIR = Path.Combine(Path.GetTempPath(), "TwoTrails\\Temp\\"));

        public const string LOG_FILE_NAME = "TwoTrails.log";
        public static string _LOG_FILE_PATH = null;
        public static string LOG_FILE_PATH { get; } = _LOG_FILE_PATH ?? (_LOG_FILE_PATH = Path.Combine(TwoTrailsAppDataDir, LOG_FILE_NAME));

        private TtTextWriterTraceListener _Listener;


        public static TtSettings Settings { get; private set; } 


        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(ID))
            {
                Settings = new TtSettings(new DeviceSettings(), new MetadataSettings(), new TtPolygonGraphicSettings());

                string[] cmdArgs = Environment.GetCommandLineArgs();
                if (cmdArgs.Length > 2 && cmdArgs[1].Equals("/export", StringComparison.InvariantCultureIgnoreCase))
                {
                    ExportFile(cmdArgs[2]);
                }
                else
                {
                    //Check for update
                    if (Settings.LastUpdateCheck == null || Settings.LastUpdateCheck < DateTime.Now.Subtract(TimeSpan.FromDays(1)))
                    {
                        bool? res = TtUtils.CheckForUpdate();

                        if (res != null)
                        {
                            Settings.LastUpdateCheck = DateTime.Now;

                            if (res == true && MessageBox.Show("A new version of TwoTrails is ready for download. Would you like to download it now?", "TwoTrails Update",
                                    MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                Process.Start(Consts.URL_TWOTRAILS);
                                return;
                            }
                        }
                    }

                    var application = new App();

                    application.InitializeComponent();
                    application.Run();

                    SingleInstance<App>.Cleanup();
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            if (!Directory.Exists(TwoTrailsAppDataDir))
                Directory.CreateDirectory(TwoTrailsAppDataDir);

            _Listener = new TtTextWriterTraceListener(LOG_FILE_PATH);

            _Listener.WriteLine($"TwoTrails Started ({Assembly.GetExecutingAssembly().GetName().Version}|{TwoTrailsSchema.SchemaVersion}D)");

#if DEBUG
            Debug.Listeners.Add(_Listener);
#else
            Trace.Listeners.Add(_Listener);
#endif

            AppDomain.CurrentDomain.UnhandledException += (s, ue) =>
            {
                Exception ex = ue.ExceptionObject as Exception;
                
                _Listener.WriteLine($"{ex.Message}\n\t{ex.StackTrace}", "[UnhandledException]");
                _Listener.Flush();
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _Listener.WriteLine("TwoTrails Exit");
            _Listener.Flush();

            try
            {
                if (Directory.Exists(TEMP_DIR))
                    Directory.Delete(TEMP_DIR, true);
            }
            catch { }

            base.OnExit(e);
        }


        private static void ExportFile(string file)
        {
            if (File.Exists(file))
            {
                using (StreamWriter logWriter = new StreamWriter(LOG_FILE_PATH, true))
                {
                    try
                    {
                        switch (Path.GetExtension(file))
                        {
                            case Consts.FILE_EXTENSION:
                                {
                                    Export.All(file);
                                    logWriter.WriteLine($"[{DateTime.Now}] Exported Project: {file}");
                                    MessageBox.Show("Project Exported");
                                    break;
                                }
                            case Consts.FILE_EXTENSION_FILTER_MEDIA:
                                {
                                    TtSqliteMediaAccessLayer mal = new TtSqliteMediaAccessLayer(file);
                                    Export.MediaFiles(mal, Path.Combine(Path.GetDirectoryName(mal.FilePath), Path.GetFileNameWithoutExtension(mal.FilePath)).Trim());
                                    logWriter.WriteLine($"[{DateTime.Now}] Exported Media: {file}");
                                    MessageBox.Show("Media Exported");
                                    break;
                                }
                            default:
                                MessageBox.Show("Invalid export file type");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logWriter.WriteLine($"[{DateTime.Now}] [Export Error]: {ex.Message}");
                        MessageBox.Show("An error has occured. Please view the log file for details");
                    }
                }
            }
            else
            {
                MessageBox.Show($"File '{file}' not found.");
            }
        }


        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            ExternalInstanceArgs?.Invoke(this, args?.Skip(1));

            return true;
        }
    }
}
