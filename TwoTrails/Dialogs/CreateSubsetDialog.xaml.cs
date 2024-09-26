using FMSC.Core.Collections;
using FMSC.Core.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Core.Units;
using TwoTrails.Settings;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for RenamePointsDialog.xaml
    /// </summary>
    public partial class CreateSubsetDialog : Window
    {
        private TtProject _Project;

        public TtPolygon SelectedPlotPolygon { get; set; }
        public int SubsetValue { get; set; }
        public bool IsPercentMode { get; set; } = true;
        public bool DeleteExistingPlots
        {
            get => _Project.Settings.DeviceSettings.DeleteExistingPlots;
            set { if (_Project.Settings.DeviceSettings is DeviceSettings ds) { ds.DeleteExistingPlots = value; } }
        }

        public ObservableFilteredCollection<TtPolygon> PlotPolygons { get; }

        public CreateSubsetDialog(TtProject project)
        {
            _Project = project;

            PlotPolygons = new ObservableFilteredCollection<TtPolygon>(_Project.HistoryManager.Polygons,
                poly => _Project.HistoryManager.GetPoints(poly.CN).All(p => p.IsWayPointAtBase()));

            this.Unloaded += CreateSubsetDialog_Unloaded;

            InitializeComponent();
            this.DataContext = this;
        }

        private void CreateSubsetDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= CreateSubsetDialog_Unloaded;
            PlotPolygons.Dispose();
        }

        private void CreateSubset()
        {
            if (SelectedPlotPolygon == null)
            {
                MessageBox.Show("A polygon filled with plots must be selected.");
                return;
            }

            if (SubsetValue < 1)
            {
                MessageBox.Show("You must have at least 1 point or 1% to create a subset.");
                return;
            }

            if (IsPercentMode == true && SubsetValue >= 100)
            {
                MessageBox.Show("You can have a maximum of 99% of points to create a subset.");
                return;
            }
            
            
            List<TtPolygon> polygons = _Project.HistoryManager.GetPolygons();
            string gPolyName = GeneratePolyName(SelectedPlotPolygon);

            TtPolygon poly = null;

            try
            {
                poly = polygons.First(p => p.Name == gPolyName);
            }
            catch
            {
                //
            }
            

            if (poly != null)
            {
                if (!DeleteExistingPlots)
                {
                    if (MessageBox.Show($"Plots '{gPolyName}' already exist. Would you like to rename the plots?", "Plots Already Exist", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        _Project.HistoryManager.StartMultiCommand();

                        poly = null;

                        for (int i = 2; i < Int32.MaxValue; i++)
                        {
                            gPolyName = GeneratePolyName(SelectedPlotPolygon, i);

                            try
                            {
                                poly = polygons.First(p => p.Name == gPolyName);
                            }
                            catch
                            {
                                poly = new TtPolygon()
                                {
                                    Name = gPolyName,
                                    PointStartIndex = (polygons.Count + 1) * 1000 + Consts.DEFAULT_POINT_INCREMENT,
                                    Increment = 1
                                };

                                _Project.HistoryManager.AddPolygon(poly);
                                break;
                            }
                        }
                    }
                    else return;
                }
                else
                {
                    _Project.HistoryManager.StartMultiCommand();
                    _Project.HistoryManager.DeletePointsInPolygon(poly.CN);
                }
            }
            else
            {
                _Project.HistoryManager.StartMultiCommand();

                poly = new TtPolygon()
                {
                    Name = gPolyName,
                    PointStartIndex = (polygons.Count + 1) * 1000 + Consts.DEFAULT_POINT_INCREMENT,
                    Increment = 1
                };

                _Project.HistoryManager.AddPolygon(poly);
            }

            List<TtPoint> points = _Project.HistoryManager.GetPoints(SelectedPlotPolygon.CN);

            int maxPoints = IsPercentMode ? (int)((SubsetValue / 100.0) * points.Count) : SubsetValue > points.Count ? points.Count : SubsetValue;
            Random rand = new Random(DateTime.Now.Millisecond);


            poly.Description = $"Subsample of {SelectedPlotPolygon.Name}. {maxPoints} of {points.Count} points.";

            while (maxPoints < points.Count)
            {
                points.RemoveAt(rand.Next(points.Count - 1));
            }

            List<TtPoint> wayPoints = new List<TtPoint>();
            int index = 0;
            WayPoint curr, prev = null;

            foreach (TtPoint p in points)
            {
                curr = new WayPoint()
                {
                    UnAdjX = p.UnAdjX,
                    UnAdjY = p.UnAdjY,
                    Polygon = poly,
                    Group = _Project.HistoryManager.MainGroup,
                    Metadata = p.Metadata,
                    Index = index++,
                    Comment = $"Generated from {p.PID} : {p.Polygon.Name}",
                    PID = PointNamer.NamePoint(poly, prev)
                };

                wayPoints.Add(curr);
                prev = curr;
            }

            _Project.HistoryManager.AddPoints(wayPoints);
            
            _Project.HistoryManager.CommitMultiCommand(DataActionType.InsertedPoints, $"Created {points.Count} points as a subset of plot {SelectedPlotPolygon.Name}");

            MessageBox.Show($"{points.Count} WayPoints Created");
        }


        public String GeneratePolyName(TtPolygon poly, int rev = 1)
        {
            return $"{poly.Name}_Sample{(rev > 1 ? $"_{rev}" : String.Empty)}";
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ControlUtils.TextIsUnsignedInteger(sender, e);
        }


        private void Create_Click(object sender, RoutedEventArgs e)
        {
            CreateSubset();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        public static bool? ShowDialog(TtProject project, Window owner = null)
        {
            CreateSubsetDialog dialog = new CreateSubsetDialog(project);

            if (owner != null)
                dialog.Owner = owner;

            return dialog.ShowDialog();
        }

        public static void Show(TtProject project, Window owner = null, Action onClose = null)
        {
            CreateSubsetDialog dialog = new CreateSubsetDialog(project);

            if (owner != null)
                dialog.Owner = owner;

            if (onClose != null)
            {
                dialog.Closed += (s, e) => onClose();
            }

            dialog.Show();
        }
    }
}
