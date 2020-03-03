using System;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for EditValuesDialog.xaml
    /// </summary>
    public partial class HistoryDialog : Window
    {
        public HistoryDialog(TtHistoryManager manager)
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Point point = PointToScreen(Mouse.GetPosition(this));

            Left = point.X;
            Top = point.Y;
        }

        //protected override void OnContentRendered(EventArgs e)
        //{
        //    base.OnContentRendered(e);

        //    var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
        //    var mouse = transform.Transform(Mouse.GetPosition());
        //    Left = mouse.X - ActualWidth;
        //    Top = mouse.Y - ActualHeight;
        //    Activate();
        //}

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Close();
        }

        public static void ShowDialog(TtHistoryManager manager, Window owner = null, Action<bool?> onClose = null)
        {
            HistoryDialog dialog = new HistoryDialog(manager);

            //var transform = PresentationSource.FromVisual(owner).CompositionTarget.TransformFromDevice;
            //var mouse = transform.Transform(Mouse.GetPosition(null));
            //dialog.Left = mouse.X - dialog.ActualWidth;
            //dialog.Top = mouse.Y - dialog.ActualHeight;

            //Point point = owner.PointToScreen(Mouse.GetPosition(null));

            //dialog.Left = point.X;
            //dialog.Top = point.Y;

            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
                dialog.Closed += (s, e) => onClose(dialog.DialogResult);

            dialog.ShowDialog();
        }
    }
}
