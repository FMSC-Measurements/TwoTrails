using FMSC.Core;
using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using FMSC.GeoSpatial;
using FMSC.GeoSpatial.UTM;
using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Dialogs;
using TwoTrails.Mapping;
using Convert = FMSC.Core.Convert;
using Point = FMSC.Core.Point;

namespace TwoTrails.ViewModels
{
    public class PointMinimizationModel : BaseModel
    {
        private const double BOUNDARY_ZOOM_MARGIN = 0.00035;

        public const double MINIMUM_ANGLE = 45d;
        public const double MAXIMUM_ANGLE = 90d;
        public const double MIN_DIST_MULTIPLIER = 5d;
        public const double MAX_DIST_MULTIPLIER = 10d;


        public ICommand PointSelectionChangedCommand { get; }
        public ICommand PointSelectedCommand { get; }
        public ICommand ApplyCommand { get; }
        public ICommand AnalyzeCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ZoomCommand { get; }



        private readonly TtProject Project;
        private TtHistoryManager Manager => Project.HistoryManager;
        private readonly PointMinimizationDialog _Dialog;


        public Map Map { get; } = new Map();

        public List<TtPolygon> Polygons { get; }
        public ReadOnlyObservableCollection<TtPoint> Points { get; private set; }
        private Dictionary<String, TtPoint> _OrigPoints;
        private readonly Dictionary<string, Pushpin> _PushPins = new Dictionary<string, Pushpin>();
        private readonly MapPolygon _OrigPoly = new MapPolygon();
        private readonly MapPolygon _MinPoly = new MapPolygon();
        public Extent Extents { get; set; }

        private readonly SolidColorBrush _OnBoundBrush, _OffBoundBrush;


        public TtPolygon TargetPolygon {
            get => Get<TtPolygon>();
            set => Set(value, () => {
                OnPropertyChanged(nameof(TargetPolygonToolTip));

                List<TtPoint> points = Manager.GetPoints(TargetPolygon.CN);
                _OrigPoints = points.ToDictionary(p => p.CN, p => p);

                Points = new ReadOnlyObservableCollection<TtPoint>(
                    new ObservableCollection<TtPoint>(points.DeepCopy()));

                UpdateOrigPoly();
                AnalyzeTargetPolygon();
            });
        }

        public string TargetPolygonToolTip => TargetPolygon?.ToString();


        public double MinimumAngle { get => Get<double>(); set => Set(value, () => AnalyzeTargetPolygon()); }
        public double? MinimumLegLength { get => Get<double?>(); set => Set(value, () => AnalyzeTargetPolygon()); }
        public double? MaximumLegAdjustDist { get => Get<double?>(); set => Set(value, () => AnalyzeTargetPolygon()); }
        public double? AccuracyOverride { get => Get<double?>(); set => Set(value, () => AnalyzeTargetPolygon()); }

        public bool AnalyzeAllPointsInPoly { get => Get<bool>(); set => Set(value, () => AnalyzeTargetPolygon()); }
        public bool LimitAreaChange { get => Get<bool>(); set => Set(value, () => AnalyzeTargetPolygon()); }
        public bool RespectCurves { get => Get<bool>(); set => Set(value, () => AnalyzeTargetPolygon()); }


        public bool HidePoints {
            get => Get<bool>();
            set => Set(value, () => {
                if (_PushPins.Any())
                {
                    Visibility vis = value ? Visibility.Collapsed : Visibility.Visible;
                    _PushPins.Values.ForEach(pin => { pin.Visibility = vis; });
                }
            });
        }



        private Tuple<double, double, double> APStats {
            get => Get<Tuple<double, double, double>>();
            set => Set(value, () =>
                    OnPropertyChanged(
                        nameof(NewAreaHa),
                        nameof(NewAreaAc),
                        nameof(NewPerimeterM),
                        nameof(NewPerimeterFt),
                        nameof(AreaDifference),
                        nameof(PerimeterDifference)
                    )
                );
        }

        public double? NewAreaHa => APStats != null ? Convert.ToHectare(APStats.Item1, Area.MeterSq) : (double?)null;
        public double? NewAreaAc => APStats != null ? Convert.ToAcre(APStats.Item1, Area.MeterSq) : (double?)null;

        public double? NewPerimeterM => APStats?.Item2;
        public double? NewPerimeterFt => APStats != null ? Convert.ToFeetTenths(APStats.Item2, Distance.Meters) : (double?)null;

        public double? AreaDifference => APStats != null ? (double?)GetDiff(APStats.Item1, TargetPolygon.Area) : null;
        public double? PerimeterDifference => APStats != null ? (double?)GetDiff(APStats.Item2, TargetPolygon.Perimeter) : null;

        private double GetDiff(double i1, double i2)
        {
            double diff = (i1 / i2);
            return ((diff > 1) ? (diff - 1) : (1 - diff)) * 100d;
        }



        public PointMinimizationModel(TtProject project, PointMinimizationDialog dialog)
        {
            Project = project;
            Polygons = Manager.Polygons.Where(p => Manager.GetPoints(p.CN).HasAtLeast(3)).ToList();
            Polygons.Sort(new PolygonSorterDirect(project.Settings.SortPolysByName));
            _Dialog = dialog;

            PointSelectedCommand = new RelayCommand(x =>
            {
                if (x is CompositeCommandParameter param && param.Parameter is TtPoint point)
                    PointSelected(point);
            });

            CancelCommand = new RelayCommand(x => _Dialog.Close());
            AnalyzeCommand = new BindedRelayCommand<PointMinimizationModel>(
                x => AnalyzeTargetPolygon(),
                x=> TargetPolygon != null,
                this,
                x => this.TargetPolygon);

            ZoomCommand = new BindedRelayCommand<PointMinimizationModel>(
                x =>
                {
                    Map.SetView(
                        new LocationRect(
                            new Location(Extents.North + BOUNDARY_ZOOM_MARGIN, Extents.West - BOUNDARY_ZOOM_MARGIN),
                            new Location(Extents.South - BOUNDARY_ZOOM_MARGIN, Extents.East + BOUNDARY_ZOOM_MARGIN)));
                    Map.Refresh();
                },
                x => Extents != null,
                this,
                x => this.Extents);
            ApplyCommand = new BindedRelayCommand<PointMinimizationModel>(
                x => Apply(),
                x => APStats != null,
                this,
                x => this.APStats);

            MinimumAngle = MINIMUM_ANGLE;
            AnalyzeAllPointsInPoly = true;
            LimitAreaChange = true;

            _OnBoundBrush = (SolidColorBrush)Application.Current.Resources["scbPrimary"];
            _OffBoundBrush = (SolidColorBrush)Application.Current.Resources["scbBackground"];

            _OrigPoly.Stroke = new SolidColorBrush(Colors.LightGray);
            _OrigPoly.StrokeThickness = 5;

            _MinPoly.Stroke = new SolidColorBrush(Colors.Red);
            _MinPoly.StrokeThickness = 3;


            Map.MouseDoubleClick += (s, e) => e.Handled = true;
            Map.CredentialsProvider = new ApplicationIdCredentialsProvider(APIKeys.BING_MAPS_API_KEY);
            Map.Mode = new AerialMode();

            Map.Children.Add(_OrigPoly);
            Map.Children.Add(_MinPoly);

            Map.Loaded += OnMapLoaded;

            TargetPolygon = Polygons.FirstOrDefault();
        }

        private void OnMapLoaded(object sender, RoutedEventArgs e)
        {
            if (Map.ActualHeight > 0)
            {
                Map.Loaded -= OnMapLoaded;

                if (Extents == null)
                    UpdateMinimizedPoly();


                if (Extents != null)
                {
                    Map.SetView(
                        new LocationRect(
                            new Location(Extents.North + BOUNDARY_ZOOM_MARGIN, Extents.West - BOUNDARY_ZOOM_MARGIN),
                            new Location(Extents.South - BOUNDARY_ZOOM_MARGIN, Extents.East + BOUNDARY_ZOOM_MARGIN)));

                    Map.Refresh();
                }
            }
        }

        private void PointSelected(TtPoint point)
        {
            point.OnBoundary = !point.OnBoundary;
            _PushPins[point.CN].Background = point.OnBoundary ? _OnBoundBrush : _OffBoundBrush;
            UpdateMinimizedPoly(false);

            _Dialog.lbPoints.ScrollIntoView(point);
        }


        public void UpdateOrigPoly()
        {
            if (Points != null && Points.Count > 2)
            {
                LocationCollection locations = new LocationCollection();
                Extent.Builder eb = new Extent.Builder();   

                if (_PushPins.Count > 0)
                {
                    foreach (Pushpin pushpin in _PushPins.Values)
                    {
                        Map.Children.Remove(pushpin);
                    }

                    _PushPins.Clear();
                }

                foreach (TtPoint point in Points.OrderBy(p => p.Index))
                {
                    Point ll = point.GetLatLon();
                    eb.Include(ll.Y, ll.X);
                    Location location = new Location(ll.Y, ll.X);

                    Pushpin pushpin = new Pushpin();
                    pushpin.Location = location;
                    pushpin.Background = _OffBoundBrush;
                    pushpin.Visibility = HidePoints ? Visibility.Collapsed : Visibility.Visible;
                    pushpin.MouseDoubleClick += (s, e) => PointSelected(point);

                    pushpin.Content = new Label()
                    {
                        FontSize = 6,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(-5, 0, -5, 0),
                        Foreground = new SolidColorBrush(Colors.White),
                        Content = point.PID
                    };

                    pushpin.ToolTip = $"({point.PID})\nX: {point.AdjX:F3}\nY: {point.AdjY:F3}";

                    Map.Children.Add(pushpin);

                    _PushPins.Add(point.CN, pushpin);
                    locations.Add(location);
                }

                _OrigPoly.Locations = locations;
                _OrigPoly.Visibility = Visibility.Visible;

                Extents = eb.Build();
            }
            else
            {
                _OrigPoly.Visibility = Visibility.Hidden;
            }

            Map.Refresh();
        }

        public void UpdateMinimizedPoly(bool updatePushpins = true)
        {
            if (Points != null && Points.Count > 2)
            {
                List<TtPoint> points = Points.Where(p => p.IsBndPoint()).OrderBy(p => p.Index).ToList();
                LocationCollection locations = new LocationCollection();
                foreach (TtPoint point in points)
                {
                    Pushpin pushpin = _PushPins[point.CN];
                    locations.Add(pushpin.Location);

                    if (updatePushpins)
                        pushpin.Background = _OnBoundBrush;
                }

                _MinPoly.Locations = locations;
                _MinPoly.Visibility = Visibility.Visible;

                APStats = TtCoreUtils.CalculateAreaPerimeterAndOnBoundTrail(points);
            }
            else
            {
                _MinPoly.Visibility = Visibility.Hidden;
                APStats = null;
            }

            Map.Refresh();
        }


        public void AnalyzeTargetPolygon()
        {
            APStats = null;

            if (TargetPolygon != null)
            {
                UpdateOrigPoly();

                List<PMSegment> segments = new List<PMSegment>();

                int targetZone = Points[0].Metadata.Zone;

                TtPoint lastPoint = Points[Points.Count - 1];
                TtPoint currPoint = Points[0];
                TtPoint nextPoint;

                UTMCoords lastCoords = lastPoint.GetCoords(targetZone);
                UTMCoords currCoords = currPoint.GetCoords(targetZone);
                UTMCoords nextCoords;

                for (int i = 0; i < Points.Count; i++)
                {
                    nextPoint = (i == Points.Count - 1) ? Points[0] : Points[i + 1];
                    nextCoords = nextPoint.GetCoords(targetZone);

                    if (currPoint.HasSameAdjLocation(nextPoint) || (!AnalyzeAllPointsInPoly && !currPoint.OnBoundary))
                    {
                        currPoint.OnBoundary = false;
                        currPoint = nextPoint;
                        continue;
                    }
                    else
                    {
                        currPoint.OnBoundary = true;
                        segments.Add(new PMSegment(lastPoint, currPoint, nextPoint, lastCoords, currCoords, nextCoords, AccuracyOverride));
                    }


                    lastPoint = currPoint;
                    lastCoords = currCoords;

                    currPoint = nextPoint;
                    currCoords = nextCoords;
                }

                if (segments.Count < 3)
                {
                    return; //not enough
                }

                PMSegment lastKeptSeg = segments[0], currSeg = null;
                List<PMSegment> nonComplientSegs = new List<PMSegment>();

                double totalSegsAngle = 0, totalSegsLength = 0, avgSegsAcc = 0;

                Action commitSegment = () =>
                {
                    if (nonComplientSegs.Count > 0)
                    {
                        foreach (PMSegment seg in nonComplientSegs)
                        {
                            seg.CurrPoint.OnBoundary = false;
                        }

                        if (LimitAreaChange)
                            LimitArea(lastKeptSeg, currSeg, nonComplientSegs);

                        nonComplientSegs.Clear();
                        totalSegsAngle = totalSegsLength = 0;
                    }

                    lastKeptSeg = currSeg;
                };

                Action addNonCompSeg = () =>
                {
                    if (currSeg.AngleDir < 0)
                        totalSegsAngle -= currSeg.AngleRel;
                    else
                        totalSegsAngle += currSeg.AngleRel;

                    totalSegsLength += currSeg.Leg2Length;

                    avgSegsAcc = (nonComplientSegs.Count == 0) ?
                        currSeg.Leg2Accuracy :
                        (avgSegsAcc * (nonComplientSegs.Count - 1) / nonComplientSegs.Count + currSeg.Leg2Accuracy / nonComplientSegs.Count);


                    if (Math.Abs(totalSegsAngle) > MinimumAngle && totalSegsLength >= (MinimumLegLength ?? avgSegsAcc * MIN_DIST_MULTIPLIER))
                    {
                        commitSegment();
                    }
                    else
                    {
                        nonComplientSegs.Add(currSeg);
                    }
                };

                foreach (PMSegment seg in segments.Concat(new []{ segments[0] }))
                {
                    currSeg = seg;

                    if (currSeg.Angle >= MinimumAngle)
                    {
                        commitSegment();
                    }
                    else
                    {
                        addNonCompSeg();
                    }
                }

                commitSegment();
            }

            UpdateMinimizedPoly();
        }


        private void LimitArea(PMSegment startSeg, PMSegment endSeg, IEnumerable<PMSegment> segs, bool reverseDir = false)
        {
            double maxSegDist = 0;
            int segIdx = 0, maxSegIdx = 0, segsCount = segs.Count();
            PMSegment maxSeg = null;

            foreach (PMSegment currSeg in segs)
            {
                double distFromSESeg = MathEx.DistanceToLine(
                    currSeg.CurrCoords.X, currSeg.CurrCoords.Y,
                    startSeg.CurrCoords.X, startSeg.CurrCoords.Y,
                    endSeg.CurrCoords.X, endSeg.CurrCoords.Y);

                double maxAllowedDistFromSeg = MaximumLegAdjustDist ?? ((AccuracyOverride ?? currSeg.CurrPoint.Accuracy) * 2);

                if (distFromSESeg > maxAllowedDistFromSeg)
                {
                    if (segsCount == 1)
                    {
                        currSeg.CurrPoint.OnBoundary = true;
                        return;
                    }

                    if (distFromSESeg > maxSegDist)
                    {
                        maxSegDist = distFromSESeg;
                        maxSeg = currSeg;
                        maxSegIdx = segIdx;
                    }
                }

                segIdx++;
            }

            if (maxSegDist > 0)
            {
                maxSeg.CurrPoint.OnBoundary = true;

                if (maxSegIdx > 0)
                {
                    LimitArea(startSeg, maxSeg, segs.Take(maxSegIdx));
                }

                if (maxSegIdx < segsCount - 1)
                {
                    LimitArea(maxSeg, endSeg, segs.Skip(maxSegIdx + 1));
                }
            }
        }


        public void Apply()
        {
            if (APStats != null)
            {
                Project.HistoryManager.MinimizePoints(Points.Select(p => _OrigPoints[p.CN]), Points.Select(p => p.OnBoundary));
                _Dialog.Close();
            }
            else
            {
                MessageBox.Show("No Adjustments Made");
            }
        }

        public class PMSegment
        {
            public TtPoint LastPoint { get; }
            public TtPoint CurrPoint { get; }
            public TtPoint NextPoint { get; }

            public UTMCoords LastCoords { get; }
            public UTMCoords CurrCoords { get; }
            public UTMCoords NextCoords { get; }


            public double Angle { get; }
            public double AngleRel { get; }
            public double AngleDir { get; }

            public bool OnBoundary => CurrPoint.OnBoundary;


            public double Leg1Length { get; }
            public double Leg2Length { get; }
            public double SegmentLength { get; }


            public double Leg1Accuracy { get; }
            public double Leg2Accuracy { get; }

            private readonly double Leg1AccDist;
            private readonly double Leg2AccDist;

            public bool Leg1MeetsMinDist => Leg1AccDist > Leg1Accuracy * MIN_DIST_MULTIPLIER;
            public bool Leg2MeetsMinDist => Leg2AccDist > Leg2Accuracy * MIN_DIST_MULTIPLIER;


            public PMSegment(TtPoint lastPoint, TtPoint currPoint, TtPoint nextPoint,
                UTMCoords lastCoords, UTMCoords currCoords, UTMCoords nextCoords, double? accOverride = null)
            {
                LastPoint = lastPoint;
                CurrPoint = currPoint;
                NextPoint = nextPoint;

                LastCoords = lastCoords;
                CurrCoords = currCoords;
                NextCoords = nextCoords;

                Angle = MathEx.CalculateAngleBetweenPoints(lastCoords.X, lastCoords.Y, currCoords.X, currCoords.Y, nextCoords.X, nextCoords.Y);
                AngleRel = Angle > MAXIMUM_ANGLE ? (MAXIMUM_ANGLE - (Angle % MAXIMUM_ANGLE)) : Angle;
                AngleDir = MathEx.CalculateNextPointDir(lastCoords.X, lastCoords.Y, currCoords.X, currCoords.Y, nextCoords.X, nextCoords.Y);

                Leg1Length = MathEx.Distance(lastCoords.X, lastCoords.Y, currCoords.X, currCoords.Y);
                Leg2Length = MathEx.Distance(currCoords.X, currCoords.Y, nextCoords.X, nextCoords.Y);
                SegmentLength = Leg1Length + Leg2Length;

                Leg1Accuracy = accOverride ?? (lastPoint.Accuracy + currPoint.Accuracy) / 2d;
                Leg2Accuracy = accOverride ?? (currPoint.Accuracy + nextPoint.Accuracy) / 2d;

                Leg1AccDist = Leg1Accuracy * Leg1Length;
                Leg2AccDist = Leg2Accuracy * Leg2Length;
            }

            public override string ToString()
            {
                return $"{LastPoint.PID} -> ({CurrPoint.PID}) -> {NextPoint.PID} | Ang: {Angle:F1} | Dist: ({Leg1Length:F1} - {Leg2Length:F1})";
            }
        }

    }
}
