using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Commands;

namespace TwoTrails
{
    public class ProjectTab : TtTabItem
    {
        public override ICommand Save { get; }
        public override ICommand Close { get; }

        public override bool IsDetachable { get; } = false;


        public ProjectTab(MainWindowModel mainModel, TtProject project) : base(mainModel, project)
        {
            Save = new SaveProjectCommand();
            Close = new CloseProjectCommand(mainModel);
        }
    }
}
