using FMSC.Core.Windows.Controls;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;
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
            model = new PointMinimizationModel(project, this);
            this.DataContext = model;
            InitializeComponent();

            List<TtPoint> points = model.Points.OnBndPointsList();

            foreach (TtPoint p in points)
            {
                lbPoints.SelectedItems.Add(p);
            }
        }

        private async void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync((sender as TextBox).SelectAll);
        }


        private void TextIsUnsignedDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsUnsignedDouble(sender, e);
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
