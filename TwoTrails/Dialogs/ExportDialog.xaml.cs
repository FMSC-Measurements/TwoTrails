using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
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
