using System;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for EditValuesDialog.xaml
    /// </summary>
    public partial class ReindexDialog : Window
    {
        public ReindexDialog(TtProject project)
        {
            this.DataContext = new ReindexModel(project);
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(TtProject project, Window owner = null, Action<bool?> onClose = null)
        {
            ReindexDialog dialog = new ReindexDialog(project);
            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
                dialog.Closed += (s, e) => onClose(dialog.DialogResult);

            dialog.Show();
        }
    }
}
