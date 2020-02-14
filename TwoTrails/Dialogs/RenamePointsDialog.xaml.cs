using FMSC.Core.Windows.Utilities;
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
    public partial class RenamePointsDialog : Window
    {
        public int StartIndex { get; set; } = Consts.DEFAULT_POINT_START_INDEX;
        public int Increment { get; set; } = Consts.DEFAULT_POINT_INCREMENT;

        public RenamePointsDialog(ITtManager manager)
        {
            InitializeComponent();
            this.DataContext = this;

            cbPolys.ItemsSource = manager.GetPolygons();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = string.IsNullOrEmpty(e.Text) ? false : !e.Text.All(char.IsDigit);
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsShownAsDialog())
                this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsShownAsDialog())
                this.DialogResult = false;
            this.Close();
        }

        private void cbPolys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPolys.SelectedItem is TtPolygon poly)
            {
                txtStartIndex.Text = poly.PointStartIndex.ToString();
                StartIndex = poly.PointStartIndex;

                txtIncrement.Text = poly.Increment.ToString();
                Increment = poly.Increment;
            }
        }
    }
}
