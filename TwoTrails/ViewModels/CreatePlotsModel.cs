﻿using FMSC.Core.ComponentModel;
using FMSC.Core;
using FMSC.Core.Collections;
using FMSC.Core.Windows.ComponentModel.Commands;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Utils;
using Point = FMSC.Core.Point;
using TwoTrails.Settings;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace TwoTrails.ViewModels
{
    public class CreatePlotsModel : BaseModel
    {
        private readonly TtProject _Project;
        private TtHistoryManager _Manager => _Project.HistoryManager;
        public TtSettings Settings { get; }
        private readonly Window _Window;

        public ICommand GenerateCommand { get; }
        public ICommand CloseCancelCommand { get; }

        public ICommand InclusionPolygonsSelectedCommand { get; }
        public ICommand ExclusionPolygonsSelectedCommand { get; }

        public ICommand ClearSelectedStartPoint { get; }


        public ObservableFilteredSortableCollection<TtPolygon, string> InclusionPolygons { get; }
        public ObservableCollection<TtPolygon> ExclusionPolygons { get; }

        public List<TtPolygon> IncludedPolygons;
        public List<TtPolygon> ExcludedPolygons;

        public bool MultiplePolysIncluded { get { return IncludedPolygons.Count > 1; } }

        public ObservableCollection<TtPoint> Points
        {
            get { return Get<ObservableCollection<TtPoint>>(); }
            set { Set(value); }
        }

        public TtPoint SelectedPoint { get { return Get<TtPoint>(); } set { Set(value); } }

        public Distance UomDistance { get { return Get<Distance>(); } set { Set(value); } }


        public double GridX { get { return Get<double>(); } set { Set(value); } }
        public double GridY { get { return Get<double>(); } set { Set(value); } }

        public int Tilt { get { return Get<int>(); } set { Set(value); } }


        public bool? SamplePoints { get { return Get<bool?>(); } set { Set(value); } }

        public SampleType SampleTypeItem { get { return Get<SampleType>(); } set { Set(value); } }
        public int SampleTypeIndex { get { return Get<int>(); } set { Set(value); } }
        public int SampleAmount { get { return Get<int>(); } set { Set(value); } }

        public bool? BoundaryBuffer { get { return Get<bool?>(); } set { Set(value); } }
        public double BufferAmount { get { return Get<double>(); } set { Set(value); } }

        public bool IsGenerating { get { return Get<bool>(); } set { Set(value, () => OnPropertyChanged(nameof(CloseCancelBtnText))); } }
        public string CloseCancelBtnText => IsGenerating ? "Cancel" : "Close";

        public bool SplitToIndividualPolys {
            get => Settings.DeviceSettings.SplitToIndividualPolys;
            set => Set(value, () => {
                if (Settings.DeviceSettings is DeviceSettings ds) { ds.SplitToIndividualPolys = value; }
            });
        }
        public bool DeleteExistingPlots
        {
            get => Settings.DeviceSettings.DeleteExistingPlots;
            set => Set(value, () => {
                if (Settings.DeviceSettings is DeviceSettings ds) { ds.DeleteExistingPlots = value; }
            });
        }

        private int polyCount;

        private readonly Dispatcher _CurrentDispatcher;
        private Task _GenerateTask;
        private CancellationTokenSource _CancellationTokenSource;


        public CreatePlotsModel(TtProject project, Window window)
        {
            _Project = project;
            Settings = project.Settings;
            _Window = window;

            InclusionPolygonsSelectedCommand = new RelayCommand(x => InclusionPolygonsSelected(x as IList));
            ExclusionPolygonsSelectedCommand = new RelayCommand(x => ExclusionPolygonsSelected(x as IList));

            ClearSelectedStartPoint = new RelayCommand(x => SelectedPoint = null);
            
            InclusionPolygons = new ObservableFilteredSortableCollection<TtPolygon, string>(
                project.HistoryManager.Polygons,
                p => _Manager.GetPoints(p.CN).HasAtLeast(2, pt => pt.OnBoundary),
                p => p.Name.ToLower());

            ExclusionPolygons = new ObservableCollection<TtPolygon>();
            
            IncludedPolygons = new List<TtPolygon>();
            ExcludedPolygons = new List<TtPolygon>();
            
            UomDistance = Distance.FeetTenths;

            GridX = GridY = 100;

            SamplePoints = false;
            SampleTypeItem = SampleType.Percent;
            SampleAmount = 100;

            BoundaryBuffer = false;
            BufferAmount = 10;

            SelectedPoint = null;

            GenerateCommand = new RelayCommand(x => ValidateSettings());
            CloseCancelCommand = new RelayCommand(x => CloseOrCancel());

            _CurrentDispatcher = Dispatcher.CurrentDispatcher;
            _Window.Closing += (s, e) =>
            {
                CancelGeneration();
            };
        }

        private void CloseOrCancel()
        {
            if (IsGenerating)
                CancelGeneration();
            else
                _Window.Close();
        }

        private void InclusionPolygonsSelected(IList selectedItems)
        {
            ExclusionPolygons.Clear();
            ExcludedPolygons.Clear();

            IncludedPolygons.Clear();
            IncludedPolygons.AddRange(selectedItems.Cast<TtPolygon>());
            IncludedPolygons.Sort();

            Points = new ObservableCollection<TtPoint>(IncludedPolygons.SelectMany(p => _Manager.GetPoints(p.CN)));
            
            foreach (TtPolygon poly in InclusionPolygons)
            {
                if (!selectedItems.Contains(poly))
                    ExclusionPolygons.Add(poly);
            }

            OnPropertyChanged(nameof(MultiplePolysIncluded), nameof(SplitToIndividualPolys));
        }

        private void ExclusionPolygonsSelected(IList selectedItems)
        {
            ExcludedPolygons.Clear();
            ExcludedPolygons.AddRange(selectedItems.Cast<TtPolygon>());
        }


        public String GeneratedPolyName(IEnumerable<TtPolygon> polys, int rev = 1)
        {
            if (polys.HasAtLeast(2))
            {
                String polyNames = String.Join("_", polys.Select(p => p.Name));

                if (polyNames.Length > 20)
                    polyNames = $"({polys.Count()})";

                return $"MultiPoly_{polyNames}_Plts{(rev > 1 ? $"_{rev}" : String.Empty)}";
            }
            else if (polys.Any())
            {
                return $"{polys.First().Name}_Plts{(rev > 1 ? $"_{rev}" : String.Empty)}";
            }
            else
                return "Plts";
        }

        private TtPolygon GetOrCreateNewPoly(List<TtPolygon> allPolygons, IEnumerable<TtPolygon> polygons)
        {
            TtPolygon poly = null;
            for (int i = 1; i < Int32.MaxValue; i++)
            {
                string gPolyName = GeneratedPolyName(polygons, i);

                try
                {
                    poly = allPolygons.First(p => p.Name == gPolyName);

                    if (DeleteExistingPlots)
                    {
                        _Manager.DeletePointsInPolygon(poly.CN);
                        return poly;
                    }
                }
                catch
                {
                    String created = $"Created from Polygon{(polygons.HasAtLeast(2) ? $"s {String.Join(", ", polygons.Select(p => p.Name))}" : $" {polygons.First().Name}")}.";

                    poly = new TtPolygon()
                    {
                        Name = gPolyName,
                        PointStartIndex = (++polyCount) * 1000 + Consts.DEFAULT_POINT_INCREMENT,
                        Increment = 1,
                        Description = $"Angle: {Tilt}°, Grid({UomDistance.ToStringAbv()}) X:{GridX} Y:{GridY}, {created}"
                    };

                    return poly;
                }
            }

            return poly;
        }


        private void ValidateSettings()
        {
            if (GridX < 1 || GridY < 1)
            {
                MessageBox.Show("Grid Interval must be greater than 0");
                return;
            }

            if (Tilt < -45 || Tilt > 45)
            {
                MessageBox.Show("Y Axis Tilt must be between -45° and +45°");
                return;
            }

            if (SamplePoints == true &&
                (SampleTypeItem == SampleType.Percent && SampleAmount < 1 || SampleAmount > 100) ||
                (SampleTypeItem == SampleType.Points && SampleAmount < 1))
            {
                MessageBox.Show($"The Sample Amount must be {(SampleTypeItem == SampleType.Percent ? "between 1% and 100%." : "greater than 1 point.")}");
                return;
            }

            if (BoundaryBuffer == true && BufferAmount < 1)
            {
                MessageBox.Show("The Boundary Buffer must be greater than 0.");
                return;
            }

            if (IncludedPolygons.Count < 1)
            {
                MessageBox.Show("Must Select at least one polygon to add plots to.");
                return;
            }

            List<TtPolygon> polygons = _Manager.GetPolygons();

            _Manager.StartMultiCommand();
            _CancellationTokenSource = new CancellationTokenSource();
            IsGenerating = true;

            try
            {
                if (!SplitToIndividualPolys || IncludedPolygons.Count < 2)
                {
                    string gPolyName = GeneratedPolyName(IncludedPolygons);

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
                        if (!Settings.DeviceSettings.DeleteExistingPlots)
                        {
                            if (MessageBox.Show($"Plots '{gPolyName}' already exist. Would you like to rename the plots?", "Plots Already Exist", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                poly = GetOrCreateNewPoly(polygons, IncludedPolygons);
                                _Manager.AddPolygon(poly);
                            }
                            else
                            {
                                _Manager.CancelMultiCommand();
                                OnGenerationFinished();
                                return;
                            }
                        }
                        else
                        {
                            _Manager.DeletePointsInPolygon(poly.CN);
                        }
                    }
                    else
                    {
                        poly = new TtPolygon()
                        {
                            Name = gPolyName,
                            PointStartIndex = (polygons.Count + 1) * 1000 + Consts.DEFAULT_POINT_INCREMENT,
                            Increment = 1
                        };

                        _Manager.AddPolygon(poly);
                    }

                    _GenerateTask = Task.Run(() => GeneratePoints(poly), _CancellationTokenSource.Token);
                }
                else
                {
                    _GenerateTask = Task.Run(() => GeneratePointsInIndividualPolys(), _CancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"{ex.Message}\n\t{ex.StackTrace}", "CreatePlotsModel");
                _Manager.CancelMultiCommand();
                OnGenerationFinished("Error generating plots");
            }
        }

        private void GeneratePoints(TtPolygon poly)
        {
            double gridX = FMSC.Core.Convert.Distance(GridX, Distance.Meters, UomDistance);
            double gridY = FMSC.Core.Convert.Distance(GridY, Distance.Meters, UomDistance);

            double angle = FMSC.Core.Convert.DegreesToRadians(Tilt * -1);

            poly.Description = $"Angle: {Tilt}°, Grid({UomDistance.ToStringAbv()}) X:{GridX} Y:{GridY}, Created from Polygon{(IncludedPolygons.Count > 1 ? $"s {String.Join(", ", IncludedPolygons)}" : $" {IncludedPolygons.First().Name}")}";

            List<IEnumerable<TtPoint>> polyIncludeTtPoints = IncludedPolygons.Select(p => _Manager.GetPoints(p.CN).OnBndPoints()).ToList();

            TtMetadata meta = polyIncludeTtPoints.First().First().Metadata;


            List<IEnumerable<Point>> polyIncudePoints = polyIncludeTtPoints.Select(pp => pp.SyncPointsToZone()).ToList();
            List<Point> allPoints = polyIncudePoints.SelectMany(pts => pts).ToList(); ;
            
            UtmExtent.Builder builder = new UtmExtent.Builder(meta.Zone);
            builder.Include(allPoints);
            UtmExtent totalExtents = builder.Build();

            if (ShouldCancel()) return;

            Random rand = new Random();
            UTMCoords startCoords = SelectedPoint != null ?
                SelectedPoint.GetCoords(meta.Zone) :
                new UTMCoords(
                    (rand.NextDouble() * (totalExtents.East - totalExtents.West) + totalExtents.West),
                    (rand.NextDouble() * (totalExtents.North - totalExtents.South) + totalExtents.South),
                    polyIncludeTtPoints.First().First().Metadata.Datum,
                    meta.Zone
                );
            
            PolygonCalculator.Boundaries totalPolyBnds = new PolygonCalculator(allPoints).PointBoundaries;

            if (ShouldCancel()) return;

            List<PolygonCalculator> polyIncludeCalcs = polyIncudePoints.Select(pp => new PolygonCalculator(pp)).ToList();

            if (ShouldCancel()) return;

            List<PolygonCalculator> polyExcludeCalcs = ExcludedPolygons.Select(p => _Manager.GetPoints(p.CN).OnBndPoints())
                                                    .Select(pp => pp.Select(p => p.GetCoords(meta.Zone).ToPoint()))
                                                    .Select(pp => new PolygonCalculator(pp)).ToList();
            if (ShouldCancel()) return;


            Point farCorner = TtUtils.GetFarthestCorner(
                startCoords.X, startCoords.Y,
                totalPolyBnds.TopLeft.Y, totalPolyBnds.BottomRight.Y,
                totalPolyBnds.TopLeft.X, totalPolyBnds.BottomRight.X);

            double dist = MathEx.Distance(startCoords.X, startCoords.Y, farCorner.X, farCorner.Y);

            int ptAmtY = (int)(Math.Floor(dist / gridY) + 1);
            int ptAmtX = (int)(Math.Floor(dist / gridX) + 1);
            
            double farLeft = startCoords.X - (ptAmtX * gridX);
            double farRight = startCoords.X + (ptAmtX * gridX);
            double farTop = startCoords.Y + (ptAmtY * gridY);
            double farBottom = startCoords.Y - (ptAmtY * gridY);
            
            int i = 0;
            double j = farLeft;
            double k = farTop;

            List<Point> addPoints = new List<Point>();
            Point tmp;

            while (j <= farRight)
            {
                while (k >= farBottom)
                {
                    tmp = angle != 0 ? MathEx.RotatePoint(j, k, angle, startCoords.X, startCoords.Y) : new Point(j, k);

                    foreach (PolygonCalculator pc in polyIncludeCalcs)
                    {
                        if (pc.IsPointInPolygon(tmp.X, tmp.Y) && !polyExcludeCalcs.Any(pec => pec.IsPointInPolygon(tmp.X, tmp.Y)))
                            addPoints.Add(tmp);

                        if (ShouldCancel()) return;
                    }

                    k -= gridY;
                }
                j += gridX;
                k = farTop;
            }

            if (BoundaryBuffer == true)
            {
                double ba = FMSC.Core.Convert.Distance(BufferAmount, Distance.Meters, UomDistance);
                
                for (i = addPoints.Count - 1; i > -1; i--)
                {
                    Point p = addPoints[i];
                    foreach (List<Point> points in polyIncudePoints.Select(pts => pts.ToList()))
                    {
                        for (int m = 0; m < points.Count - 1; m++)
                        {
                            if (MathEx.DistanceToLine(points[m], points[m + 1], p) < ba)
                            {
                                addPoints.RemoveAt(i);
                                break;
                            }
                        }

                        if (ShouldCancel()) return;
                    }
                }
            }

            if (SamplePoints == true)
            {
                int maxPoints = SampleTypeItem == SampleType.Percent ?
                    (int)((SampleAmount / 100.0) * addPoints.Count) : SampleAmount;

                while (maxPoints < addPoints.Count)
                {
                    addPoints.RemoveAt(rand.Next(addPoints.Count - 1));
                }
            }

            if (ShouldCancel())
                return;

            if (addPoints.Count > 100 && PromptToCancel(addPoints.Count))
                return;

            if (addPoints.Count > 0)
            {
                Dictionary<string, TtPoint> wayPoints = new Dictionary<string, TtPoint>();
                WayPoint curr, prev = null;
                int index = 0;

                foreach (Point p in addPoints)
                {
                    curr = new WayPoint()
                    {
                        UnAdjX = p.X,
                        UnAdjY = p.Y,
                        Polygon = poly,
                        Group = _Manager.MainGroup,
                        Metadata = meta,
                        Index = index++,
                        Comment = "Generated Point",
                        PID = PointNamer.NamePoint(poly, prev)
                    };

                    while (_Manager.PointExists(curr.CN) || wayPoints.ContainsKey(curr.CN))
                        curr.CN = Guid.NewGuid().ToString();

                    wayPoints.Add(curr.CN, curr);
                    prev = curr;
                    j++;
                }

                _Manager.AddPoints(wayPoints.Values);

                CommitNewPlots($"Created {wayPoints.Count} plot points in {poly.Name}");
                OnGenerationFinished($"{wayPoints.Count} plots created");
            }
            else
            {
                _Manager.CancelMultiCommand();
                OnGenerationFinished("No plots created");
            }
        }

        private void GeneratePointsInIndividualPolys()
        {
            double gridX = FMSC.Core.Convert.Distance(GridX, Distance.Meters, UomDistance);
            double gridY = FMSC.Core.Convert.Distance(GridY, Distance.Meters, UomDistance);

            double angle = Tilt * -1;

            int polyCount = _Manager.PolygonCount;

            List<TtPolygon> allPolys = _Manager.GetPolygons();
            this.polyCount = allPolys.Count;

            List<Tuple<TtPolygon, string>> polys = IncludedPolygons.Select(p =>
            {
                return Tuple.Create(GetOrCreateNewPoly(allPolys, new TtPolygon[] { p }), p.CN);
            }).ToList();


            if (ShouldCancel()) return;

            TtMetadata defMeta = _Manager.DefaultMetadata;

            List<Tuple<TtPolygon, IEnumerable<Point>, string>> polyIncudePoints =
                polys.Select(p => Tuple.Create(
                    p.Item1,
                    _Manager.GetPoints(p.Item2).OnBndPoints().SyncPointsToZone(defMeta.Zone),
                    p.Item2))
                .ToList();

            if (ShouldCancel()) return;

            List<Point> allPoints = polyIncudePoints.SelectMany(pts => pts.Item2).ToList(); ;

            UtmExtent.Builder builder = new UtmExtent.Builder(defMeta.Zone);
            builder.Include(allPoints);
            UtmExtent totalExtents = builder.Build();

            if (ShouldCancel()) return;

            Random rand = new Random();
            UTMCoords startCoords = SelectedPoint != null ?
                SelectedPoint.GetCoords(defMeta.Zone) :
                new UTMCoords(
                    (rand.NextDouble() * (totalExtents.East - totalExtents.West) + totalExtents.West),
                    (rand.NextDouble() * (totalExtents.North - totalExtents.South) + totalExtents.South),
                    SelectedPoint.Metadata.Datum,
                    defMeta.Zone
                );

            PolygonCalculator.Boundaries totalPolyBnds = new PolygonCalculator(allPoints).PointBoundaries;

            if (ShouldCancel()) return;

            List<Tuple<TtPolygon, PolygonCalculator, string>> polyIncludeCalcs = polyIncudePoints.Select(pp => Tuple.Create(pp.Item1, new PolygonCalculator(pp.Item2), pp.Item3)).ToList();

            if (ShouldCancel()) return;

            List<PolygonCalculator> polyExcludeCalcs = ExcludedPolygons.Select(p => _Manager.GetPoints(p.CN).OnBndPoints())
                                                    .Select(pp => new PolygonCalculator(pp.SyncPointsToZone(defMeta.Zone))).ToList();

            if (ShouldCancel()) return;

            Point farCorner = TtUtils.GetFarthestCorner(
                startCoords.X, startCoords.Y,
                totalPolyBnds.TopLeft.Y, totalPolyBnds.BottomRight.Y,
                totalPolyBnds.TopLeft.X, totalPolyBnds.BottomRight.X);

            double dist = MathEx.Distance(startCoords.X, startCoords.Y, farCorner.X, farCorner.Y);

            int ptAmtY = (int)(Math.Floor(dist / gridY) + 1);
            int ptAmtX = (int)(Math.Floor(dist / gridX) + 1);

            double farLeft = startCoords.X - (ptAmtX * gridX);
            double farRight = startCoords.X + (ptAmtX * gridX);
            double farTop = startCoords.Y + (ptAmtY * gridY);
            double farBottom = startCoords.Y - (ptAmtY * gridY);

            int i = 0;
            double j = farLeft;
            double k = farTop;

            Dictionary<string, Tuple<TtPolygon, List<Point>>> addPoints = polys.ToDictionary(p => p.Item2, p => Tuple.Create(p.Item1, new List<Point>()));
            Point tmp;

            while (j <= farRight)
            {
                while (k >= farBottom)
                {
                    tmp = angle != 0 ? MathEx.RotatePoint(j, k, angle, startCoords.X, startCoords.Y) : new Point(j, k);

                    foreach (Tuple<TtPolygon, PolygonCalculator, string> tpc in polyIncludeCalcs)
                    {
                        if (tpc.Item2.IsPointInPolygon(tmp.X, tmp.Y) && !polyExcludeCalcs.Any(pec => pec.IsPointInPolygon(tmp.X, tmp.Y)))
                            addPoints[tpc.Item3].Item2.Add(tmp);

                        if (ShouldCancel())
                            return;
                    }

                    k -= gridY;
                }
                j += gridX;
                k = farTop;
            }

            if (BoundaryBuffer == true)
            {
                double ba = FMSC.Core.Convert.Distance(BufferAmount, Distance.Meters, UomDistance);

                foreach (List<Point> points in addPoints.Values.Select(plypts => plypts.Item2))
                {
                    for (i = points.Count - 1; i > -1; i--)
                    {
                        Point p = points[i];
                        foreach (List<Point> ipoints in polyIncudePoints.Select(pts => pts.Item2.ToList()))
                        {
                            for (int m = 0; m < ipoints.Count - 1; m++)
                            {
                                if (MathEx.DistanceToLine(ipoints[m], ipoints[m + 1], p) < ba)
                                {
                                    points.RemoveAt(i);
                                    break;
                                }
                            }

                            if (ShouldCancel()) return;
                        }
                    }
                }
            }

            if (SamplePoints == true)
            {
                foreach (List<Point> points in addPoints.Values.Select(plypts => plypts.Item2))
                {
                    int maxPoints = SampleTypeItem == SampleType.Percent ?
                                (int)((SampleAmount / 100.0) * points.Count) : SampleAmount;

                    while (maxPoints < points.Count)
                    {
                        points.RemoveAt(rand.Next(points.Count - 1));
                    } 
                }
            }

            if (ShouldCancel()) return;

            Dictionary<string, TtPoint> wayPoints = new Dictionary<string, TtPoint>();
            int polysAdded = 0;

            int totalPlots = addPoints.Sum(pp => pp.Value.Item2.Count);
            if (totalPlots > 500 && PromptToCancel(totalPlots))
                return;

            foreach (Tuple<TtPolygon, List<Point>> polypts in addPoints.Values)
            {
                WayPoint curr, prev = null;

                if (!allPolys.Contains(polypts.Item1))
                {
                    _Manager.AddPolygon(polypts.Item1);
                    polysAdded++;
                }

                int index = 0;
                foreach (Point p in polypts.Item2)
                {
                    curr = new WayPoint()
                    {
                        UnAdjX = p.X,
                        UnAdjY = p.Y,
                        Polygon = polypts.Item1,
                        Group = _Manager.MainGroup,
                        Metadata = defMeta,
                        Index = index,
                        Comment = "Generated Point",
                        PID = PointNamer.NamePoint(polypts.Item1, prev)
                    };

                    while (_Manager.PointExists(curr.CN) || wayPoints.ContainsKey(curr.CN))
                        curr.CN = Guid.NewGuid().ToString();

                    wayPoints.Add(curr.CN, curr);
                    prev = curr;
                    j++;
                }
            }

            _Manager.AddPoints(wayPoints.Values);

            if (wayPoints.Count > 0)
            {
                CommitNewPlots($"Created {wayPoints.Count} plot points in {(polysAdded > 1 ? $"{polysAdded} units" : $"unit {wayPoints.Values.First().Polygon.Name}")}");
                OnGenerationFinished($"{wayPoints.Count} plots created");
            }
            else
            {
                _Manager.CancelMultiCommand();
                OnGenerationFinished("No plots created");
            }
        }

        private void OnGenerationFinished(String message = null)
        {
            IsGenerating = false;
            if (!String.IsNullOrEmpty(message))
                MessageBox.Show(message);
        }

        private bool ShouldCancel()
        {
            if (_CancellationTokenSource != null && _CancellationTokenSource.IsCancellationRequested)
            {
                _Manager.CancelMultiCommand();
                IsGenerating = false;
                _CancellationTokenSource = null;
                _GenerateTask = null;
                return true;
            }

            return false;
        }

        private void CommitNewPlots(string message)
        {
            _CurrentDispatcher.Invoke(() => _Manager.CommitMultiCommand(DataActionType.InsertedPoints, message));
        }

        private void CancelGeneration()
        {
            if (_GenerateTask != null && _GenerateTask.Status == TaskStatus.Running && _CancellationTokenSource != null)
            {
                _CancellationTokenSource.Cancel();
            }
        }

        private bool PromptToCancel(int totalPlots)
        {
            SetWindowEnabled(false);

            if (MessageBox.Show($"{totalPlots} will be generated. This may take a while and TwoTrails may look frozen. Would you like to continue?",
                "Massive point generation", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                CancelGeneration();
                ShouldCancel();
                SetWindowEnabled(true);
                return true;
            }

            SetWindowEnabled(true);

            return false;
        }

        private void SetWindowEnabled(bool enabled)
        {
            _CurrentDispatcher.Invoke(() =>
            {
                _Window.IsEnabled = enabled;
            });
        }
    }


    public static class UTMCoordExtension
    {
        public static Point ToPoint(this UTMCoords coords)
        {
            return new Point(coords.X, coords.Y);
        }
    }
}
