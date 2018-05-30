using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
