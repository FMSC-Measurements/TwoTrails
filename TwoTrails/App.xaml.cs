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
                
                _Listener.WriteLine(ex.Message, "[UnhandledException]");
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



        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            ExternalInstanceArgs?.Invoke(this, args?.Skip(1));

            return true;
        }
    }
}
