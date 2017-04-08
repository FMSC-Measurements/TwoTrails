using FMSC.Core;
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
    /// Interaction logic for CreatePointDialog.xaml
    /// </summary>
    public partial class CreatePointDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ITtManager _Manager;
        
        public String Txt1Watermark { get { return radUTM.IsChecked == true ? "UTM X" : "Latitude"; } }
        public String Txt2Watermark { get { return radUTM.IsChecked == true ? "UTM Y" : "Longitude"; } }
        
        public String Txt3Watermark { get; private set; }


        public CreatePointDialog(ITtManager manager, TtPolygon target = null)
        {
            _Manager = manager;

            DataContext = this;
            InitializeComponent();

            cboPoly.ItemsSource = _Manager.GetPolygons();
            cboPoly.SelectedItem = target??cboPoly.Items[0];

            cboMeta.ItemsSource = _Manager.GetMetadata();
            cboMeta.SelectedItem = _Manager.DefaultMetadata;
            UpdateMetadata(_Manager.DefaultMetadata);

            cboGroup.ItemsSource = _Manager.GetGroups();
            cboGroup.SelectedItem = _Manager.MainGroup;
        }

        private void UpdateMetadata(TtMetadata metadata)
        {
            if (metadata != null)
            {
                Txt3Watermark = $"Elevation ({metadata.Elevation.ToStringAbv()})";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Txt3Watermark)));
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

        private void cboMeta_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMetadata(cboMeta.SelectedItem as TtMetadata);
        }

        private void radUTM_CheckChanged(object sender, RoutedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Txt1Watermark)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Txt2Watermark)));
        }

        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnCreateClick(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txt1.Text, out double a))
            {
                if (double.TryParse(txt2.Text, out double b))
                {
                    bool elevIsEmpty = String.IsNullOrWhiteSpace(txt3.Text);
                    if (elevIsEmpty || double.TryParse(txt3.Text, out double c))
                    {
                        TtPolygon poly = cboPoly.SelectedItem as TtPolygon;
                        TtMetadata meta = cboMeta.SelectedItem as TtMetadata;
                        TtGroup group = cboGroup.SelectedItem as TtGroup;

                        TtPoint prevPoint = null;

                        if (rbInsAft.IsChecked == true)
                        {
                            prevPoint = cboPolyPoints.Items.GetItemAt(cboPolyPoints.SelectedIndex) as TtPoint;
                        }
                        else if (rbInsEnd.IsChecked == true)
                        {
                            prevPoint = cboPolyPoints.Items.GetItemAt(cboPolyPoints.Items.Count - 1) as TtPoint;
                        }

                        if (elevIsEmpty)
                            c = 0;

                        bool pidIsEmpty = String.IsNullOrWhiteSpace(txt5.Text);
                        int pid = 0;
                        if (!pidIsEmpty && !Int32.TryParse(txt5.Text, out pid))
                        {
                            if (MessageBox.Show($"{txt5.Text} is not a valid PID. Would you like to automatically name your point?",
                                "Invalid PID Name", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                            {
                                pid = PointNamer.NamePoint(poly, prevPoint);
                            }
                            else return;
                        }

                        if (pidIsEmpty)
                            pid = PointNamer.NamePoint(poly, prevPoint);

                        bool onBnd = chkBnd.IsChecked == true;

                        bool manAccIsEmpty = String.IsNullOrWhiteSpace(txt6.Text);
                        double? manAcc = null;
                        if (!manAccIsEmpty)
                        {
                            if (double.TryParse(txt6.Text, out double ma))
                                manAcc = ma;
                            else
                            {
                                if (MessageBox.Show($"{txt6.Text} is not a valid Manual Accuracy. Would you like to continue without giving the point an Accuracy?",
                                    "Invalid Manual Accuracy", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.No)
                                    return;
                            } 
                        }

                        GpsPoint point = new GpsPoint()
                        {
                            Polygon = poly,
                            Metadata = meta,
                            Group = group,
                            PID = pid,
                            OnBoundary =  onBnd,
                            Comment = txt4.Text,
                            ManualAccuracy = manAcc
                        };

                        return;
                    }
                    else
                    {
                        MessageBox.Show("Invalid Elevation", "");
                    }
                }
                else
                {
                    MessageBox.Show($"Invalid {Txt2Watermark}", "");
                }
            }
            else
            {
                MessageBox.Show($"Invalid {Txt1Watermark}", "");
            }
        }

        public static bool? ShowDialog(ITtManager manager, TtPolygon target = null, Window owner = null)
        {
            CreatePointDialog cpd = new CreatePointDialog(manager, target);
            if (owner != null)
                cpd.Owner = owner;
            return cpd.ShowDialog();
        }
    }
}
