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
        public TtMapControl MapControl { get; private set; }


        public MapWindow(TtProject project) :
            this($"Map - {project.ProjectName}", new TtMapControl(project.Manager))
        { }

        public MapWindow(string projectName, TtMapControl mapControl)
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
