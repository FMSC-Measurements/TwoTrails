using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Controls
{
    public class DataEditorModel : NotifyPropertyChangedEx
    {
        private ICollectionView _Points;
        public ICollectionView Points
        {
            get { return _Points; }
            set { SetField(ref _Points, value); }
        }

        ITtManager _Manager;


        public bool IsAdvancedMode { get { return Get<bool>(); } set { Set(value); } }

        public bool MultipleSelections
        {
            get { return _SelectedPoints != null && _SelectedPoints.Count > 0; }
        }

        public TtPoint SelectedPoint
        {
            get { return Get<TtPoint>(); }
            set
            {
                if (Set(value))
                {
                    OnPropertyChanged(
                        nameof(PID),
                        nameof(MultipleSelections)
                    );
                }
            }
        }

        private List<TtPoint> _SelectedPoints;
        public List<TtPoint> SelectedPoints
        {
            get { return _SelectedPoints; }
            set
            {
                _SelectedPoints = value;

                OnPropertyChanged(
                    nameof(SelectedPoints),
                    nameof(MultipleSelections)
                );
            }
        }

        public string PID
        {
            get { return Get<string>(); }
            set
            {

            }
        }








        public DataEditorModel(TtProject project)
        {
            _Manager = project.Manager;
            List<TtPoint> points = _Manager.GetPoints();
            points.Sort();
            Points = CollectionViewSource.GetDefaultView(points);

            SelectedPoints = points;
            //Points.Filter = Filter;


        }
        

        private bool Filter(object obj)
        {
            return true;
        }

    }
}
