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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TwoTrails.Core;
using TwoTrails.Core.Points;
using TwoTrails.Mapping;

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
        public ICommand CommitCommand { get; }



        private TtProject Project;
        private TtHistoryManager Manager => Project.HistoryManager;


        public Map Map { get; } = new Map();

        public List<TtPolygon> Polygons { get; }
        public ReadOnlyObservableCollection<TtPoint> Points { get; private set; }
        private List<TtPoint> _OrigPoints;
        private readonly MapPolygon _OrigPoly = new MapPolygon();
        private readonly MapPolygon _MinPoly = new MapPolygon();
        private Extent Extents;


        public TtPolygon TargetPolygon {
            get => Get<TtPolygon>();
            set => Set(value, () => {
                OnPropertyChanged(nameof(TargetPolygonToolTip));
                UpdateOrigPoly();
                AnalyzeTargetPolygon();
            });
        }

        public string TargetPolygonToolTip => TargetPolygon?.ToString();


        public double MinimumAngle { get => Get<double>(); set => Set(value); }
        public double MinimumLegLength { get => Get<double>(); set => Set(value); }
        public double? AccuracyOverride { get => Get<double?>(); set => Set(value); }

        public bool RunPartial { get => Get<bool>(); set => Set(value); }
        public bool AnalyzeAllPointsInPoly { get => Get<bool>(); set => Set(value); }



        private Tuple<double, double, double> APStats {
            get => Get<Tuple<double, double, double>>();
            set => Set(value, () =>
                    OnPropertyChanged(
                        nameof(NewArea),
                        nameof(NewPerimeter),
                        nameof(AreaDifference),
                        nameof(PerimeterDifference)
                    )
                );
        }

        public double? NewArea => APStats?.Item1;
        public double? NewPerimeter => APStats?.Item2;

        public double? AreaDifference => APStats != null ? (double?)(APStats.Item1 / TargetPolygon.Area) : null;
        public double? PerimeterDifference => APStats != null ? (double?)(APStats.Item2 / TargetPolygon.Perimeter) : null;



        public PointMinimizationModel(TtProject project)
        {
            Project = project;
            Polygons = Manager.Polygons.Where(p => Manager.GetPoints(p.CN).HasAtLeast(3)).ToList();
            Polygons.Sort(new PolygonSorterDirect(project.Settings.SortPolysByName));

            PointSelectionChangedCommand = new RelayCommand(x => SelectedPointsChanged(x as CompositeCommandParameter));

            MinimumAngle = MINIMUM_ANGLE;
            AnalyzeAllPointsInPoly = true;


            _OrigPoly.Stroke = new SolidColorBrush(Colors.Gray);
            _OrigPoly.StrokeThickness = 5;

            _MinPoly.Stroke = new SolidColorBrush(Colors.Red);
            _MinPoly.StrokeThickness = 2;

            TargetPolygon = Polygons.FirstOrDefault();

            Map.CredentialsProvider = new ApplicationIdCredentialsProvider(APIKeys.BING_MAPS_API_KEY);
            Map.Mode = new AerialMode();
            Map.Loaded += OnMapLoaded;
        }


        private void OnMapLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Map.ActualHeight > 0)
            {
                //IEnumerable<Location> locs = MapManager.PolygonManagers.SelectMany(mpm => mpm.Points.Select(p => p.AdjLocation));
                //if (locs.Any())
                //    map.SetView(locs, new Thickness(30), 0);


                Map.Children.Add(_OrigPoly);
                Map.Children.Add(_MinPoly);

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


        private void SelectedPointsChanged(CompositeCommandParameter parameters)
        {
            SelectionChangedEventArgs args = parameters.EventArgs as SelectionChangedEventArgs;

            foreach (TtPoint point in args.AddedItems)
            {
                point.OnBoundary = true;
            }

            foreach (TtPoint point in args.RemovedItems)
            {
                point.OnBoundary = false;
            }

            UpdateMinimizedPoly();
        }


        public void UpdateOrigPoly()
        {
            if (_OrigPoints != null && _OrigPoints.Count > 2)
            {
                LocationCollection locations = new LocationCollection();
                IEnumerable<TtPoint> points = AnalyzeAllPointsInPoly ? _OrigPoints : _OrigPoints.Where(p => p.IsBndPoint());

                Extent.Builder eb = new Extent.Builder();

                foreach (TtPoint point in points.OrderBy(p => p.Index))
                {
                    Point ll = point.GetLatLon();
                    eb.Include(ll.Y, ll.X);
                    locations.Add(new Location(ll.Y, ll.X));
                }

                _OrigPoly.Locations = locations;
                _OrigPoly.Visibility = System.Windows.Visibility.Visible;

                Extents = eb.Build();
            }
            else
            {
                _OrigPoly.Visibility = System.Windows.Visibility.Hidden;
            }

            Map.Refresh();
        }

        public void UpdateMinimizedPoly()
        {
            if (Points != null && Points.Count > 2)
            {
                List<TtPoint> points = Points.Where(p => p.IsBndPoint()).OrderBy(p => p.Index).ToList();
                LocationCollection locations = new LocationCollection();
                foreach (TtPoint point in points)
                {
                    Point ll = point.GetLatLon();
                    locations.Add(new Location(ll.Y, ll.X));
                }

                _MinPoly.Locations = locations;
                _MinPoly.Visibility = System.Windows.Visibility.Visible;

                APStats = TtCoreUtils.CalculateAreaPerimeterAndOnBoundTrail(points);
            }
            else
            {
                _MinPoly.Visibility = System.Windows.Visibility.Hidden;
                APStats = null;
            }

            Map.Refresh();
        }


        public void AnalyzeTargetPolygon()
        {
            APStats = null;

            if (TargetPolygon != null)
            {
                _OrigPoints = Manager.GetPoints(TargetPolygon.CN);
                UpdateOrigPoly();

                List<TtPoint> points = new List<TtPoint>(_OrigPoints.DeepCopy());

                Points = new ReadOnlyObservableCollection<TtPoint>(
                    new ObservableCollection<TtPoint>(points));

                List<PMSegment> segments = new List<PMSegment>();

                int targetZone = Points[0].Metadata.Zone;

                TtPoint lastPoint = Points[Points.Count - 1];
                TtPoint currPoint = Points[0];
                TtPoint nextPoint;

                UTMCoords lastCoords = lastPoint.GetCoords(targetZone);
                UTMCoords currCoords = currPoint.GetCoords(targetZone);
                UTMCoords nextCoords;

                for (int i = 0; i < points.Count; i++)
                {
                    nextPoint = (i == points.Count - 1) ? points[0] : points[i + 1];
                    nextCoords = nextPoint.GetCoords(targetZone);

                    if (currPoint.HasSameAdjLocation(nextPoint) || (AnalyzeAllPointsInPoly && !currPoint.OnBoundary))
                    {
                        currPoint.OnBoundary = false;
                        currPoint = nextPoint;
                        continue;
                    }
                    else
                    {
                        currPoint.OnBoundary = true;
                    }

                    segments.Add(new PMSegment(lastPoint, currPoint, nextPoint, lastCoords, currCoords, nextCoords, AccuracyOverride));

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
                    lastKeptSeg = currSeg;

                    if (nonComplientSegs.Count > 0)
                    {
                        foreach (PMSegment seg in nonComplientSegs)
                        {
                            seg.OnBoundary = false;
                        }

                        nonComplientSegs.Clear();
                        totalSegsAngle = totalSegsLength = 0;
                    }
                };

                Action addNonCompSeg = () =>
                {
                    if (currSeg.AngleDir < 0)
                        totalSegsAngle -= currSeg.Angle;
                    else
                        totalSegsAngle += currSeg.Angle;

                    totalSegsLength += currSeg.Leg2Length;

                    avgSegsAcc = (nonComplientSegs.Count == 0) ?
                        currSeg.Leg2Accuracy :
                        (avgSegsAcc * (nonComplientSegs.Count - 1) / nonComplientSegs.Count + currSeg.Leg2Accuracy / nonComplientSegs.Count);

                    if (Math.Abs(totalSegsAngle) > MinimumAngle && totalSegsLength >= avgSegsAcc * MIN_DIST_MULTIPLIER)
                    {
                        commitSegment();
                    }
                };

                for (int i = 1; i < segments.Count; i++)
                {
                    currSeg = segments[i];

                    if (currSeg.Angle >= MinimumAngle && currSeg.Leg1MeetsMinDist && currSeg.Leg2MeetsMinDist)
                    {
                        commitSegment();
                    }
                    else
                    {
                        addNonCompSeg();
                    }
                }

                commitSegment();

                UpdateMinimizedPoly();
            }
        }

        public bool Apply()
        {
            if (APStats != null)
            {

            }
            else
            {
                //nothing to apply
            }


            return true;
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
            public double AngleDir { get; }

            public bool OnBoundary { get; set; }
            public bool Ignore { get; set; }


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
                AngleDir = MathEx.CalculateNextPointDir(lastCoords.X, lastCoords.Y, currCoords.X, currCoords.Y, nextCoords.X, nextCoords.Y);

                Leg1Length = MathEx.Distance(lastCoords.X, lastCoords.Y, currCoords.X, currCoords.Y);
                Leg2Length = MathEx.Distance(currCoords.X, currCoords.Y, nextCoords.X, nextCoords.Y);
                SegmentLength = Leg1Length + Leg2Length;

                Leg1Accuracy = accOverride ?? (lastPoint.Accuracy + currPoint.Accuracy) / 2d;
                Leg2Accuracy = accOverride ?? (currPoint.Accuracy + nextPoint.Accuracy) / 2d;

                Leg1AccDist = Leg1Accuracy * Leg1Length;
                Leg2AccDist = Leg2Accuracy * Leg2Length;
            }
        }

    }
}
