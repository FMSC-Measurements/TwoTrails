using System;
using System.Collections.Generic;
using FSConvert = FMSC.Core.Convert;

namespace TwoTrails.Core
{
    public delegate void PolygonChangedEvent(TtPolygon polygon);

    public class TtPolygon : TtObject, IComparable<TtPolygon>, IComparer<TtPolygon>
    {
        public event PolygonChangedEvent PreviewPolygonAccuracyChanged;
        public event PolygonChangedEvent PolygonAccuracyChanged;

        public event PolygonChangedEvent PreviewPolygonChanged;
        public event PolygonChangedEvent PolygonChanged;


        #region Properties
        protected String _Name;
        public String Name
        {
            get => _Name;
            set { SetField(ref _Name, value); }
        }
        
        private String _Description = String.Empty;
        public String Description
        {
            get => _Description;
            set { SetField(ref _Description, value); }
        }

        protected Int32 _PointStartIndex = Consts.DEFAULT_POINT_START_INDEX;
        public Int32 PointStartIndex
        {
            get => _PointStartIndex;
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
            get => _TimeCreated;
            set { SetField(ref _TimeCreated, value); }
        }

        protected Double _Accuracy = Consts.DEFAULT_POINT_ACCURACY;
        public Double Accuracy
        {
            get => _Accuracy;
            set { SetField(ref _Accuracy, value, OnPolygonAccuracyChanged); }
        }

        protected Double _Area = 0;
        public Double Area
        {
            get => _Area;
            private set { SetField(ref _Area, value, () => OnPropertyChanged(nameof(AreaAcres), nameof(AreaHectaAcres))); }
        }

        public Double AreaAcres => _Area * FSConvert.SquareMeterToAcre_Coeff;
        public Double AreaHectaAcres => _Area * FSConvert.SquareMeterToHectare_Coeff;

        protected Double _Perimeter = 0;
        public Double Perimeter
        {
            get => _Perimeter;
            private set { SetField(ref _Perimeter, value, () => OnPropertyChanged(nameof(PerimeterFt))); }
        }

        protected Double _PerimeterLine = 0;
        public Double PerimeterLine
        {
            get => _PerimeterLine;
            private set { SetField(ref _PerimeterLine, value, () => OnPropertyChanged(nameof(PerimeterLineFt))); }
        }

        public Double PerimeterFt => Perimeter * FSConvert.MetersToFeet_Coeff;
        public Double PerimeterLineFt => PerimeterLine * FSConvert.MetersToFeet_Coeff;


        protected TtPolygon _ParentUnit;
        public TtPolygon ParentUnit
        {
            get => _ParentUnit;
            set
            {
                TtPolygon oldParent = _ParentUnit;
                
                //TODO check unit type if area-less throw area (for multi unit types)

                if (SetField(ref _ParentUnit, value))
                {
                    ParentUnitCN = _ParentUnit?.CN;

                    if (oldParent != null && oldParent.PolygonChanged != null)
                    {
                        oldParent.PolygonChanged.Invoke(oldParent);
                    }
                    else if (_ParentUnit.PolygonChanged != null)
                    {
                        _ParentUnit.PolygonChanged.Invoke(_ParentUnit);
                    }
                }
            }
        }


        protected String _ParentUnitCN;
        public String ParentUnitCN
        {
            get => _ParentUnitCN;
            private set { SetField(ref _ParentUnitCN, value); }
        }
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
            _PerimeterLine = polygon._PerimeterLine;
            _ParentUnitCN = polygon._ParentUnitCN;
        }

        public TtPolygon(string cn, string name, string desc, int psi, int inc, DateTime time,
            double acc, double area, double perim, double perimLine, string parentUnitCN = null) : base(cn)
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
            _ParentUnitCN = parentUnitCN;
        }


        public void OnPolygonAccuracyChanged()
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


        public override string ToString() => Name;


        public override bool Equals(object obj)
        {
            TtPolygon polygon = obj as TtPolygon;

            return base.Equals(polygon) &&
                _Area == polygon._Area &&
                _Perimeter == polygon._Perimeter &&
                _PerimeterLine == polygon._PerimeterLine &&
                _Name == polygon._Name &&
                _Accuracy == polygon._Accuracy &&
                _Description == polygon._Description &&
                _PointStartIndex == polygon._PointStartIndex &&
                _Increment == polygon._Increment &&
                _ParentUnitCN == polygon._ParentUnitCN &&
                _TimeCreated == polygon._TimeCreated;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
