using FMSC.Core.Windows.Controls;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Mapping;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for MinimizPointsDialog.xaml
    /// </summary>
    public partial class PointMinimizationDialog : Window
    {
        private PointMinimizationModel model;

        public PointMinimizationDialog(TtProject project)
        {
            model = new PointMinimizationModel(project);
            this.DataContext = model;
            InitializeComponent();
        }

        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            model.AnalyzeTargetPolygon();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (model.Apply())
                this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private async void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }

        private void TextIsUnsignedInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsUnsignedInteger(sender, e);
        }

        private void TextIsInteger(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsInteger(sender, e);
        }


        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            PointMinimizationDialog diag = new PointMinimizationDialog(project);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action<bool?> onClose = null)
        {
            PointMinimizationDialog dialog = new PointMinimizationDialog(project);
            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
                dialog.Closed += (s, e) => onClose(dialog.DialogResult);

            dialog.Show();
        }
    }
}
