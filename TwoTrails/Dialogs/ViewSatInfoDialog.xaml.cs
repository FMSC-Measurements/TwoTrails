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
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for ViewSatInfoDialog.xaml
    /// </summary>
    public partial class ViewSatInfoDialog : Window
    {
        public List<TtNmeaBurst> Bursts { get; set; }


        public ViewSatInfoDialog(ITtManager manager, IEnumerable<TtPoint> points)
        {
            Bursts = manager.GetNmeaBursts(points.Select(p => p.CN));
            DataContext = this;
            InitializeComponent();
        }


        public static void ShowDialog(IList<TtPoint> points, ITtManager manager, Window parentWindow = null)
        {
            ViewSatInfoDialog d = new ViewSatInfoDialog(manager, points);
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
