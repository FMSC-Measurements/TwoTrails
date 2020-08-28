using FMSC.Core.Windows.Controls;
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
        private ProjectEditorModel _ProjectEditor;

        public ProjectEditorControl(ProjectEditorModel projectEditor, ProjectTabSection tab = ProjectTabSection.Project)
        {
            _ProjectEditor = projectEditor;
            this.DataContext = _ProjectEditor;

            InitializeComponent();

            lbMetadata.SelectedIndex = 0;
            lbGroups.SelectedIndex = 0;

            if (lbPolys.Items.Count > 0)
                lbPolys.SelectedIndex = 0;

            SwitchToTab(tab);
        }

        public void SwitchToTab(ProjectTabSection tab)
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

        private void TextHasRestrictedCharacters(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextHasRestrictedCharacters(sender, e);
        }

        private void CommandInterceptor(object sender, KeyEventArgs e)
        {
            foreach (InputBinding inputBinding in this.InputBindings)
            {
                KeyGesture keyGesture = inputBinding.Gesture as KeyGesture;
                if (keyGesture != null && keyGesture.Key == e.Key && keyGesture.Modifiers == Keyboard.Modifiers)
                {
                    if (inputBinding.Command != null)
                    {
                        if (inputBinding.Command.CanExecute(0))
                        {
                            inputBinding.Command.Execute(0);
                        }
                        e.Handled = true;
                    }
                }
            }

            foreach (CommandBinding cb in this.CommandBindings)
            {
                RoutedCommand command = cb.Command as RoutedCommand;
                if (command != null)
                {
                    foreach (InputGesture inputGesture in command.InputGestures)
                    {
                        KeyGesture keyGesture = inputGesture as KeyGesture;
                        if (keyGesture != null && keyGesture.Key == e.Key && keyGesture.Modifiers == Keyboard.Modifiers)
                        {
                            command.Execute(0, this);
                            e.Handled = true;
                        }
                    }
                }
            }
        }
    }

    public enum ProjectTabSection
    {
        Project = 0,
        Points = 1,
        Polygons = 2,
        Metadata = 3,
        Groups = 4,
        Media = 5,
        Map = 6
    }
}
