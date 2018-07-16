using FMSC.Core.Controls;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    //TODO dispaly project version and other info in project tab

    /// <summary>
    /// Interaction logic for ProjectEditorControl.xaml
    /// </summary>
    public partial class ProjectEditorControl : UserControl
    {
        public ProjectEditorControl(ProjectEditorModel projectEditor, ProjectStartupTab tab = ProjectStartupTab.Project)
        {
            this.DataContext = projectEditor;

            InitializeComponent();

            SwitchToTab(tab);
        }

        public void SwitchToTab(ProjectStartupTab tab)
        {
            tabControl.SelectedIndex = (int)tab;
        }

        private void TextIsUnsignedInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsUnsignedInteger(sender, e);
        }

        private void TextIsDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsDouble(sender, e);
        }
    }

    public enum ProjectStartupTab
    {
        Project = 0,
        Points = 1,
        Polygons = 2,
        Metadata = 3,
        Groups = 4,
        Media = 5
    }
}
