using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class RetraceModel : NotifyPropertyChangedEx
    {
        private TtHistoryManager _Manager;

        public ObservableCollection<Retrace> Retraces { get { return Get<ObservableCollection<Retrace>>(); } set { Set(value); } }

        ICommand CommitCommand { get; }

        public List<TtPolygon> Polygons { get; }
        public TtPolygon TargetPolygon { get { return Get<TtPolygon>(); } set { Set(value, () => PolygonChanged(value)); } }
        public int InsertIndex { get { return Get<int>(); } set { Set(value); } }
        public List<TtPoint> AfterPoints { get { return Get<List<TtPoint>>(); } set { Set(value); } }

        public bool OnBoundary { get { return Get<bool>(); } set { Set(value); } }
        public bool OffBoundary { get { return Get<bool>(); } set { Set(value); } }

        public bool InsertBegining { get { return Get<bool>(); } set { Set(value); } }
        public bool InsertAfter { get { return Get<bool>(); } set { Set(value); } }

        public RetraceModel(TtHistoryManager manager)
        {
            _Manager = manager;

            Polygons = manager.GetPolyons();

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

                    _Manager.CreateQuondamLinks(retracePoints, TargetPolygon, 
                        InsertBegining ? 0 : InsertAfter ? InsertIndex : int.MaxValue, 
                        OnBoundary || OffBoundary ? (bool?)OnBoundary : null);

                    return true;
                } 
            }

            return false;
        }
    }

    public class Retrace : NotifyPropertyChangedEx
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
            Polygons = manager.GetPolyons();
            DirInc = true;
        }

        private void PolygonChanged(TtPolygon polygon)
        {
            if (polygon != null)
                Points = _Manager.GetPoints(polygon.CN);
        }
    }
}
