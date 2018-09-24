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
    /// Interaction logic for EditValuesDialog.xaml
    /// </summary>
    public partial class EditValuesDialog : Window
    {
        public List<string> Values { get; private set; }
        private DataType _DataType;

        public EditValuesDialog(List<string> values, DataType dataType = DataType.TEXT)
        {
            InitializeComponent();
            _DataType = dataType;
            tbValues.Text = String.Join(Environment.NewLine, values);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Values = tbValues.Text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

            if (_DataType != DataType.TEXT)
            {
                foreach (string val in Values)
                {
                    if (_DataType == DataType.INTEGER)
                    {
                        if (!int.TryParse(val, out int i))
                        {
                            MessageBox.Show("One or more values are not an Integer.", "Invalid Values", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else if (_DataType == DataType.DECIMAL || _DataType == DataType.FLOAT)
                    {
                        if (!double.TryParse(val, out double d))
                        {
                            MessageBox.Show("One or more values are not a Number.", "Invalid Values", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
