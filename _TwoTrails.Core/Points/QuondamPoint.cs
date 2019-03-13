using System;

namespace TwoTrails.Core.Points
{
    public class QuondamPoint : TtPoint, IManualAccuracy
    {
        #region Properties
        private Double? _ManualAccuracy;
        public Double? ManualAccuracy
        {
            get { return _ManualAccuracy; }
            set
            {
                SetField(ref _ManualAccuracy, value,
                    () => SetAccuracy(Polygon != null ? Polygon.Accuracy : Consts.DEFAULT_POINT_ACCURACY));
            }
        }

        public override double AdjX { get { return ParentPoint.AdjX; } }
        public override double AdjY { get { return ParentPoint.AdjY; } }
        public override double AdjZ{ get { return ParentPoint.AdjZ; } }

        public override double UnAdjX { get { return ParentPoint.UnAdjX; } }
        public override double UnAdjY { get { return ParentPoint.UnAdjY; } }
        public override double UnAdjZ { get { return ParentPoint.UnAdjZ; } }

        private TtPoint _ParentPoint;
        public TtPoint ParentPoint
        {
            get { return _ParentPoint; }
            set
            {
                TtPoint oldParent = _ParentPoint;
                if (SetField(ref _ParentPoint, value))
                {
                    if (_ParentPoint != null)
                    {
                        ParentPointCN = _ParentPoint.CN;

                        _ParentPoint.AddLinkedPoint(this);
                        _ParentPoint.LocationChanged += ParentPoint_LocationChanged;
                        _ParentPoint.OnAccuracyChanged += ParentPoint_OnAccuracyChanged;
                    }

                    if (oldParent != null)
                    {
                        oldParent.LocationChanged -= ParentPoint_LocationChanged;
                        _ParentPoint.OnAccuracyChanged -= ParentPoint_OnAccuracyChanged;
                        oldParent.RemoveLinkedPoint(this);

                        if (_ParentPoint != null && !oldParent.HasSameAdjLocation(_ParentPoint))
                        {
                            OnLocationChanged();
                        }
                    }
                    else
                    {
                        OnLocationChanged();
                    }
                }
            }
        }

        private String _ParentPointCN;
        public String ParentPointCN
        {
            get { return _ParentPointCN; }
            private set { SetField(ref _ParentPointCN, value); }
        }

        public override OpType OpType { get { return OpType.Quondam; } }

        public override bool OnBoundary { get => false; set => base.OnBoundary = value; }
        #endregion


        public QuondamPoint() : base() { }

        public QuondamPoint(TtPoint point) : base(point)
        {
            if (point is QuondamPoint qndm)
                CopyQndmValues(qndm);
        }

        public QuondamPoint(QuondamPoint point) : base(point)
        {
            CopyQndmValues(point);
        }

        public QuondamPoint(string cn, int index, int pid, DateTime time, string polycn, string metacn, string groupcn,
            string comment, bool onbnd, double adjx, double adjy, double adjz, double unadjx, double unadjy, double unadjz,
            double acc, string qlinks, string pcn, double? manacc, DataDictionary extended = null)
            : base(cn, index, pid, time, polycn, metacn, groupcn, comment, onbnd, adjx, adjy, adjz, unadjx,
            unadjy, unadjz, acc, qlinks, extended)
        {
            _ParentPointCN = pcn;
            _ManualAccuracy = manacc;
        }

        private void CopyQndmValues(QuondamPoint point)
        {
            _ParentPointCN = point._ParentPointCN;
            _ManualAccuracy = point._ManualAccuracy;
        }


        private void ParentPoint_LocationChanged(TtPoint point)
        {
            OnLocationChanged();
        }

        private void ParentPoint_OnAccuracyChanged(TtPoint point)
        {
            SetAccuracy(point.Accuracy);
        }

        public override void SetAccuracy(double accuracy)
        {
            base.SetAccuracy(ManualAccuracy ?? ParentPoint.Accuracy);
        }


        public override string ToString()
        {
            return $"{base.ToString()}{(ParentPoint != null ? $" \u2794 {ParentPoint.ToString()} : {ParentPoint.Polygon.Name}" : String.Empty)}";
        }


        public override bool Equals(object obj)
        {
            QuondamPoint point = obj as QuondamPoint;

            return base.Equals(point) &&
                _ParentPointCN == point._ParentPointCN &&
                _ManualAccuracy == point._ManualAccuracy;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
