using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails
{
    public class ProjectTab : TtTabModel
    {
        public ProjectTab(TtProject project) : base(project)
        {
        }

        public override bool IsDetachable { get; } = false;

        public override bool IsPointsEditable { get; } = false;

        //Use HistoryManager for data
    }
}
