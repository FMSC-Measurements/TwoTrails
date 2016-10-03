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
        public TtProject Project { get; set; }

        public MapWindow(TtProject project)
        {
            this.Project = project;
            this.DataContext = this;
            InitializeComponent();
        }
    }
}
