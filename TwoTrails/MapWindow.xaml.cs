using System.ComponentModel;
using TwoTrails.Controls;
using TwoTrails.Mapping;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : TtWindow
    {
        public MapControl MapControl { get; private set; }


        public MapWindow(TtProject project) :
            this($"Map - {project.ProjectName}", new MapControl(project.Manager))
        { }

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
