using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Controls;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public class UserActivityTab : TtTabModel
    {
        public override bool IsDetachable { get; } = false;

        public override string TabTitle
        {
            get
            {
                return String.Format("{0} (Activity)",
                  Project.ProjectName);
            }
        }

        public UserActivityTab(TtProject project) : base(project)
        {
            this.Tab.Content = new UserActivityControl(project.DAL);
        }
    }
}
