using System.Windows;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        public ExportDialog(TtProject project)
        {
            this.DataContext = new ExportProjectModel(this, project);

            InitializeComponent();
        }

        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            ExportDialog dialog = new ExportDialog(project);
            if (owner != null)
                dialog.Owner = owner;
            return dialog.ShowDialog();
        }
    }
}
