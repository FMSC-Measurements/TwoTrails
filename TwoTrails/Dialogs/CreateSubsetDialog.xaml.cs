using FMSC.Core.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for RenamePointsDialog.xaml
    /// </summary>
    public partial class CreateSubsetDialog : Window
    {
        public int StartIndex { get; set; } = 1010;
        public int Increment { get; set; } = 10;

        public int SubsetValue { get; set; }



        public CreateSubsetDialog(ITtManager manager)
        {
            InitializeComponent();
            this.DataContext = this;

            cbPolys.ItemsSource = manager.GetPolygons();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsUnsignedInteger(sender, e);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
