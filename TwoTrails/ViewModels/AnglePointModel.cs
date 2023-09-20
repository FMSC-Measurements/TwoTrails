using FMSC.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.ViewModels
{
    public class AnglePointModel : BaseModel
    {
        private TtHistoryManager _Manager;


        public ICommand CommitCommand { get; }


        public List<TtPolygon> Polygons { get; }


        public TtPolygon TargetPolygon { get { return Get<TtPolygon>(); } set { Set(value, () => Reset()); } }

        public bool AnalyzeAllPointsInPoly { get; set; }


        private List<TtPoint> _OnBoundPoints = new List<TtPoint>();
        private List<TtPoint> _OffBoundPoints = new List<TtPoint>();

        private Tuple<double, double, double> _APStats {
            get { return Get<Tuple<double, double, double>>(); }
            set {
                Set(value, () =>
                    OnPropertyChanged(
                        nameof(NewArea),
                        nameof(NewPerimeter),
                        nameof(AreaDifference),
                        nameof(PerimeterDifference)
                    )
                );
            }
        }

        public double? NewArea => _APStats?.Item1;
        public double? NewPerimeter => _APStats?.Item2;

        public double? AreaDifference => _APStats != null ? (double?)(_APStats.Item1 / TargetPolygon.Area) : null;
        public double? PerimeterDifference => _APStats != null ? (double?)(_APStats.Item2 / TargetPolygon.Perimeter) : null;



        public AnglePointModel(TtHistoryManager manager)
        {
            _Manager = manager;


        }


        public void Reset()
        {
            _APStats = null;
            _OnBoundPoints.Clear();
            _OffBoundPoints.Clear();
        }


        public void AnalyzePolygon()
        {
            if (TargetPolygon != null)
            { 
                List<TtPoint> points = _Manager.GetPoints(TargetPolygon.CN);

                if (!AnalyzeAllPointsInPoly) points = points.Where(p => p.IsBndPoint()).ToList();

                if (points.Count < 3)
                {
                    //not enough points
                }


            }

            _APStats = null;
        }

        public void ApplyAnglePoint()
        {
            if (_APStats != null)
            {

            }
            else
            {
                //nothing to apply
            }
        }


    }
}
