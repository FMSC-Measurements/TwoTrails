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
    /// Interaction logic for CreatePlotsDialog.xaml
    /// </summary>
    public partial class CreatePlotsDialog : Window
    {
        public CreatePlotsDialog(ITtManager manager)
        {
            this.DataContext = new CreatePlotsModel(manager, this);
            InitializeComponent();
        }

        private void TextIsInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = string.IsNullOrEmpty(e.Text) ? false : !e.Text.All(char.IsDigit);
        }


        public static bool? ShowDialog(ITtManager manager)
        {
            CreatePlotsDialog dialog = new CreatePlotsDialog(manager);
            return dialog.ShowDialog();
        }
    }
}
