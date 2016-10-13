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

        public MapWindow(TtProject project)
        {
            InitializeComponent();

            MapControl = new MapControl(project.Manager);
            cc.Content = MapControl;
        }

        public MapWindow(TtProject project, MapControl mapControl)
        {
            InitializeComponent();

            MapControl = mapControl;
            cc.Content = MapControl;
        }
    }
}
