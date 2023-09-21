using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core.Units
{
    public class PolygonUnit : TtUnit
    {
        public PolygonUnit() : base()
        {

        }

        public PolygonUnit(TtUnit unit) : base(unit)
        {
            //
        }

        public PolygonUnit(string cn, string name, string desc, int psi, int inc, DateTime time,
            double acc, double area, double perim, double perimLine) :
            base(cn, name, desc, psi, inc, time, acc, area, perim, perimLine)
        {
            //
        }

        public override UnitType UnitType => UnitType.Polygon;

    }
}
