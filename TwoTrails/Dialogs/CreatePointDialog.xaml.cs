using FMSC.Core;
using FMSC.Core.Windows.Utilities;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for CreatePointDialog.xaml
    /// </summary>
    public partial class CreateGpsPointDialog : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ITtManager _Manager;
        private OpType _OpType;
        
        public String Txt1Watermark
        {
            get
            {
                if (IsGpsType)
                    return radUTM.IsChecked == true ? "UTM X" : "Latitude";
                else
                    return "Forward Azimuth";
            }
        }

        public String Txt2Watermark
        {
            get
            {
                if (IsGpsType)
                    return radUTM.IsChecked == true ? "UTM Y" : "Longitude";
                else
                    return "Backward Azimuth";
            }
        }
        
        public String Txt3Watermark { get; private set; }

        public String Txt4Watermark { get; private set; } = "Manual Accuracy";

        public bool IsGpsType { get { return _OpType.IsGpsType(); } }


        public CreateGpsPointDialog(ITtManager manager, TtPolygon target = null, OpType opType = OpType.GPS)
        {
            if (opType == OpType.Quondam)
                throw new Exception("Invalid Operation: Cannot create quondam");


            _Manager = manager;

            _OpType = opType;

            DataContext = this;
            InitializeComponent();

            this.Title = $"Create {opType}";

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
                if (_OpType.IsGpsType())
                {
                    Txt3Watermark = $"Elevation ({metadata.Elevation.ToStringAbv()})";
                }
                else
                {
                    Txt3Watermark = $"Slope Distance ({metadata.Distance.ToStringAbv()})";
                    Txt4Watermark = $"Slope Angle ({metadata.Slope})";

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Txt4Watermark)));
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Txt3Watermark)));
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
            double? dn = null;
            bool aie = String.IsNullOrWhiteSpace(txt1.Text),
                bie = String.IsNullOrWhiteSpace(txt2.Text),
                cie = String.IsNullOrWhiteSpace(txt3.Text),
                die = String.IsNullOrWhiteSpace(txt6.Text);

            if (aie && bie && !IsGpsType)
            {
                MessageBox.Show("You must enter at least the Forward or Backward Azimuth", String.Empty);
            }
            else
            {
                if (double.TryParse(txt1.Text, out double a) || (aie && !IsGpsType))
                {
                    if (double.TryParse(txt2.Text, out double b) || (bie && !IsGpsType))
                    {
                        if (double.TryParse(txt3.Text, out double c) || (cie && IsGpsType))
                        {
                            TtPolygon poly = cboPoly.SelectedItem as TtPolygon;
                            TtMetadata meta = cboMeta.SelectedItem as TtMetadata;
                            TtGroup group = cboGroup.SelectedItem as TtGroup;

                            TtPoint prevPoint = null, nextPoint = null;

                            int index = 0;

                            if (cboPolyPoints.Items.Count > 0)
                            {
                                if (rbInsAft.IsChecked == true)
                                {
                                    index = cboPolyPoints.SelectedIndex;
                                    prevPoint = cboPolyPoints.Items.GetItemAt(index) as TtPoint;

                                    index++;

                                    if (index < cboPolyPoints.Items.Count)
                                    {
                                        nextPoint = cboPolyPoints.Items.GetItemAt(index) as TtPoint;
                                    }
                                }
                                else if (rbInsEnd.IsChecked == true)
                                {
                                    index = cboPolyPoints.Items.Count - 1;
                                    prevPoint = cboPolyPoints.Items.GetItemAt(index) as TtPoint;
                                    index++;
                                }
                            }

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
                            {
                                pid = PointNamer.NamePoint(poly, prevPoint);

                                if (nextPoint != null && nextPoint.PID < pid)
                                    pid = prevPoint.PID + 1;
                            }

                            bool onBnd = chkBnd.IsChecked == true;

                            if (!die)
                            {
                                if (double.TryParse(txt6.Text, out double ma))
                                    dn = ma;
                                else
                                {
                                    if (MessageBox.Show($"'{txt6.Text}' is not a valid {(IsGpsType ? "Manual Accuracy" : "Slope Angle")}. Would you like to continue without giving the point an {(IsGpsType ? "Accuracy" : "Angle")}?",
                                        $"Invalid {(IsGpsType ? "Manual Accuracy" : "Slope Angle")}", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.No)
                                        return;
                                }
                            }
                            else if (!IsGpsType)
                            {
                                dn = 0;
                            }

                            if (IsGpsType)
                            {
                                GpsPoint point = new GpsPoint()
                                {
                                    Index = index,
                                    Polygon = poly,
                                    Metadata = meta,
                                    Group = group,
                                    PID = pid,
                                    OnBoundary = onBnd,
                                    Comment = txt4.Text,
                                    ManualAccuracy = dn
                                };

                                if (_OpType != OpType.GPS)
                                {
                                    switch (_OpType)
                                    {
                                        case OpType.Take5: point = new Take5Point(point); break;
                                        case OpType.Walk: point = new WalkPoint(point); break;
                                        case OpType.WayPoint: point = new WayPoint(point); break;
                                    }
                                }

                                if (radUTM.IsChecked == true)
                                    point.SetUnAdjLocation(a, b, c);
                                else
                                    point.SetUnAdjLocation(a, b, point.Metadata.Zone, c);

                                point.SetAccuracy(poly.Accuracy);

                                _Manager.AddPoint(point);
                            }
                            else
                            {
                                TravPoint point = new TravPoint()
                                {
                                    Index = index,
                                    Polygon = poly,
                                    Metadata = meta,
                                    Group = group,
                                    PID = pid,
                                    OnBoundary = onBnd,
                                    Comment = txt4.Text,
                                    FwdAzimuth = aie ? null : (double?)a,
                                    BkAzimuth = bie ? null : (double?)b,
                                    SlopeDistance = FMSC.Core.Convert.Distance(c, Distance.Meters, meta.Distance),
                                    SlopeAngle = FMSC.Core.Convert.Angle(dn, Slope.Degrees, meta.Slope) ?? 0
                                };

                                if (_OpType == OpType.SideShot)
                                    point = new SideShotPoint(point);

                                _Manager.AddPoint(point);
                            }

                            if (this.IsShownAsDialog())
                                this.DialogResult = true;

                            Close();
                        }
                        else
                        {
                            MessageBox.Show($"Invalid {(IsGpsType ? "Elevation" : "Slope Distance")}", String.Empty);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Invalid {Txt2Watermark}", String.Empty);
                    }
                }
                else
                {
                    MessageBox.Show($"Invalid {Txt1Watermark}", String.Empty);
                }
            }
        }

        public static bool? ShowDialog(ITtManager manager, TtPolygon target = null, OpType opType = OpType.GPS, Window owner = null)
        {
            CreateGpsPointDialog cpd = new CreateGpsPointDialog(manager, target, opType);
            if (owner != null)
                cpd.Owner = owner;
            return cpd.ShowDialog();
        }
    }
}
