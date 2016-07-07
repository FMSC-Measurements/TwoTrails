using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
