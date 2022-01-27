using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TwoTrails.Core.Points;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for PointEditorControl.xaml
    /// </summary>
    public partial class PointEditorControl : UserControl
    {
        private DataStyleModel DataStyles;
        private PointEditorModel PointEditor;

        public PointEditorControl(PointEditorModel pointEditor, DataStyleModel dataStyles)
        {
            DataStyles = dataStyles;

            InitializeComponent();
            
            this.DataContext = PointEditor = pointEditor;

            dgPoints.PreviewMouseUp += DgPoints_PreviewMouseUp;
            dgPoints.LoadingRow += DgPoints_LoadingRow;
            dgPoints.BindableColumns = PointEditor.DataColumns;

            SortPolys();

            pointEditor.Project.Settings.PropertyChanged += (s, pce) =>
            {
                if (pce.PropertyName == nameof(TtSettings.SortPolysByName)) SortPolys();
            };
        }

        private void SortPolys()
        {
            lbPolys.Items.SortDescriptions.Clear();
            lbPolys.Items.SortDescriptions.Add(new SortDescription($"Item.{(PointEditor.Project.Settings.SortPolysByName ? "Name" : "TimeCreated")}", ListSortDirection.Ascending));
        }

        private void DgPoints_PreviewMouseUp(object sender, MouseButtonEventArgs e)
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
                    PointEditor.SelectedColumnIndex = columnHeader.DisplayIndex;
            }
            else if (dep is DataGridCell)
            {
                if (dep is DataGridCell cell)
                    PointEditor.SelectedColumnIndex = cell.Column.DisplayIndex;
            }
        }

        private void DgPoints_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => AlterRow(e)));
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

        private void AlterRow(DataGridRowEventArgs e)
        {
            e.Row.Style = DataStyles.GetRowStyle(e.Row.Item as TtPoint);
        }

        private void TextIsInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Windows.Controls.ControlUtils.TextIsInteger(sender, e);
        }

        private void TextIsUnsginedInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Windows.Controls.ControlUtils.TextIsUnsignedInteger(sender, e);
        }

        private void TextIsDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Windows.Controls.ControlUtils.TextIsDouble(sender, e);
        }

        private void TextIsUnsignedDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Windows.Controls.ControlUtils.TextIsUnsignedDouble(sender, e);
        }
    }
}
