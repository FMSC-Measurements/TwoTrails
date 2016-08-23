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


        public static bool? ShowDialog(TtHistoryManager manager)
        {
            return new RetraceDialog(manager).ShowDialog();
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
    }
}
