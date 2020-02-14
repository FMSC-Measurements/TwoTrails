using System;
using System.Collections.Generic;

namespace TwoTrails.Core
{
    public delegate void PolygonChangedEvent(TtPolygon polygon);

    public class TtPolygon : TtObject, IComparable<TtPolygon>, IComparer<TtPolygon>
    {
        public event PolygonChangedEvent PolygonAccuracyChanged;
        public event PolygonChangedEvent PreviewPolygonAccuracyChanged;

        public event PolygonChangedEvent PolygonChanged;
        public event PolygonChangedEvent PreviewPolygonChanged;

        #region Properties
        protected String _Name;
        public String Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value); }
        }
        
        private String _Description = String.Empty;
        public String Description
        {
            get { return _Description; }
            set { SetField(ref _Description, value); }
        }

        protected Int32 _PointStartIndex = Consts.DEFAULT_POINT_START_INDEX;
        public Int32 PointStartIndex
        {
            get { return _PointStartIndex; }
            set { SetField(ref _PointStartIndex, value); }
        }

        protected Int32 _Increment = Consts.DEFAULT_POINT_INCREMENT;
        public Int32 Increment
        {
            get { return _Increment; }
            set { SetField(ref _Increment, value); }
        }

        private DateTime _TimeCreated = DateTime.Now;
        public DateTime TimeCreated
        {
            get { return _TimeCreated; }
            set { SetField(ref _TimeCreated, value); }
        }

        protected Double _Accuracy = Consts.DEFAULT_POINT_ACCURACY;
        public Double Accuracy
        {
            get { return _Accuracy; }
            set { SetField(ref _Accuracy, value, OnPolygonAccuracyChanged); }
        }

        protected Double _Area = 0;
        public Double Area
        {
            get { return _Area; }
            set
            {
                if (SetField(ref _Area, value))
                {
                    OnPropertyChanged(nameof(AreaAcres), nameof(AreaHectaAcres));
                }
            }
        }

        public Double AreaAcres { get { return _Area * 0.00024711; } }
        public Double AreaHectaAcres { get { return _Area / 10000; } }

        protected Double _Perimeter = 0;
        public Double Perimeter
        {
            get { return _Perimeter; }
            set
            {
                if (SetField(ref _Perimeter, value))
                {
                    OnPropertyChanged(nameof(PerimeterFt));
                }
            }
        }

        public Double PerimeterFt { get { return Perimeter * 3937d / 1200d; } }


        protected Double _PerimeterLine = 0;
        public Double PerimeterLine
        {
            get { return _PerimeterLine; }
            set
            {
                if (SetField(ref _PerimeterLine, value))
                {
                    OnPropertyChanged(nameof(PerimeterLineFt));
                }
            }
        }

        public Double PerimeterLineFt { get { return PerimeterLine * 3937d / 1200d; } }
        #endregion


        public TtPolygon() { }

        public TtPolygon(TtPolygon polygon) : base(polygon.CN)
        {
            _Name = polygon._Name;
            _Description = polygon._Description;
            _PointStartIndex = polygon._PointStartIndex;
            _Increment = polygon._Increment;
            _TimeCreated = polygon._TimeCreated;
            _Accuracy = polygon._Accuracy;
            _Area = polygon._Area;
            _Perimeter = polygon._Perimeter;
        }

        public TtPolygon(string cn, string name, string desc, int psi, int inc, DateTime time,
            double acc, double area, double perim, double perimLine) : base(cn)
        {
            _Name = name;
            _Description = desc;
            _PointStartIndex = psi;
            _Increment = inc;
            _TimeCreated = time;
            _Accuracy = acc;
            _Area = area;
            _Perimeter = perim;
            _PerimeterLine = perimLine;
        }

        protected void OnPolygonAccuracyChanged()
        {
            PreviewPolygonAccuracyChanged?.Invoke(this);
            PolygonAccuracyChanged?.Invoke(this);
        }

        public void Update(double area, double perimeter, double linePerimeter)
        {
            Area = area;
            Perimeter = perimeter;
            PerimeterLine = linePerimeter;

            PreviewPolygonChanged?.Invoke(this);
            PolygonChanged?.Invoke(this);
        }


        public int CompareTo(TtPolygon other)
        {
            return Compare(this, other);
        }

        public int Compare(TtPolygon @this, TtPolygon other)
        {
            if (@this == null && other == null)
                return 0;

            if (@this == null)
                return -1;

            if (other == null)
                return 1;

            return @this.TimeCreated.CompareTo(other.TimeCreated);
        }

        public override string ToString()
        {
            return Name;
        }


        public override bool Equals(object obj)
        {
            TtPolygon polygon = obj as TtPolygon;

            return base.Equals(polygon) &&
                _Area == polygon._Area &&
                _Perimeter == polygon._Perimeter &&
                _Name == polygon._Name &&
                _Accuracy == polygon._Accuracy &&
                _Description == polygon._Description &&
                _PointStartIndex == polygon._PointStartIndex &&
                _Increment == polygon._Increment;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
