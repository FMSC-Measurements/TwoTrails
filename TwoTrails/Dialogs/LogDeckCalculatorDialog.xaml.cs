using System;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for LogDeckCalculatorDialog.xaml
    /// </summary>
    public partial class LogDeckCalculatorDialog : Window
    {
        public LogDeckCalculatorDialog(TtProject project)
        {
            this.DataContext = new LogDeckCalculatorModel(project);
            InitializeComponent();
        }

        private void TextIsUnsignedDouble(object sender, TextCompositionEventArgs e)
        {
            e.Handled = FMSC.Core.Windows.Controls.ControlUtils.TextIsUnsignedDouble(sender, e);
        }

        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            LogDeckCalculatorDialog dialog = new LogDeckCalculatorDialog(project);

            if (owner != null)
                dialog.Owner = owner;

            return dialog.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action onClose = null)
        {
            LogDeckCalculatorDialog dialog = new LogDeckCalculatorDialog(project);

            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
            {
                dialog.Closed += (s, e) => onClose();
            }

            dialog.Show();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
