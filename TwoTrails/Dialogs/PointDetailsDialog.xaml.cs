using System.Collections.Generic;
using System.Windows;
using TwoTrails.Core.Points;
using static TwoTrails.Controls.PointDetailsControl;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for PointDetailsDialog.xaml
    /// </summary>
    public partial class PointDetailsDialog : Window
    {
        public IList<TtPoint> Points { get; private set; }
        public PointFieldController FieldController { get; set; }

        public PointDetailsDialog(IList<TtPoint> points)
        {
            Points = points;
            FieldController = new PointFieldController();
            DataContext = this;
            InitializeComponent();
        }

        public static void ShowDialog(IList<TtPoint> points, Window parentWindow = null)
        {
            PointDetailsDialog d = new PointDetailsDialog(points);
            if (parentWindow != null)
                d.Owner = parentWindow;
            d.ShowDialog();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
