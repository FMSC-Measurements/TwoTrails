﻿using System;
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
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for PointLocManipDialog.xaml
    /// </summary>
    public partial class PointLocManipDialog : Window
    {
        TtHistoryManager _Manager;
        List<TtPoint> _Points;

        public PointLocManipDialog(TtHistoryManager manager, List<TtPoint> points, bool quondam = false, bool reverse = false, TtPolygon target = null)
        {
            _Manager = manager;
            _Points = points;

            InitializeComponent();

            cboPoly.ItemsSource = manager.GetPolyons();

            if (target != null)
            {
                cboPoly.SelectedItem = target;
                cboPolyPoints.ItemsSource = manager.GetPoints(target.CN);
            }

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
            this.DialogResult = false;
        }

        private void btnCommitClick(object sender, RoutedEventArgs e)
        {
            if (cboPoly.SelectedIndex > 0)
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

                    TtPolygon targetPoly = cboPoly.SelectedItem as TtPolygon;
                    bool reverse = rbDirReverse.IsChecked == true;

                    if (rbActQuondam.IsChecked == true)
                    {
                        _Manager.CreateQuondamLinks(_Points, targetPoly, index, null, reverse);
                    }
                    else
                    {
                        _Manager.MovePointsToPolygon(_Points, targetPoly, index, reverse);
                    } 

                    this.Close(); 
                }
            }
            else
            {
                MessageBox.Show("No Target Polygon selected");
            }
        }

        private void cboPoly_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TtPolygon polygon = (cboPoly.SelectedItem as TtPolygon);
            if (polygon != null)
            {
                cboPolyPoints.ItemsSource = _Manager.GetPoints(polygon.CN);
                cboPoly.ToolTip = polygon.Name; 
            }
        }

        public static bool? ShowDialog(TtHistoryManager manager, List<TtPoint> points, bool quondam = false, bool reverse = false, TtPolygon target = null)
        {
            PointLocManipDialog plmd = new PointLocManipDialog(manager, points, quondam, reverse, target);
            return plmd.ShowDialog();
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
    }
}