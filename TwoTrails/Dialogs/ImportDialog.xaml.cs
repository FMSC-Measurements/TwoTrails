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
using System.Windows.Shapes;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportDialog.xaml
    /// </summary>
    public partial class ImportDialog : Window
    {
        public ImportDialog(ITtManager manager)
        {
            this.DataContext = new ImportModel(this, manager);
            InitializeComponent();
        }

        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            ImportDialog diag = new ImportDialog(project.Manager);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }
    }
}
