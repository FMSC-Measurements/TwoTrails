﻿using System;

namespace TwoTrails.Core.Points
{
    public delegate void TraverseChangedEvent(TravPoint point);

    public class TravPoint : TtPoint
    {
        public event TraverseChangedEvent PositionChanged;

        #region Properties
        private Double? _FwdAzimuth;
        public Double? FwdAzimuth
        {
            get { return _FwdAzimuth; }
            set { SetField(ref _FwdAzimuth, AzimuthModule(value), AdjustAzimuth); }
        }
        
        private Double? _BkAzimuth;
        public Double? BkAzimuth
        {
            get { return _BkAzimuth; }
            set { SetField(ref _BkAzimuth, AzimuthModule(value), AdjustAzimuth); }
        }

        private Double _Azimuth = 0;
        public Double Azimuth { get { return _Azimuth; } }


        private Double _SlopeDistance = 0;
        public Double SlopeDistance
        {
            get { return _SlopeDistance; }
            set { SetField(ref _SlopeDistance, value, AdjustSlope); }
        }

        private Double _SlopeAngle = 0;
        public Double SlopeAngle
        {
            get { return _SlopeAngle; }
            set { SetField(ref _SlopeAngle, value, AdjustSlope); }
        }
        
        private Double _HorizontalDistance;
        public Double HorizontalDistance { get { return _HorizontalDistance; } }
        
        public Double Declination { get { return _Metadata.MagDec; } }
        

        public bool PositionHasChanged { get; protected set; }

        public override bool LocationChangedEventEnabled
        {
            set
            {
                base.LocationChangedEventEnabled = value;

                if (PositionHasChanged)
                    OnPositionChanged();
            }
        }
        
        public override OpType OpType { get { return OpType.Traverse; } }
        #endregion


        public TravPoint() : base() { }

        public TravPoint(TtPoint point) : base(point)
        {
            if (point is TravPoint trav)
                CopyTravValues(trav);
        }
        
        public TravPoint(TravPoint point) : base(point)
        {
            CopyTravValues(point);
        }

        public TravPoint(string cn, int index, int pid, DateTime time, string polycn, string metacn, string groupcn,
            string comment, bool onbnd, double adjx, double adjy, double adjz, double unadjx, double unadjy, double unadjz,
            double acc, string qlinks, double? fwd, double? bk, double sd, double sa, DataDictionary extended = null)
            : base(cn, index, pid, time, polycn, metacn, groupcn, comment, onbnd, adjx, adjy, adjz, unadjx,
            unadjy, unadjz, acc, qlinks, extended)
        {
            _FwdAzimuth = fwd;
            _BkAzimuth = bk;
            AdjustAzimuth();

            _SlopeDistance = sd;
            _SlopeAngle = sa;
            AdjustSlope();
        }

        private void CopyTravValues(TravPoint point)
        {
            _FwdAzimuth = point._FwdAzimuth;
            _BkAzimuth = point._BkAzimuth;

            _SlopeDistance = point._SlopeDistance;
            _SlopeAngle = point._SlopeAngle;

            AdjustAzimuth();
            AdjustSlope();
        }

        protected override void OnMetadataChanged()
        {
            base.OnMetadataChanged();
            
            OnPropertyChanged(
                nameof(SlopeDistance),
                nameof(SlopeAngle),
                nameof(HorizontalDistance),
                nameof(Declination)
            );
        }


        protected void OnPositionChanged()
        {
            if (LocationChangedEventEnabled)
            {
                PositionChanged?.Invoke(this);
                PositionHasChanged = false;
            }
            else
            {
                PositionHasChanged = true;
            }
        }


        protected void AdjustAzimuth()
        {
            double az = Double.PositiveInfinity;

            if (FwdAzimuth is double fwdaz && fwdaz >= 0)
            {
                if (BkAzimuth is double bkaz && bkaz >= 0)
                {
                    double adjBackAz;

                    if (bkaz > fwdaz && bkaz >= 180)
                    {
                        adjBackAz = bkaz - 180;
                    }
                    else
                    {
                        adjBackAz = bkaz + 180;
                    }

                    az = (fwdaz + adjBackAz) / 2d;

                    if (Math.Abs(az - adjBackAz) > 100)
                        az = AzimuthModule(az + 180);
                    else
                        az = AzimuthModule(az);
                }
                else
                {
                    az = fwdaz;
                }
            }
            else if (BkAzimuth is double bkaz && bkaz >= 0)
            {
                az = AzimuthModule(bkaz + 180);
            }

            if (!Double.IsInfinity(az))
            {
                SetField(ref _Azimuth, az, OnPositionChanged, nameof(Azimuth));
            }
        }

        protected void AdjustSlope()
        {
            SetField(ref _HorizontalDistance, _SlopeDistance * Math.Cos(FMSC.Core.Convert.PercentToRadians(SlopeAngle)), OnPositionChanged, nameof(HorizontalDistance));
        }


        public void Calculate(double prevX, double prevY, double prevZ, bool isPrevAdjusted)
        {
            double az = Azimuth;

            if (az > -1)
            {
                az = 90 - Azimuth - Declination;

                double x, y, z, azInRadians = FMSC.Core.Convert.DegreesToRadians(az);

                x = prevX + (HorizontalDistance * Math.Cos(azInRadians));
                y = prevY + (HorizontalDistance * Math.Sin(azInRadians));
                z = prevZ + (SlopeDistance * Math.Sin(FMSC.Core.Convert.PercentToRadians(SlopeAngle)));

                if (isPrevAdjusted)
                {
                    bool changed = SetField(ref _AdjX, x, nameof(AdjX));
                    changed |= SetField(ref _AdjY, y, nameof(AdjY));
                    changed |= SetField(ref _AdjZ, z, nameof(AdjZ));

                    if (changed)
                        OnLocationChanged();
                }
                else
                {
                    SetField(ref _UnAdjX, x,  nameof(UnAdjX));
                    SetField(ref _UnAdjY, y, nameof(UnAdjY));
                    SetField(ref _UnAdjZ, z, nameof(UnAdjZ));
                }
            }
        }


        protected Double AzimuthModule(Double value)
        {
            double integerPart = Math.Floor(value);
            return (integerPart % 360) + value - integerPart;
        }

        protected Double? AzimuthModule(Double? value)
        {
            return (value != null) ? (double?)AzimuthModule((double)value) : null;
        }


        public override bool Equals(TtPoint point)
        {
            TravPoint travPoint = point as TravPoint;

            return base.Equals(point) &&
                _FwdAzimuth == travPoint._FwdAzimuth &&
                _BkAzimuth == travPoint._BkAzimuth &&
                _SlopeDistance == travPoint._SlopeDistance &&
                _SlopeAngle == travPoint._SlopeAngle;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
