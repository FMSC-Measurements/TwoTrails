using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using TwoTrails.DAL;

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public string Version { get { return AppInfo.Version.ToString(); } }

        public string DBVersion { get { return TwoTrailsSchema.SchemaVersion.ToString(); } }

        public string Copyright { get { return $"USDA Forest Service {DateTime.Now.Year}"; } }

        public ICommand CloseCommand { get; }


        public AboutWindow()
        {
            this.DataContext = this;

            CloseCommand = new RelayCommand((x) => this.Close());

            InitializeComponent();
        }

        public static bool? ShowDialog(Window window)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = window;
            return about.ShowDialog();
        }

        private void Hyperlink_RequestNavigate(Object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
