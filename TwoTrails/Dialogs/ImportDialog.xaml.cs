using System;
using System.Linq;
using System.Windows;
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

        public ImportDialog(ITtManager manager, string fileName = null)
        {
            _ImportModel = new ImportModel(this, manager, fileName);
            this.DataContext = _ImportModel;
            InitializeComponent();
        }

        public static bool? ShowDialog(TtProject project, Window owner = null, String fileName = null)
        {
            ImportDialog diag = new ImportDialog(project.HistoryManager, fileName);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }

        private void Grid_Drop(Object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                if (files.Length > 1 && files.All(f => f.EndsWith(Consts.SHAPE_EXT, StringComparison.InvariantCultureIgnoreCase)))
                    _ImportModel.SetupShapeFiles(files);
                else
                    _ImportModel.SetupImport(files.First());
            }
        }
    }
}
