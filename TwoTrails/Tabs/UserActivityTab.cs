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
                return $"{Project.ProjectName} (Activity)";
            }
        }

        public UserActivityTab(TtProject project) : base(project)
        {
            this.Tab.Content = new UserActivityControl(project.DAL);
        }
    }
}
