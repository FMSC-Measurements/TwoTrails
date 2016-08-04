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

        public override bool IsPointsEditable { get; } = true;


        public override string TabTitle
        {
            get { return String.Format("(Map){0}", base.TabTitle); }
        }

        public MapTab(TtProject project) : base(project)
        {

        }
    }
}
