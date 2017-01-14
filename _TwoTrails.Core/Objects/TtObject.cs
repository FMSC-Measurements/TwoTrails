using CSUtil.ComponentModel;
using System;

namespace TwoTrails.Core
{
    public class TtObject : NotifyPropertyChangedEx
    {
        private String _CN;
        public String CN
        {
            get
            {
                if (_CN == null)
                    _CN = Guid.NewGuid().ToString();
                return _CN;
            }

            set { SetField(ref _CN, value); }
        }


        public TtObject() { }

        public TtObject(string cn)
        {
            _CN = cn;
        }


        public override bool Equals(object obj)
        {
            TtObject tto = obj as TtObject;

            if (tto == null)
                return false;

            return CN == tto.CN;
        }

        public override int GetHashCode()
        {
            return CN.GetHashCode();
        }

    }
}
