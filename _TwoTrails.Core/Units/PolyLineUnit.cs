using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.Units
{
    public class PolyLineUnit : TtUnit
    {
        public PolyLineUnit() : base()
        {

        }

        public PolyLineUnit(TtUnit unit) : base(unit)
        {
            //
        }

        public PolyLineUnit(string cn, string name, string desc, int psi, int inc, DateTime time,
            double acc, double area, double perim, double perimLine) :
            base(cn, name, desc, psi, inc, time, acc, area, perim, perimLine)
        {
            //
        }

        public override UnitType UnitType => UnitType.Polygon;

    }
}
