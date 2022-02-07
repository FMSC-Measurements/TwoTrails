using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.ComponentModel.History;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class RetraceModel : BaseModel
    {
        private TtHistoryManager _Manager;

        public ObservableCollection<Retrace> Retraces { get { return Get<ObservableCollection<Retrace>>(); } set { Set(value); } }

        public ICommand CommitCommand { get; }

        public List<TtPolygon> Polygons { get; }
        public TtPolygon TargetPolygon { get { return Get<TtPolygon>(); } set { Set(value, () => PolygonChanged(value)); } }
        public int InsertIndex { get { return Get<int>(); } set { Set(value); } }
        public List<TtPoint> AfterPoints { get { return Get<List<TtPoint>>(); } set { Set(value); } }

        public QuondamBoundaryMode BoundaryMode { get { return Get<QuondamBoundaryMode>(); } set { Set(value); } }
        public bool InsertBeginning { get { return Get<bool>(); } set { Set(value); } }
        public bool InsertAfter { get { return Get<bool>(); } set { Set(value); } }

        public bool MovePoints { get { return Get<bool>(); } set { Set(value, () => OnPropertyChanged(nameof(TargetPolygonToolTip))); } }
        public string TargetPolygonToolTip => MovePoints ? "The polygon in which to move the points to." : "The polygon in which to place the Quondams.";


        public RetraceModel(TtHistoryManager manager)
        {
            _Manager = manager;

            AfterPoints = new List<TtPoint>();
            Polygons = manager.GetPolygons();

            Retraces = new ObservableCollection<Retrace>();
            Retraces.Add(new Retrace(this, _Manager));

            CommitCommand = new RelayCommand(x => RetracePoints());
        }


        private void PolygonChanged(TtPolygon polygon)
        {
            if (polygon != null)
                AfterPoints = _Manager.GetPoints(polygon.CN);
        }

        public void AddRetrace(Retrace sender)
        {
            Retrace retrace = new Retrace(this, _Manager);
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

        public bool RetracePoints()
        {
            if (Retraces.Count > 0)
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

                    List<TtPoint> retracePoints = new List<TtPoint>();

                    foreach (Retrace r in Retraces)
                    {
                        if (r.SinglePoint)
                        {
                            retracePoints.Add(r.PointFrom);
                        }
                        else
                        {
                            List<TtPoint> points = r.Points;

                            int sindex = points.IndexOf(r.PointFrom);
                            int eindex = points.IndexOf(r.PointTo);

                            if (r.DirInc)
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
                            else
                            {
                                if (sindex > eindex)
                                {
                                    for (int i = sindex; i >= eindex; i--)
                                        retracePoints.Add(points[i]);
                                }
                                else
                                {
                                    for (int i = sindex; i > -1; i--)
                                        retracePoints.Add(points[i]);

                                    for (int i = points.Count - 1; i >= eindex; i--)
                                        retracePoints.Add(points[i]);
                                }
                            }
                        }
                    }
                    
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
                            _Manager.CommitMultiCommand();
                        }
                    }
                    else
                    {
                        _Manager.CreateRetrace(retracePoints, TargetPolygon,
                            InsertBeginning ? 0 : InsertAfter ? InsertIndex : int.MaxValue, BoundaryMode);
                    }

                    return true;
                } 
            }

            return false;
        }
    }

    public class Retrace : BaseModel
    {
        public string GCN { get; } = Guid.NewGuid().ToString();

        private ITtManager _Manager;

        public List<TtPolygon> Polygons { get; }
        
        public List<TtPoint> Points { get { return Get<List<TtPoint>>(); } set { Set(value); } }

        public TtPoint PointFrom { get { return Get<TtPoint>(); } set { Set(value); } }
        public TtPoint PointTo { get { return Get<TtPoint>(); } set { Set(value); } }

        public bool DirInc { get { return Get<bool>(); } set { Set(value); } }
        public bool SinglePoint { get { return Get<bool>(); } set { Set(value); } }

        public bool IsValid { get { return PointFrom != null && PointTo != null || SinglePoint ; } }

        
        public TtPolygon SelectedPolygon { get { return Get<TtPolygon>(); } set { Set(value, () => PolygonChanged(value)); } }
        

        public Retrace(RetraceModel model, ITtManager manager)
        {
            _Manager = manager;
            PointFrom = PointTo = null;
            Polygons = manager.GetPolygons();
            DirInc = true;
        }

        private void PolygonChanged(TtPolygon polygon)
        {
            Points = polygon == null ? new List<TtPoint>() : _Manager.GetPoints(polygon.CN);
        }
    }
}
