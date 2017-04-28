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
        private ImportModel _ImportModel;

        public ImportDialog(ITtManager manager)
        {
            _ImportModel = new ImportModel(this, manager);
            this.DataContext = _ImportModel;
            InitializeComponent();
        }

        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            ImportDialog diag = new ImportDialog(project.HistoryManager);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }

        private void Grid_Drop(Object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                _ImportModel.SetupImport(files.First());
            }
        }
    }
}
