﻿using FMSC.Core.ComponentModel;
using System;

namespace TwoTrails.Core
{
    public class TtObject : BaseModel
    {
        private String _CN;
        public String CN
        {
            get { return _CN ?? (_CN = Guid.NewGuid().ToString()); }

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

            return tto != null && CN == tto.CN;
        }

        public override int GetHashCode()
        {
            return CN.GetHashCode();
        }
    }
}
