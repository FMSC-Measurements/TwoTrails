using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SelectPointDialog.xaml
    /// </summary>
    public partial class SelectPointDialog : Window
    {
        public TtPoint SelectedPoint { get { return lbPoints.SelectedItem as TtPoint; } }
        
        private Dictionary<String, IEnumerable<TtPoint>> _Points = new Dictionary<string, IEnumerable<TtPoint>>();

        private ITtManager _Manager;
        private IEnumerable<string> _HidePoints;

        public SelectPointDialog(ITtManager manager, List<TtPoint> hidePoints = null)
        {
            _Manager = manager;
            _HidePoints = hidePoints.Select(p => p.CN);

            InitializeComponent();
            lbPolys.ItemsSource = manager.GetPolyons();
        }

        private void Polygon_Selected(object sender, RoutedEventArgs e)
        {
            TtPolygon poly = lbPolys.SelectedItem as TtPolygon;

            if (poly != null)
            {
                if (_Points.ContainsKey(poly.CN))
                    lbPoints.ItemsSource = _Points[poly.CN];
                else
                {
                    IEnumerable<TtPoint> points = _Manager.GetPoints(poly.CN);

                    if (_HidePoints != null)
                        points = points.Where(p => !_HidePoints.Contains(p.CN));

                    _Points.Add(poly.CN, points);

                    lbPoints.ItemsSource = points;
                }
            }
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPoint != null)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
                MessageBox.Show("No Point Selected");
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
