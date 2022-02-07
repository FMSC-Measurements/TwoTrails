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
        public ReindexDialog(TtHistoryManager manager)
        {
            this.DataContext = new ReindexModel(manager);
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(TtHistoryManager manager, Window owner = null, Action<bool?> onClose = null)
        {
            ReindexDialog dialog = new ReindexDialog(manager);
            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
                dialog.Closed += (s, e) => onClose(dialog.DialogResult);

            dialog.Show();
        }
    }
}
