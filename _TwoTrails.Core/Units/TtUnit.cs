using System;
using System.Collections.Generic;

namespace TwoTrails.Core.Units
{
    public delegate void UnitChangedEvent(TtUnit unit);

    public abstract class TtUnit : TtObject, IComparable<TtUnit>, IComparer<TtUnit>
    {
        public event UnitChangedEvent UnitAccuracyChanged;
        public event UnitChangedEvent PreviewUnitAccuracyChanged;

        public event UnitChangedEvent UnitChanged;
        public event UnitChangedEvent PreviewUnitChanged;

        #region Properties
        protected String _Name;
        public String Name
        {
            get { return _Name; }
            set { SetField(ref _Name, value); }
        }

        public abstract UnitType UnitType { get; }
        
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
            set { SetField(ref _Accuracy, value, OnUnitAccuracyChanged); }
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

        public Double AreaAcres => _Area * FMSC.Core.Convert.SquareMeterToAcre_Coeff;
        public Double AreaHectaAcres => _Area * FMSC.Core.Convert.SquareMeterToHectare_Coeff;

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

        public Double PerimeterFt => Perimeter * FMSC.Core.Convert.MetersToFeet_Coeff;


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

        public Double PerimeterLineFt => PerimeterLine * FMSC.Core.Convert.MetersToFeet_Coeff;
        #endregion


        public TtUnit()
        {

        }

        public TtUnit(TtUnit unit) : base(unit.CN)
        {
            _Name = unit._Name;
            _Description = unit._Description;
            _PointStartIndex = unit._PointStartIndex;
            _Increment = unit._Increment;
            _TimeCreated = unit._TimeCreated;
            _Accuracy = unit._Accuracy;
            _Area = unit._Area;
            _Perimeter = unit._Perimeter;
        }

        public TtUnit(string cn, string name, string desc, int psi, int inc, DateTime time,
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

        protected void OnUnitAccuracyChanged()
        {
            PreviewUnitAccuracyChanged?.Invoke(this);
            UnitAccuracyChanged?.Invoke(this);
        }

        public void Update(double area, double perimeter, double linePerimeter)
        {
            Area = area;
            Perimeter = perimeter;
            PerimeterLine = linePerimeter;

            PreviewUnitChanged?.Invoke(this);
            UnitChanged?.Invoke(this);
        }


        public int CompareTo(TtUnit other)
        {
            return Compare(this, other);
        }

        public int Compare(TtUnit @this, TtUnit other)
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
            return $"{Name} ({UnitType})";
        }


        public override bool Equals(object obj)
        {
            TtUnit unit = obj as TtUnit;

            return base.Equals(unit) &&
                UnitType == unit.UnitType &&
                _Area == unit._Area &&
                _Perimeter == unit._Perimeter &&
                _Name == unit._Name &&
                _Accuracy == unit._Accuracy &&
                _Description == unit._Description &&
                _PointStartIndex == unit._PointStartIndex &&
                _Increment == unit._Increment;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
