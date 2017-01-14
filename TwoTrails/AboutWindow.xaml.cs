using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        public string Copyright { get { return String.Format("USDA Forest Service {0}", DateTime.Now.Year); } }

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
