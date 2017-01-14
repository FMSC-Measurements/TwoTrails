using System;

namespace TwoTrails.Core
{
    public enum GroupType
    {
        General = 0,
        Walk = 1,
        Take5 = 2
    }

    public static partial class TtTypes
    {
        public static GroupType ParseGroupType(this String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "g":
                case "general": return GroupType.General;
                case "1":
                case "w":
                case "walk": return GroupType.Walk;
                case "2":
                case "t5":
                case "take5":
                case "take 5": return GroupType.Take5;
            }

            throw new Exception("Unknown GroupType");
        }
    }
}
