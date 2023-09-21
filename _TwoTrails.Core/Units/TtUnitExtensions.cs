using System;
using System.Collections.Generic;
using System.Linq;

namespace TwoTrails.Core.Units
{
    public static class TtUnitExtensions
    {
        public static TtUnit DeepCopy(this TtUnit unit)
        {
            switch (unit.UnitType)
            {
                case UnitType.Polygon: return new PolygonUnit(unit);
                case UnitType.PolyLine:
                case UnitType.Plots:
                case UnitType.LogDeck:
                default: throw new Exception("Unkown Unit Type");
            }
        }

        public static IEnumerable<TtUnit> DeepCopy(this IEnumerable<TtUnit> units)
        {
            return units.Select(p => p.DeepCopy());
        }
    }
}
