using System;
using System.Collections.Generic;
using System.Text;

namespace TwoTrails.Core
{
    public enum UnitType
    {
        Polygon = 0,
        PolyLine = 1,
        Plots = 2,
        LogDeck = 3
    }

    public static partial class TtTypes
    {
        public static UnitType ParseUnitType(String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "poly":
                case "polygon": return UnitType.Polygon;
                case "1":
                case "polyline": return UnitType.PolyLine;
                case "2":
                case "plts":
                case "plots": return UnitType.Plots;
                case "3":
                case "logdeck": return UnitType.LogDeck;
            }

            throw new Exception("Unknown UnitType");
        }
    }
}
