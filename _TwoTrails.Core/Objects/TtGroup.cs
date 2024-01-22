using FMSC.Core;
using System;
using System.Collections.Generic;

namespace TwoTrails.Core
{
    public class TtGroup : TtObject, IComparable<TtGroup>, IComparer<TtGroup>
    {
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

        private GroupType _GroupType = GroupType.General;
        public GroupType GroupType
        {
            get { return _GroupType; }
            set { SetField(ref _GroupType, value); }
        }
        #endregion


        public TtGroup() { }

        public TtGroup(string name)
        {
            _Name = name;
        }

        public TtGroup(string name, string description) : this(name)
        {
            _Description = description;
        }

        public TtGroup(TtGroup group) : base(group.CN)
        {
            _Name = group._Name;
            _Description = group._Description;
            _GroupType = group._GroupType;
        }

        public TtGroup(string cn, string name, string desc, GroupType gt) : base(cn)
        {
            _Name = name;
            _Description = desc;
            _GroupType = gt;
        }


        public int CompareTo(TtGroup other)
        {
            return Compare(this, other);
        }

        public int Compare(TtGroup @this, TtGroup other)
        {
            return @this.Name.CompareToNatural(other.Name);
        }

        public override string ToString()
        {
            return $"{Name} ({GroupType})";
        }


        public override bool Equals(object obj)
        {
            TtGroup group = obj as TtGroup;

            return base.Equals(obj) &&
                _Name == group._Name &&
                _Description == group._Description &&
                _GroupType == group._GroupType;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
