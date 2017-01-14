using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class MapTab : TtTabModel
    {
        public override bool IsDetachable { get; } = true;

        public override ICommand OpenInWinndowCommand { get; }

        public MapControl MapControl { get; set; }

        public override string TabTitle
        {
            get { return String.Format("{0} (Map)", base.TabTitle); }
        }

        public MapTab(TtProject project) : base(project)
        {
            MapControl = new MapControl(project.Manager);
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
