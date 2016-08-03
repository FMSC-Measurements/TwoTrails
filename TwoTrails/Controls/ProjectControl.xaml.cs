using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for ProjectControl.xaml
    /// </summary>
    public partial class ProjectControl : UserControl
    {
        DataEditorModel DataEditor;
        DataStyleModel DataStyles;

        public ProjectControl(DataEditorModel dataEditor, DataStyleModel dataStyles)
        {
            DataEditor = dataEditor;
            DataStyles = dataStyles;

            InitializeComponent();

            this.DataContext = dataEditor;
        }

        private void TextBox_UpdateBinding(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null && tb.IsEnabled)
            {
                var expression = tb.GetBindingExpression(TextBox.TextProperty);

                if (expression != null)
                    expression.UpdateSource();
            }
        }
        

        private void DataGrid_OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AlterRow(e)));
        }

        private void AlterRow(DataGridRowEventArgs e)
        {
            e.Row.Style = DataStyles.GetRowStyle(e.Row.Item as TtPoint);
        }

        private void dgPoints_SelectedItemListChanged(System.Collections.IList items)
        {
            DataEditor.OnSelectionChanged();
        }
    }
}
