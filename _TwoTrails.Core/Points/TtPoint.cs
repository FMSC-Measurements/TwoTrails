using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TwoTrails.Core.Points
{
    public delegate void PointChangedEvent(TtPoint point);
    public delegate void PointPolygonChangedEvent(TtPoint point, TtPolygon newPolygon, TtPolygon oldPolygon);
    public delegate void PointIndexChangedEvent(TtPoint point, int newIndex, int oldIndex);

    public abstract class TtPoint : TtObject, IAccuracy, IComparable<TtPoint>, IComparer<TtPoint>
    {
        public event PointChangedEvent LocationChanged;
        public event PointChangedEvent PreviewLocationChanged;
        public event PointChangedEvent OnBoundaryChanged;
        public event PointPolygonChangedEvent PointPolygonChanged;
        public event PointIndexChangedEvent PointIndexChanged;

        #region Properties
        private Int32 _Index;
        public Int32 Index
        {
            get { return _Index; }
            set { int oldIndex = _Index; SetField(ref _Index, value, () => PointIndexChanged?.Invoke(this, value, oldIndex)); }
        }

        private Int32 _PID;
        public Int32 PID
        {
            get { return _PID; }
            set { SetField(ref _PID, value); }
        }

        private DateTime _TimeCreated = DateTime.Now;
        public DateTime TimeCreated
        {
            get { return _TimeCreated; }
            set { SetField(ref _TimeCreated, value); }
        }
        
        private String _PolyCN;
        public String PolygonCN
        {
            get { return _PolyCN; }
            set { SetField(ref _PolyCN, value); }
        }

        protected TtPolygon _Polygon;
        public TtPolygon Polygon
        {
            get { return _Polygon; }
            set
            {
                TtPolygon oldPoly = _Polygon;
                if (SetField(ref _Polygon, value))
                {
                    if (oldPoly != null)
                    {
                        oldPoly.PreviewPolygonAccuracyChanged -= Polygon_PreviewPolygonAccuracyChanged;
                    }

                    if (value != null)
                    {
                        PolygonCN = value.CN;

                        _Polygon.PreviewPolygonAccuracyChanged += Polygon_PreviewPolygonAccuracyChanged; 
                    }

                    if (oldPoly != null && value != null)
                    {
                        PointPolygonChanged.Invoke(this, value, oldPoly);
                    }
                }
            }
        }

        private String _GroupCN;
        public String GroupCN
        {
            get { return _GroupCN; }
            set { SetField(ref _GroupCN, value); }
        }

        protected TtGroup _Group;
        public TtGroup Group
        {
            get { return _Group; }
            set
            {
                if (SetField(ref _Group, value))
                {
                    GroupCN = value?.CN;
                }
            }
        }
        
        private String _MetadataCN;
        public String MetadataCN
        {
            get { return _MetadataCN; }
            set { SetField(ref _MetadataCN, value); }
        }

        protected TtMetadata _Metadata;
        public virtual TtMetadata Metadata
        {
            get { return _Metadata; }
            set
            {
                if (SetField(ref _Metadata, value))
                {
                    MetadataCN = value?.CN;
                    OnMetadataChanged();
                }
            }
        }

        private String _Comment = String.Empty;
        public String Comment
        {
            get { return _Comment; }
            set { SetField(ref _Comment, value); }
        }

        private bool _OnBoundary;
        public bool OnBoundary
        {
            get { return _OnBoundary; }
            set { SetField(ref _OnBoundary, value, () => OnBoundaryChanged?.Invoke(this)); }
        }


        protected Double _AdjX;
        public virtual Double AdjX
        {
            get { return _AdjX; }
            protected set { SetField(ref _AdjX, value, OnLocationChanged); }
        }

        protected Double _AdjY;
        public virtual Double AdjY
        {
            get { return _AdjY; }
            protected set { SetField(ref _AdjY, value, OnLocationChanged); }
        }

        protected Double _AdjZ;
        public virtual Double AdjZ
        {
            get { return _AdjZ; }
            protected set { SetField(ref _AdjZ, value, OnLocationChanged); }
        }


        protected Double _UnAdjX;
        public virtual Double UnAdjX
        {
            get { return _UnAdjX; }
            set { SetField(ref _UnAdjX, value); }
        }

        protected Double _UnAdjY;
        public virtual Double UnAdjY
        {
            get { return _UnAdjY; }
            set { SetField(ref _UnAdjY, value); }
        }

        protected Double _UnAdjZ;
        public virtual Double UnAdjZ
        {
            get { return _UnAdjZ; }
            set { SetField(ref _UnAdjZ, value); }
        }


        private Double _Accuracy;
        public Double Accuracy
        {
            get { return _Accuracy; }
            protected set { SetField(ref _Accuracy, value); }
        }

        private ObservableCollection<String> _LinkedPoints = new ObservableCollection<string>();
        public ReadOnlyCollection<String> LinkedPoints => new ReadOnlyCollection<string>(_LinkedPoints);

        internal void ClearLinkedPoints()
        {
            _LinkedPoints.Clear();
        }

        public abstract OpType OpType { get; }


        public bool HasQuondamLinks { get { return _LinkedPoints.Count > 0; } }

        private bool _LocationChangedEventEnabled = true;
        public virtual bool LocationChangedEventEnabled
        {
            get { return _LocationChangedEventEnabled; }
            set
            {
                _LocationChangedEventEnabled = true;

                if (LocationHasChanged)
                    OnLocationChanged();
            }
        }

        public bool LocationHasChanged { get; protected set; }
        #endregion
        

        public TtPoint()
        {
            _LinkedPoints.CollectionChanged += LinkedPoints_CollectionChanged;
        }

        public TtPoint(TtPoint point) : base(point.CN)
        {
            _LinkedPoints.CollectionChanged += LinkedPoints_CollectionChanged;

            _Index = point._Index;
            _PID = point._PID;
            _TimeCreated = point._TimeCreated;

            if (point._Polygon != null)
                Polygon = point._Polygon;
            else
                _PolyCN = point._PolyCN;

            if (point._Group != null)
                Group = point._Group;
            else
                _GroupCN = point._GroupCN;

            if (point._Metadata != null)
                Metadata = point._Metadata;
            else
                _MetadataCN = point._MetadataCN;

            _Comment = point._Comment;
            _OnBoundary = point._OnBoundary;

            _AdjX = point._AdjX;
            _AdjY = point._AdjY;
            _AdjZ = point._AdjZ;

            _UnAdjX = point._UnAdjX;
            _UnAdjY = point._UnAdjY;
            _UnAdjZ = point._UnAdjZ;

            _Accuracy = point._Accuracy;
        }

        public TtPoint(string cn, int index, int pid, DateTime time, string polycn, string metacn, string groupcn,
            string comment, bool onbnd, double adjx, double adjy, double adjz, double unadjx, double unadjy, double unadjz,
            double acc, string qlinks) : base(cn)
        {
            _LinkedPoints.CollectionChanged += LinkedPoints_CollectionChanged;

            _Index = index;
            _PID = pid;
            _TimeCreated = time;

            _PolyCN = polycn;
            _GroupCN = groupcn;
            _MetadataCN = metacn;

            _Comment = comment;
            _OnBoundary = onbnd;

            _AdjX = adjx;
            _AdjY = adjy;
            _AdjZ = adjz;

            _UnAdjX = unadjx;
            _UnAdjY = unadjy;
            _UnAdjZ = unadjz;

            _Accuracy = acc;

            if (qlinks != null)
            {
                foreach (String lcn in qlinks.Split('_'))
                {
                    if (!String.IsNullOrWhiteSpace(lcn))
                        _LinkedPoints.Add(lcn);
                } 
            }
        }

        protected virtual void LinkedPoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(LinkedPoints));

            if (e.OldItems == null ^ e.NewItems == null || e.NewItems != null && e.NewItems.Count > 0 ^ e.OldItems.Count < 0)
                OnPropertyChanged(nameof(HasQuondamLinks));
        }
        
        protected virtual void Polygon_PreviewPolygonAccuracyChanged(TtPolygon polygon)
        {
            SetAccuracy(polygon.Accuracy);
        }


        protected void OnLocationChanged()
        {
            if (LocationChangedEventEnabled)
            {
                PreviewLocationChanged?.Invoke(this);
                LocationChanged?.Invoke(this);
                LocationHasChanged = false;
            }
            else
            {
                LocationHasChanged = true;
            }
        }

        protected virtual void OnMetadataChanged()
        {
            OnPropertyChanged(
                nameof(AdjZ),
                nameof(UnAdjZ)
            );
        }

        public virtual void SetAccuracy(double accuracy)
        {
            Accuracy = accuracy;
        }
        

        public void AddLinkedPoint(QuondamPoint point)
        {
            if (!_LinkedPoints.Contains(point.CN))
            {
                _LinkedPoints.Add(point.CN);
            }
        }

        public void RemoveLinkedPoint(QuondamPoint point)
        {
            _LinkedPoints.Remove(point.CN);
        }

        public void ClearLinks()
        {
            _LinkedPoints.Clear();
        }


        public void SetUnAdjLocation(double x, double y, double z)
        {
            bool locEventEnabled = LocationChangedEventEnabled;

            LocationChangedEventEnabled = false;

            UnAdjX = x;
            UnAdjY = y;
            UnAdjZ = z;

            LocationChangedEventEnabled = locEventEnabled;

            OnLocationChanged();
        }

        public void SetAdjLocation(double x, double y, double z)
        {
            bool locEventEnabled = LocationChangedEventEnabled;

            LocationChangedEventEnabled = false;

            AdjX = x;
            AdjY = y;
            AdjZ = z;

            LocationChangedEventEnabled = locEventEnabled;
        }
        

        public int CompareTo(TtPoint other)
        {
            return Compare(this, other);
        }

        public int Compare(TtPoint @this, TtPoint other)
        {
            if (@this == null && other == null)
                return 0;

            if (@this == null)
                return -1;

            if (other == null)
                return 1;

            int val = @this.Polygon.CompareTo(other.Polygon);

            if (val != 0)
                return val;
            else
            {
                val = @this.Index.CompareTo(other.Index);

                if (val != 0)
                    return val;
                else
                {
                    val = @this.PID.CompareTo(other.PID);

                    if (val != 0)
                        return val;
                    else
                        return @this.CN.CompareTo(other.CN);
                }
            }
        }


        public override bool Equals(object obj)
        {
            TtPoint point = obj as TtPoint;
            
            return base.Equals(point) &&
                _Index == point._Index &&
                _OnBoundary == point._OnBoundary &&
                _AdjX == point._AdjX &&
                _AdjY == point._AdjY &&
                _AdjZ == point._AdjZ &&
                _UnAdjX == point._UnAdjX &&
                _UnAdjY == point._UnAdjY &&
                _UnAdjZ == point._UnAdjZ &&
                _Accuracy == point._Accuracy &&
                _PID == point._PID &&
                _Comment == point._Comment &&
                _PolyCN == point._PolyCN &&
                _GroupCN == point._GroupCN &&
                _MetadataCN == point._MetadataCN;
        }

        public override int GetHashCode()
        {
            return CN.GetHashCode();
        }


        public override string ToString()
        {
            return String.Format("{0} ({1})", PID, OpType);
        }
    }
}
