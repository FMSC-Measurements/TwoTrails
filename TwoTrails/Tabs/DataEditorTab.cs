using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Controls;

namespace TwoTrails
{
    public class DataEditorTab : TtTabModel
    {
        public override bool IsDetachable { get; } = false;

        public override bool IsPointsEditable { get; } = true;

        public DataStyleModel DataStyles { get; private set; }


        public DataEditorTab(TtProject project) : base(project)
        {
            DataStyles = new DataStyleModel(Project);

            Tab.Content = new ProjectControl(Project.DataEditor, DataStyles);
        }
    }
}
