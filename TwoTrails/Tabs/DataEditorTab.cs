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
    public class DataEditorTab : TtTabModel
    {
        public override bool IsDetachable { get; } = false;

        public override bool IsPointsEditable { get; } = true;

        public override string ToolTip { get { return Project.FilePath; } }

        public DataStyleModel DataStyles { get; private set; }


        public DataEditorTab(TtProject project) : base(project)
        {
            DataStyles = new DataStyleModel(Project);

            Tab.Content = new DataEditorControl(Project.DataEditor, DataStyles);
        }
    }
}
