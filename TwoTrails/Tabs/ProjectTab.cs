using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Controls;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {
        private ProjectEditorControl _ProjectEditorControl;

        public override bool IsDetachable { get; } = false;

        public override bool IsPointsEditable { get; } = false;

        public override string TabTitle
        {
            get
            {
                return String.Format("{0}{1}",
                  Project.ProjectName,
                  Project.RequiresSave ? "*" : String.Empty);
            }
        }

        public ProjectTab(TtProject project) : base(project)
        {
            _ProjectEditorControl = new ProjectEditorControl(project, ProjectStartupTab.Points);
            Tab.Content = _ProjectEditorControl;
        }

        public void SwitchToTab(ProjectStartupTab tab)
        {
            _ProjectEditorControl.SwitchToTab(tab);
        }
    }
}
