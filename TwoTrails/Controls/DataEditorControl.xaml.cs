using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using TwoTrails.Core.Points;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for DataEditorControl.xaml
    /// </summary>
    public partial class DataEditorControl : UserControl
    {
        private DataEditorModel DataEditor;
        private DataStyleModel DataStyles;

        public DataEditorControl(DataEditorModel dataEditor, DataStyleModel dataStyles)
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

        private void dgPoints_SelectedItemListChanged(IList items)
        {
            //DataEditor.OnSelectionChanged();
        }
    }
}
