using System;
using System.Windows;
using System.Windows.Controls;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for RetraceDialog.xaml
    /// </summary>
    public partial class RetraceDialog : Window
    {
        RetraceModel model;

        public RetraceDialog(TtProject project)
        {
            model = new RetraceModel(project);
            this.DataContext = model;
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            model.AddRetrace(((sender as Button).DataContext as Retrace));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            model.DeleteRetrace(((sender as Button).DataContext as Retrace));
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (model.RetracePoints())
                this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            RetraceDialog diag = new RetraceDialog(project);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action<bool?> onClose = null)
        {
            RetraceDialog dialog = new RetraceDialog(project);
            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
                dialog.Closed += (s, e) => onClose(dialog.DialogResult);

            dialog.Show();
        }
    }
}
