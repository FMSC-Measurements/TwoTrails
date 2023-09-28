using FMSC.Core;
using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using FMSC.GeoSpatial.UTM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class PointMinimizationModel : BaseModel
    {
        public const double MINIMUM_ANGLE = 45d;
        public const double MAXIMUM_ANGLE = 90d;
        public const double MIN_DIST_MULTIPLIER = 5d;
        public const double MAX_DIST_MULTIPLIER = 10d;


        public ICommand PointSelectionChangedCommand { get; }


        private TtProject Project;
        private TtHistoryManager Manager => Project.HistoryManager;


        public ICommand CommitCommand { get; }


        public List<TtPolygon> Polygons { get; }

        public ReadOnlyObservableCollection<TtPoint> Points { get; private set; }


        public TtPolygon TargetPolygon {
            get => Get<TtPolygon>();
            set => Set(value, () => {
                Reset();
                OnPropertyChanged(nameof(TargetPolygonToolTip));
                AnalyzeTargetPolygon();
            });
        }

        public string TargetPolygonToolTip => TargetPolygon?.ToString();


        public double MinimumAngle { get => Get<double>(); set => Set(value); }
        public double MinimumLegLength { get => Get<double>(); set => Set(value); }
        public double? AccuracyOverride { get => Get<double?>(); set => Set(value); }

        public bool RunPartial { get => Get<bool>(); set => Set(value); }
        public bool AnalyzeAllPointsInPoly { get => Get<bool>(); set => Set(value); }



        private Tuple<double, double, double> _APStats {
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

        public double? NewArea => _APStats?.Item1;
        public double? NewPerimeter => _APStats?.Item2;

        public double? AreaDifference => _APStats != null ? (double?)(_APStats.Item1 / TargetPolygon.Area) : null;
        public double? PerimeterDifference => _APStats != null ? (double?)(_APStats.Item2 / TargetPolygon.Perimeter) : null;



        public PointMinimizationModel(TtProject project)
        {
            Project = project;
            Polygons = Manager.Polygons.Where(p => Manager.GetPoints(p.CN).HasAtLeast(3)).ToList();
            Polygons.Sort(new PolygonSorterDirect(project.Settings.SortPolysByName));

            PointSelectionChangedCommand = new RelayCommand(x => SelectedPointsChanged(x as CompositeCommandParameter));

            MinimumAngle = MINIMUM_ANGLE;
            AnalyzeAllPointsInPoly = true;

            TargetPolygon = Polygons.FirstOrDefault();
        }


        public void Reset()
        {
            _APStats = null;
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
        }

        public void UpdateMinimizedPoly()
        {
            _APStats = TtCoreUtils.CalculateAreaPerimeterAndOnBoundTrail(Points.Where(p => p.IsBndPoint()).ToList());
        }


        public void AnalyzeTargetPolygon()
        {
            if (TargetPolygon != null)
            {
                List<TtPoint> points = new List<TtPoint>(Manager.GetPoints(TargetPolygon.CN).DeepCopy());

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

            _APStats = null;
        }

        public bool Apply()
        {
            if (_APStats != null)
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
