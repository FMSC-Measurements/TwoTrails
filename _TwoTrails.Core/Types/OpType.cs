using System;

namespace TwoTrails.Core
{
    public enum OpType
    {
        GPS = 0,
        Take5 = 6,
        Traverse = 1,
        SideShot = 4,
        Quondam = 3,
        Walk = 5,
        WayPoint = 2
    }

    public static partial class TtTypes
    {
        public static OpType ParseOpType(String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "gps": return OpType.GPS;
                case "1":
                case "trav":
                case "traverse": return OpType.Traverse;
                case "2":
                case "wp":
                case "waypoint":
                case "way point": return OpType.WayPoint;
                case "3":
                case "quondam":
                case "qndm": return OpType.Quondam;
                case "4":
                case "ss":
                case "sideshot":
                case "side shot": return OpType.SideShot;
                case "5":
                case "walk": return OpType.Walk;
                case "6":
                case "t5":
                case "take5":
                case "take 5": return OpType.Take5;
            }

            throw new Exception("Unknown OpType");
        }

        public static bool IsGpsType(this OpType op)
        {
            return op == OpType.GPS || op == OpType.Take5 ||
                op == OpType.Walk || op == OpType.WayPoint;
        }

        public static bool IsTravType(this OpType op)
        {
            return op == OpType.Traverse || op == OpType.SideShot;
        }
    }
}
