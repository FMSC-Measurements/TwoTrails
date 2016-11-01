using System;
using System.Collections.Generic;
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

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public TtSettings Settings { get; }

        public SettingsWindow(TtSettings settings)
        {
            this.Settings = settings;
            this.DataContext = this;
            InitializeComponent();
        }

        public static bool? ShowDialog(TtSettings settings, Window owner = null)
        {
            SettingsWindow window = new SettingsWindow(settings);

            if (owner != null)
                window.Owner = owner;

            return window.ShowDialog();
        }
    }
}
