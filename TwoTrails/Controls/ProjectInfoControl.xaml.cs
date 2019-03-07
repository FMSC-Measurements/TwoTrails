using System.Windows.Controls;
using TwoTrails.Core;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for ProjectInfoControl.xaml
    /// </summary>
    public partial class ProjectInfoControl : UserControl
    {
        public ProjectInfoControl()
        {
            InitializeComponent();
        }

        public void SetProjectInfo(TtProjectInfo projectInfo)
        {
            txtName.DataContext = projectInfo;
            txtRegion.DataContext = projectInfo;
            txtDistrict.DataContext = projectInfo;
            txtForest.DataContext = projectInfo;
            txtDesc.DataContext = projectInfo;
        }

        public void FocusName()
        {
            txtName.Focus();
        }
    }
}
