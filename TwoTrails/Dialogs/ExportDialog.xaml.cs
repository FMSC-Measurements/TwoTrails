using System.Windows;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        public ExportDialog(TtProject project, MainWindowModel mainWindowModel)
        {
            this.DataContext = new ExportProjectModel(this, project, mainWindowModel);

            InitializeComponent();
        }

        public static bool? ShowDialog(TtProject project, MainWindowModel mainWindowModel, Window owner = null)
        {
            ExportDialog dialog = new ExportDialog(project, mainWindowModel);
            if (owner != null)
                dialog.Owner = owner;
            return dialog.ShowDialog();
        }
    }
}
