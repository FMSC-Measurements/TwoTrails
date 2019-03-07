using System.Windows;

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

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
