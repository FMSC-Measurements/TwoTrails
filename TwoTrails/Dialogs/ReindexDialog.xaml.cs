using FMSC.Core.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for EditValuesDialog.xaml
    /// </summary>
    public partial class ReindexDialog : Window
    {
        private TtHistoryManager _Manager;
        public ReadOnlyObservableCollection<TtPolygon> Polygons => _Manager.Polygons;
        public TtPolygon SelectedPolygon { get; set; }


        public ReindexDialog(TtHistoryManager manager)
        {
            _Manager = manager;
            this.DataContext = this;
            InitializeComponent();
        }


        private void Reindex(bool byPid)
        {
            if (SelectedPolygon != null)
            {
                List<TtPoint> points = _Manager.GetPoints(SelectedPolygon.CN);

                if (points.Count > 0)
                {
                    if (byPid)
                    {
                        _Manager.EditPointsMultiValues(points.OrderBy(p => p.PID), PointProperties.INDEX, Enumerable.Range(0, points.Count));
                    }
                    else
                    {
                        _Manager.EditPointsMultiValues(points, PointProperties.INDEX, Enumerable.Range(0, points.Count));
                    }


                    if (this.IsShownAsDialog())
                        this.DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Polygon does not contain any points.");
                }
            }
            else
            {
                MessageBox.Show("No Polygon selected.");
            }
        }



        private void CO_Click(object sender, RoutedEventArgs e)
        {
            Reindex(false);
        }

        private void PID_Click(object sender, RoutedEventArgs e)
        {
            Reindex(true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(TtHistoryManager manager, Window owner = null, Action<bool?> onClose = null)
        {
            ReindexDialog dialog = new ReindexDialog(manager);
            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
                dialog.Closed += (s, e) => onClose(dialog.DialogResult);

            dialog.Show();
        }
    }
}
