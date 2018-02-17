﻿using Microsoft.Shell;
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

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
    public partial class App : Application, ISingleInstanceApp
    {
        public event EventHandler<IList<string>> ExternalInstanceArgs;

        private const string ID = "TwoTrailsApp";

        public const string LOG_FILE_NAME = "TwoTrails.log";
        public static readonly string LOG_FILE_PATH = Path.Combine(Directory.GetCurrentDirectory(), LOG_FILE_NAME);

        private TtTextWriterTraceListener _Listener;


        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(ID))
            {
                var application = new App();

                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _Listener = new TtTextWriterTraceListener(LOG_FILE_NAME);

            _Listener.WriteLine($"TwoTrails Started ({Assembly.GetExecutingAssembly().GetName().Version}|{TwoTrailsSchema.SchemaVersion}D)");

            Trace.Listeners.Add(_Listener);
#if DEBUG
            Debug.Listeners.Add(_Listener);
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
                if (Directory.Exists(Consts.TEMP_DIR))
                    Directory.Delete(Consts.TEMP_DIR, true);
            }
            catch { }

            base.OnExit(e);
        }



        public bool SignalExternalCommandLineArgs(IList<string> args)
        {


            return true;
        }
    }
}
