using FMSC.Core;
using FMSC.GeoSpatial;
using System;
using System.Collections.Generic;

namespace TwoTrails.Core
{
    public delegate void MetadataChangedEvent(TtMetadata polygon);
    public delegate void MetadataZoneChangedEvent(TtMetadata polygon, int oldZone);

    public class TtMetadata : TtObject, IComparable<TtMetadata>, IComparer<TtMetadata>
    {
        public event MetadataZoneChangedEvent ZoneChanged;
        public event MetadataChangedEvent MagDecChanged;

        #region Properties
        protected String _Name;
        public String Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value); }
        }

        private String _Comment = String.Empty;
        public String Comment
        {
            get { return _Comment; }
            set { SetField(ref _Comment, value); }
        }

        private Int32 _Zone;
        public Int32 Zone
        {
            get { return _Zone; }
            set
            {
                int oldZone = _Zone;
                SetField(ref _Zone, value, () => { OnZoneChanged(oldZone); });
            }
        }
        

        private DeclinationType _DecType;
        public DeclinationType DecType
        {
            get { return _DecType; }
            set { SetField(ref _DecType, value); }
        }

        private Double _MagDec = 0;
        public Double MagDec
        {
            get { return _MagDec; }
            set { SetField(ref _MagDec, value, OnMagDecChanged); }
        }


        private Datum _Datum;
        public Datum Datum
        {
            get { return _Datum; }
            set { SetField(ref _Datum, value); }
        }

        private Distance _Distance;
        public Distance Distance
        {
            get { return _Distance; }
            set { SetField(ref _Distance, value); }
        }

        private Distance _Elevation;
        public Distance Elevation
        {
            get { return _Elevation; }
            set { SetField(ref _Elevation, value); }
        }

        private Slope _Slope;
        public Slope Slope
        {
            get { return _Slope; }
            set { SetField(ref _Slope, value); }
        }


        private String _GpsReceiver = String.Empty;
        public String GpsReceiver
        {
            get { return _GpsReceiver; }
            set { SetField(ref _GpsReceiver, value); }
        }

        private String _RangeFinder = String.Empty;
        public String RangeFinder
        {
            get { return _RangeFinder; }
            set { SetField(ref _RangeFinder, value); }
        }

        private String _Compass = String.Empty;
        public String Compass
        {
            get { return _Compass; }
            set { SetField(ref _Compass, value); }
        }

        private String _Crew = String.Empty;
        public String Crew
        {
            get { return _Crew; }
            set { SetField(ref _Crew, value); }
        }
        #endregion


        public TtMetadata() { }

        public TtMetadata(string name)
        {
            _Name = name;
        }

        public TtMetadata(TtMetadata meta) : base(meta.CN)
        {
            _Name = meta._Name;
            _Comment = meta._Comment;
            _Zone = meta._Zone;
            _DecType = meta._DecType;
            _MagDec = meta._MagDec;
            _Datum = meta._Datum;
            _Distance = meta._Distance;
            _Elevation = meta._Elevation;
            _Slope = meta._Slope;
            _GpsReceiver = meta._GpsReceiver;
            _RangeFinder = meta._RangeFinder;
            _Compass = meta._Compass;
            _Crew = meta._Crew;
        }

        public TtMetadata(string cn, string name, string cmt, int zone,
            DeclinationType decType, double magdec, Datum datum, Distance dist,
            Distance elev, Slope slope, string gps, string rf, string compass, string crew) : base(cn)
        {
            _Name = name;
            _Comment = cmt;
            _Zone = zone;
            _DecType = decType;
            _MagDec = magdec;
            _Datum = datum;
            _Distance = dist;
            _Elevation = elev;
            _Slope = slope;
            _GpsReceiver = gps;
            _RangeFinder = rf;
            _Compass = compass;
            _Crew = crew;
        }


        protected void OnZoneChanged(int oldZone)
        {
            ZoneChanged?.Invoke(this, oldZone);
        }

        protected void OnMagDecChanged()
        {
            MagDecChanged?.Invoke(this);
        }


        public int CompareTo(TtMetadata other)
        {
            return Compare(this, other);
        }

        public int Compare(TtMetadata @this, TtMetadata other)
        {
            return @this.Name.CompareToNatural(other.Name);
        }


        public override string ToString()
        {
            return Name;
        }


        public override bool Equals(object obj)
        {
            TtMetadata meta = obj as TtMetadata;

            return base.Equals(meta) &&
                _Name == meta._Name &&
                _Comment == meta._Comment &&
                _Zone == meta._Zone &&
                _DecType == meta._DecType &&
                _MagDec == meta._MagDec &&
                _Datum == meta._Datum &&
                _Distance == meta._Distance &&
                _Elevation == meta._Elevation &&
                _Slope == meta._Slope &&
                _GpsReceiver == meta._GpsReceiver &&
                _RangeFinder == meta._RangeFinder &&
                _Compass == meta._Compass &&
                _Crew == meta._Crew;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
