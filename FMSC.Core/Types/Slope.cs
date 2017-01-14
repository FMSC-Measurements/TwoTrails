using System;

namespace FMSC.Core
{
    public enum Slope
    {
        Percent = 0,
        Degrees = 1
    }

    public static partial class Types
    {
        public static Slope ParseSlope(String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "p":
                case "percent": return Slope.Percent;
                case "1":
                case "d":
                case "degrees": return Slope.Degrees;
            }

            if (value.Length > 2 && value.Contains(" "))
                return ParseSlope(value.Split(' ')[0]);

            throw new Exception("Unknown Slope Type");
        }
    }
}
