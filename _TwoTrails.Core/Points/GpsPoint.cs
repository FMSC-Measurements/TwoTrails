using FMSC.GeoSpatial.UTM;
using System;

namespace TwoTrails.Core.Points
{
    public class GpsPoint : TtPoint, IManualAccuracy
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

        public override double UnAdjX
        {
            get { return _UnAdjX; }
            set
            {
                SetField(ref _UnAdjX, value);
                AdjX = _UnAdjX;
            }
        }

        public override double UnAdjY
        {
            get { return _UnAdjY; }
            set
            {
                SetField(ref _UnAdjY, value);
                AdjY = _UnAdjY;
            }
        }

        public override double UnAdjZ
        {
            get { return _UnAdjZ; }
            set
            {
                SetField(ref _UnAdjZ, value);
                AdjZ = _UnAdjZ;
            }
        }


        private Double? _Latitude;
        public Double? Latitude
        {
            get { return _Latitude; }
            set { SetField(ref _Latitude, value); }
        }

        private Double? _Longitude;
        public Double? Longitude
        {
            get { return _Longitude; }
            set { SetField(ref _Longitude, value); }
        }

        private Double? _Elevation;
        public Double? Elevation
        {
            get { return _Elevation; }
            set { SetField(ref _Elevation, value); }
        }

        private Double? _RMSEr;
        public Double? RMSEr
        {
            get { return _RMSEr; }
            set { SetField(ref _RMSEr, value); }
        }

        public bool HasLatLon
        {
            get { return Latitude != null && Longitude != null; }
        }


        public override OpType OpType { get { return OpType.GPS; } }
        #endregion


        public GpsPoint() : base() { }

        public GpsPoint(TtPoint point) : base(point)
        {
            if (point is GpsPoint gps)
                CopyGpsValues(gps);

            if (point is IManualAccuracy imanacc)
                _ManualAccuracy = imanacc.ManualAccuracy;
        }

        public GpsPoint(GpsPoint point) : base(point)
        {
            CopyGpsValues(point);
        }

        public GpsPoint(string cn, int index, int pid, DateTime time, string polycn, string metacn, string groupcn,
            string comment, bool onbnd, double adjx, double adjy, double adjz, double unadjx, double unadjy, double unadjz,
            double acc, string qlinks, double? lat, double? lon, double? elev, double? manacc, double? rmser, DataDictionary extended = null)
            : base(cn, index, pid, time, polycn, metacn, groupcn, comment, onbnd, adjx, adjy, adjz, unadjx,
            unadjy, unadjz, acc, qlinks, extended)
        {
            _Latitude = lat;
            _Longitude = lon;
            _Elevation = elev;
            _ManualAccuracy = manacc;
            _RMSEr = rmser;

            if (_AdjX != _UnAdjX || _AdjY != _UnAdjY || _AdjZ != _UnAdjZ)
                SetUnAdjLocation(_UnAdjX, _UnAdjY, _UnAdjZ);
        }

        private void CopyGpsValues(GpsPoint point)
        {
            _AdjX = UnAdjX = point._UnAdjX;
            _AdjY = UnAdjY = point._UnAdjY;
            _AdjZ = UnAdjZ = point._UnAdjZ;

            _Latitude = point._Latitude;
            _Longitude = point._Longitude;
            _Elevation = point._Elevation;

            _ManualAccuracy = point._ManualAccuracy;

            Accuracy = point.Accuracy;
        }


        protected override void OnMetadataChanged()
        {
            base.OnMetadataChanged();
            OnPropertyChanged(nameof(Elevation));
        }


        public override void SetAccuracy(double accuracy)
        {
            base.SetAccuracy(ManualAccuracy ?? accuracy);
        }

        public void SetUnAdjLocation(TtPoint point)
        {
            SetUnAdjLocation(point.UnAdjX, point.UnAdjY, point.UnAdjZ);
        }

        public void SetUnAdjLocation(double lat, double lon, int zone, double elev = 0)
        {
            Latitude = lat;
            Longitude = lon;

            UTMCoords coords = UTMTools.ConvertLatLonSignedDecToUTM(lat, lon, zone);

            SetUnAdjLocation(coords.X, coords.Y, elev);
        }

        internal void Adjust()
        {
            if (HasLatLon)
                SetUnAdjLocation((double)Latitude, (double)Longitude, Metadata.Zone, Elevation ?? UnAdjZ);
            else
                SetUnAdjLocation(this);

            SetAccuracy(Polygon.Accuracy);
        }

        public override bool Equals(object obj)
        {
            GpsPoint point = obj as GpsPoint;

            return base.Equals(point) &&
                _Latitude == point._Latitude &&
                _Longitude == point._Longitude &&
                _Elevation == point._Elevation &&
                _ManualAccuracy == point._ManualAccuracy;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
