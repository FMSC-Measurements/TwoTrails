﻿using FMSC.Core.Windows.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for CreatePlotsDialog.xaml
    /// </summary>
    public partial class CreatePlotsDialog : Window
    {
        public CreatePlotsDialog(TtProject project)
        {
            this.DataContext = new CreatePlotsModel(project, this);
            InitializeComponent();
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

        private void TextIsDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsDouble(sender, e);
        }

        private void TextIsUnsignedDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsUnsignedDouble(sender, e);
        }


        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            CreatePlotsDialog dialog = new CreatePlotsDialog(project);

            if (owner != null)
                dialog.Owner = owner;

            return dialog.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action onClose = null)
        {
            CreatePlotsDialog dialog = new CreatePlotsDialog(project);

            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
            {
                dialog.Closed += (s, e) => onClose();
            }

            dialog.Show();
        }
    }
}
