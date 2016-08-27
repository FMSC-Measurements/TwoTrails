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

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for RenamePointsDialog.xaml
    /// </summary>
    public partial class RenamePointsDialog : Window
    {
        public int StartIndex { get; set; } = 1010;
        public int Increment { get; set; } = 10;

        public RenamePointsDialog(ITtManager manager)
        {
            InitializeComponent();
            this.DataContext = this;

            cbPolys.ItemsSource = manager.GetPolyons();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = string.IsNullOrEmpty(e.Text) ? false : !e.Text.All(char.IsDigit);
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void cbPolys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TtPolygon poly = cbPolys.SelectedItem as TtPolygon;

            if (poly != null)
            {
                txtStartIndex.Text = poly.PointStartIndex.ToString();
                StartIndex = poly.PointStartIndex;

                txtIncrement.Text = poly.Increment.ToString();
                Increment = poly.Increment;
            }
        }
    }
}
