using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TwoTrails.Core;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for PointLocManipDialog.xaml
    /// </summary>
    public partial class PointLocManipDialog : Window
    {
        private TtHistoryManager _Manager;
        private List<TtPoint> _Points;
        public TtPolygon TargetPoly { get; private set; }

        public PointLocManipDialog(TtHistoryManager manager, List<TtPoint> points, bool quondam = false, bool reverse = false, TtPolygon target = null)
        {
            if (points == null || points.Count < 1)
                throw new Exception("No Points");

            _Manager = manager;
            _Points = points;

            InitializeComponent();

            cboPoly.ItemsSource = manager.GetPolygons();

            if (target != null)
                target = points.First().Polygon;

            cboPoly.SelectedItem = target;
            cboPolyPoints.ItemsSource = manager.GetPoints(target?.CN);

            if (quondam)
                rbActQuondam.IsChecked = true;
            else
                rbActMove.IsChecked = true;

            if (reverse)
                rbDirReverse.IsChecked = true;
            else
                rbDirForward.IsChecked = true;
        }

        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCommitClick(object sender, RoutedEventArgs e)
        {
            if (cboPoly.SelectedIndex > -1)
            {
                if (rbInsAft.IsChecked == true && cboPolyPoints.SelectedIndex < 0)
                {
                    MessageBox.Show("No Point to insert after is selected");
                }
                else
                {
                    int index = rbInsBeg.IsChecked == true ? 0
                                    : rbInsEnd.IsChecked == true ? int.MaxValue
                                        : cboPolyPoints.SelectedIndex + 1;

                    TargetPoly = cboPoly.SelectedItem as TtPolygon;
                    bool reverse = rbDirReverse.IsChecked == true;

                    if (rbActQuondam.IsChecked == true)
                    {
                        _Manager.CreateQuondamLinks(_Points, TargetPoly, index, QuondamBoundaryMode.Inherit, reverse);
                    }
                    else
                    {
                        _Manager.MovePointsToPolygon(_Points, TargetPoly, index, reverse);
                    } 

                    Close(); 
                }
            }
            else
            {
                MessageBox.Show("No Target Polygon selected");
            }
        }

        private void cboPoly_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPoly.SelectedItem is TtPolygon polygon)
            {
                cboPolyPoints.ItemsSource = _Manager.GetPoints(polygon.CN);
                cboPoly.ToolTip = polygon.Name; 
            }
        }

        private void rbActMove_Checked(object sender, RoutedEventArgs e)
        {
            if (rbActMove.IsChecked == true)
                this.Title = "Move Points";
        }

        private void rbActQuondam_Checked(object sender, RoutedEventArgs e)
        {
            if (rbActQuondam.IsChecked == true)
                this.Title = "Quondam Points";
        }


        public static bool? ShowDialog(TtHistoryManager manager, List<TtPoint> points, bool quondam = false, bool reverse = false, TtPolygon target = null, Window owner = null)
        {
            PointLocManipDialog plmd = new PointLocManipDialog(manager, points, quondam, reverse, target);
            if (owner != null)
                plmd.Owner = owner;
            return plmd.ShowDialog();
        }

        public static void Show(TtHistoryManager manager, List<TtPoint> points, bool quondam = false, bool reverse = false, TtPolygon target = null, Window owner = null, Action<TtPolygon> onClose = null)
        {
            PointLocManipDialog plmd = new PointLocManipDialog(manager, points, quondam, reverse, target);
            if (owner != null)
                plmd.Owner = owner;

            if (onClose != null)
            {
                plmd.Closed += (s, e) => onClose(plmd.TargetPoly);
            }

            plmd.Show();
        }
    }
}
