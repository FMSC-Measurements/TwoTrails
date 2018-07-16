using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TwoTrails.Core.Points;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for DataEditorControl.xaml
    /// </summary>
    public partial class PointEditorControl : UserControl
    {
        private DataStyleModel DataStyles;
        private PointEditorModel DataEditor;

        public PointEditorControl(PointEditorModel dataEditor, DataStyleModel dataStyles)
        {
            DataStyles = dataStyles;

            InitializeComponent();
            
            this.DataContext = (DataEditor = dataEditor);
        }



        private void TextBox_UpdateBinding(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.IsEnabled)
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

        private void dgPoints_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            if (dep is DataGridColumnHeader)
            {
                if (dep is DataGridColumnHeader columnHeader)
                    DataEditor.SelectedColumnIndex = columnHeader.DisplayIndex;
            }
            else if (dep is DataGridCell)
            {
                if (dep is DataGridCell cell)
                    DataEditor.SelectedColumnIndex = cell.Column.DisplayIndex;
            }
        }


        private void TextIsInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Controls.ControlUtils.TextIsInteger(sender, e);
        }

        private void TextIsUnsginedInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Controls.ControlUtils.TextIsUnsignedInteger(sender, e);
        }

        private void TextIsDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Controls.ControlUtils.TextIsDouble(sender, e);
        }

        private void TextIsUnsignedDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Controls.ControlUtils.TextIsUnsignedDouble(sender, e);
        }
    }
}
