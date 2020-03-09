using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class MapTab : TtTabModel
    {
        public override bool IsDetachable { get; } = true;

        public override ICommand OpenInWinndowCommand { get; }

        public TtMapControl MapControl { get; set; }

        public override string TabTitle
        {
            get { return $"{base.TabTitle} (Map)"; }
        }

        public MapTab(TtProject project) : base(project)
        {
            MapControl = new TtMapControl(project.Manager);
            Tab.Content = MapControl;

            OpenInWinndowCommand = Project.OpenMapWindowCommand;
        }

        public MapTab(TtProject project, MapWindow mapWindow) : base(project)
        {
            MapControl = mapWindow.MapControl;
            Tab.Content = MapControl;

            OpenInWinndowCommand = Project.OpenMapWindowCommand;
        }
    }
}
