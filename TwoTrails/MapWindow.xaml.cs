using TwoTrails.Controls;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : TtWindow
    {
        MapControl MapControl { get; }

        public MapWindow(TtProject project)
        {
            MapControl = new MapControl(project.Manager);
            InitializeComponent();
        }
    }
}
