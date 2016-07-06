using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Commands;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {

        public override bool IsDetachable { get; } = false;


        public ProjectTab(MainWindowModel mainModel, TtProject project) : base(mainModel, project)
        {

        }


        protected override void CloseTab()
        {
            if (MainModel.CanCloseProject(Project))
            {
                MainModel.CloseProject(Project);
            }
        }

        protected override void SaveProject()
        {
            Project.Save();
        }
    }
}
