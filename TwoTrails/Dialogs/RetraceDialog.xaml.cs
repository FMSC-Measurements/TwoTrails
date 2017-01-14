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
    /// Interaction logic for RetraceDialog.xaml
    /// </summary>
    public partial class RetraceDialog : Window
    {
        RetraceModel model;

        public RetraceDialog(TtHistoryManager manager)
        {
            model = new RetraceModel(manager);
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


        public static bool? ShowDialog(TtHistoryManager manager, Window owner = null)
        {
            RetraceDialog diag = new RetraceDialog(manager);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }
    }
}
