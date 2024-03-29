﻿using System;
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

        public ImportDialog(TtProject project, MainWindowModel mainWindowModel, string fileName = null, bool autoCloseOnImport = false)
        {
            _ImportModel = new ImportModel(project, mainWindowModel, this, fileName, autoCloseOnImport);
            this.DataContext = _ImportModel;
            InitializeComponent();
        }

        public static bool? ShowDialog(TtProject project, MainWindowModel mainWindowModel, Window owner = null, String fileName = null, bool autoCloseOnImport = false)
        {
            ImportDialog diag = new ImportDialog(project, mainWindowModel, fileName, autoCloseOnImport);
            if (owner != null)
                diag.Owner = owner;
            else if (mainWindowModel != null)
                diag.Owner = mainWindowModel.MainWindow;
            return diag.ShowDialog();
        }

        private void Grid_Drop(Object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files != null && files.Length > 0)
            {
                if (files.All(f => f.EndsWith(Consts.SHAPE_EXT, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _ImportModel.SetupShapeFiles(files);
                }
                else
                {
                    if (files.Count() > 1)
                        MessageBox.Show("Only Shape Files can be imported in bulk.", "Import Files", MessageBoxButton.OK, MessageBoxImage.Stop);

                    _ImportModel.SetupImport(files.First());
                }
            }
        }
    }
}
