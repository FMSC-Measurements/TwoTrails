using FMSC.Core.Windows.Controls;
using System;
using System.Linq;
using System.Windows;
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
            CreatePlotsDialog dialog = new CreatePlotsDialog(project);

            if (owner != null)
                dialog.Owner = owner;
            else
                dialog.Owner = project.MainModel.MainWindow;

            return dialog.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action onClose = null)
        {
            CreatePlotsDialog dialog = new CreatePlotsDialog(project);

            if (owner != null)
                dialog.Owner = owner;
            else
                dialog.Owner = project.MainModel.MainWindow;

            if (onClose != null)
            {
                dialog.Closed += (s, e) => onClose();
            }

            dialog.Show();
        }
    }
}
