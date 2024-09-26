using FMSC.Core;
using FMSC.Core.ComponentModel;
using FMSC.Core.Utilities;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class RetraceModel : BaseModel
    {
        private TtProject _Project;
        private TtHistoryManager _Manager => _Project.HistoryManager;

        public ObservableCollection<Retrace> Retraces { get { return Get<ObservableCollection<Retrace>>(); } set { Set(value); } }


        public List<TtPolygon> Polygons { get; }
        public TtPolygon TargetPolygon { get { return Get<TtPolygon>(); } set { Set(value, () => PolygonChanged(value)); } }
        public int InsertIndex { get { return Get<int>(); } set { Set(value); } }
        public List<TtPoint> AfterPoints { get { return Get<List<TtPoint>>(); } set { Set(value); } }

        public QuondamBoundaryMode BoundaryMode { get { return Get<QuondamBoundaryMode>(); } set { Set(value); } }
        public bool InsertBeginning { get { return Get<bool>(); } set { Set(value); } }
        public bool InsertAfter { get { return Get<bool>(); } set { Set(value); } }

        public bool MovePoints { get { return Get<bool>(); } set { Set(value, () => OnPropertyChanged(nameof(TargetPolygonToolTip))); } }
        public string TargetPolygonToolTip => MovePoints ? "The polygon in which to move the points to." : "The polygon in which to place the Quondams.";


        public RetraceModel(TtProject project)
        {
            _Project = project;
                
            AfterPoints = new List<TtPoint>();
            Polygons = _Project.GetSortedPolygons();

            if (_Project.ProjectSettings.LastRetrace != null)
            {
                Retraces = new ObservableCollection<Retrace>(_Project.ProjectSettings.LastRetrace);
                TargetPolygon = _Project.ProjectSettings.LastRetraceTargetPolygon;
            }
            else
            {
                Retraces = new ObservableCollection<Retrace>
                {
                    new Retrace(_Project)
                };
            }
        }


        private void PolygonChanged(TtPolygon polygon)
        {
            if (polygon != null)
                AfterPoints = _Manager.GetPoints(polygon.CN);
        }

        public void AddRetrace(Retrace sender)
        {
            Retrace retrace = new Retrace(_Project);
            int index = -1;

            if (sender != null)
                index = Retraces.IndexOf(sender);

            if (index < 0)
                Retraces.Add(retrace);
            else
                Retraces.Insert(index + 1, retrace);
        }

        public void DeleteRetrace(Retrace sender)
        {
            if (Retraces.Count > 1 && sender != null)
                Retraces.Remove(sender);
        }

        public void ClearRetraces()
        {
            if (MessageBox.Show("Are you sure you would like to clear all retraces?", "Clear Retraces",
                MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                Retraces.Clear();
        }

        public bool RetracePoints()
        {
            if (Retraces.Count > 0 && TargetPolygon != null)
            {
                if (Retraces.Any(r => !r.IsValid))
                {
                    MessageBox.Show("One or more retraces are invalid.");
                }
                else
                {
                    bool overrideAccs = false;
                    if (MovePoints && Retraces.Any(r => r.Points.Any(p => p.Polygon.Accuracy != TargetPolygon.Accuracy)))
                    {
                        var mbr = MessageBox.Show($"The Target Polygon '{TargetPolygon.Name}' has an accuracy that is different than some" +
                            "of the polygons that the points are in. Would you like to apply a manual accuracy to the points to keep their " +
                            "original polygon accuracy? This will not override points that already have a manual accuracy set.",
                            "Polygon Accuracy Mismatch", MessageBoxButton.YesNoCancel, MessageBoxImage.Hand, MessageBoxResult.No);

                        if (mbr == MessageBoxResult.Yes)
                            overrideAccs = true;
                        else if (mbr == MessageBoxResult.Cancel)
                            return false;
                    }

                    List<TtPoint> retracePoints = Retraces.SelectMany(r => r.RetracePoints).ToList();
                    
                    if (MovePoints)
                    {
                        bool mcStarted = false;
                        if (overrideAccs)
                        {
                            TtPoint[] gpsPoints = retracePoints.Where(p => p is GpsPoint gps && gps.ManualAccuracy == null).ToArray();

                            if (gpsPoints.Length > 0)
                            {
                                _Manager.StartMultiCommand();
                                mcStarted = true;

                                _Manager.EditPointsMultiValues(gpsPoints, PointProperties.MAN_ACC_GPS, gpsPoints.Select(p => (double?)p.Polygon.Accuracy));
                            }
                        }

                        _Manager.MovePointsToPolygon(retracePoints, TargetPolygon,
                            InsertBeginning ? 0 : InsertAfter ? InsertIndex : int.MaxValue);

                        if (mcStarted)
                        {
                            _Manager.CommitMultiCommand(DataActionType.MovedPoints);
                        }
                    }
                    else
                    {
                        _Manager.CreateRetrace(retracePoints, TargetPolygon,
                            InsertBeginning ? 0 : InsertAfter ? InsertIndex : int.MaxValue, BoundaryMode);
                    }


                    _Project.ProjectSettings.LastRetraceTargetPolygon = TargetPolygon;
                    _Project.ProjectSettings.LastRetrace = Retraces.ToList();
                    return true;
                } 
            }

            return false;
        }
    }


    public class Retrace : BaseModel
    {
        public string GCN { get; } = Guid.NewGuid().ToString();

        private TtProject _Project;
        private TtHistoryManager _Manager => _Project.HistoryManager;

        public List<TtPolygon> Polygons { get; }
        
        public List<TtPoint> Points { get { return Get<List<TtPoint>>(); } set { Set(value, UpdateRetraceDelay); } }

        public TtPoint PointFrom { get { return Get<TtPoint>(); } set { Set(value, UpdateRetraceDelay); } }
        public TtPoint PointTo { get { return Get<TtPoint>(); } set { Set(value, UpdateRetraceDelay); } }

        public bool DirFwd { get { return Get<bool>(); } set { Set(value, UpdateRetraceDelay); } }
        public bool DirRev { get { return Get<bool>(); } set { Set(value, UpdateRetraceDelay); } }
        public bool SinglePoint { get { return Get<bool>(); } set { Set(value, UpdateRetraceDelay); } }

        public bool IsValid { get { return PointFrom != null && PointTo != null || SinglePoint; } }

        
        public TtPolygon SelectedPolygon { get { return Get<TtPolygon>(); } set { Set(value, () => PolygonChanged(value)); } }



        private readonly DelayActionHandler updateRetraceDAH;

        public ReadOnlyCollection<TtPoint> RetracePoints
        {
            get { return Get<ReadOnlyCollection<TtPoint>>(); }
            set { Set(value, () => OnPropertyChanged(nameof(NumberOfPoints), nameof(PreviewText))); }
        }

        public int NumberOfPoints => RetracePoints != null ? RetracePoints.Count : 0;

        public string PreviewText
        {
            get
            {
                if (RetracePoints == null || RetracePoints.Count == 0) return "No Points";

                Func<int, int> pid = (i) => RetracePoints[i].PID;

                if (RetracePoints.Count > 4)
                {
                    return $"{pid(0)}, {pid(1)} ... {pid(RetracePoints.Count - 2)}, {pid(RetracePoints.Count - 1)}";
                }
                else
                {
                    return RetracePoints.ToStringContents(p => p.PID.ToString(), ", ");
                }
            }
        }


        public Retrace(TtProject project)
        {
            _Project = project;
            updateRetraceDAH = new DelayActionHandler(UpdateRetrace, 100);

            PointFrom = PointTo = null;
            Polygons = _Project.GetSortedPolygons();
            DirFwd = true;
        }


        private void PolygonChanged(TtPolygon polygon)
        {
            Points = polygon == null ? new List<TtPoint>() : _Manager.GetPoints(polygon.CN);
        }


        private void UpdateRetraceDelay()
        {
            updateRetraceDAH.DelayInvoke();
        }

        private void UpdateRetrace()
        {
            RetracePoints = null;

            if (PointFrom == null) return;

            List<TtPoint> retracePoints = new List<TtPoint>();

            if (SinglePoint)
            {
                retracePoints.Add(PointFrom);
            }
            else
            {
                if (PointFrom == null || PointTo == null) return;

                List<TtPoint> points = Points;

                int sindex = points.IndexOf(PointFrom);
                int eindex = points.IndexOf(PointTo);

                if (DirFwd)
                {
                    if (sindex < eindex)
                    {
                        for (int i = sindex; i <= eindex; i++)
                            retracePoints.Add(points[i]);
                    }
                    else
                    {
                        for (int i = sindex; i < points.Count; i++)
                            retracePoints.Add(points[i]);

                        for (int i = 0; i <= eindex; i++)
                            retracePoints.Add(points[i]);
                    }
                }
                else if (DirRev)
                {
                    if (sindex < eindex)
                    {
                        for (int i = eindex; i >= sindex; i--)
                            retracePoints.Add(points[i]);
                    }
                    else
                    {
                        for (int i = eindex; i > -1; i--)
                            retracePoints.Add(points[i]);

                        for (int i = points.Count - 1; i >= sindex; i--)
                            retracePoints.Add(points[i]);
                    }
                }
            }

            RetracePoints = new ReadOnlyCollection<TtPoint>(retracePoints);
        }

        public void SwapPointFromTo()
        {
            TtPoint from = PointFrom;
            PointFrom = PointTo;
            PointTo = from;
        }
    }
}
