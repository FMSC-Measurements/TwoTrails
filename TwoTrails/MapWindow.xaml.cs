using System.ComponentModel;
using System.Windows;
using TwoTrails.Controls;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        public MapControl MapControl { get; private set; }


        public MapWindow(string projectName, MapControl mapControl)
        {
            InitializeComponent();

            Title = $"Map - {projectName}";
            cc.Content = MapControl = mapControl;
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            cc.Content = null;
            base.OnClosing(e);
        }
    }
}
