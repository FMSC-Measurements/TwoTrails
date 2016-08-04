using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class UserActivityTab : TtTabModel
    {
        public override bool IsDetachable { get; } = false;

        public override bool IsPointsEditable { get; } = false;

        public UserActivityTab(TtProject project) : base(project)
        {
        }
    }
}
