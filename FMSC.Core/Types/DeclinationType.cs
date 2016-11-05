using System;

namespace FMSC.Core
{
    public enum DeclinationType
    {
        MagDec = 0,
        DeedRot = 1
    }

    public static partial class Types
    {
        public static DeclinationType ParseDeclinationType(String value)
        {
            switch (value.ToLower())
            {
                case "0":
                case "md":
                case "magdec": return DeclinationType.MagDec;
                case "1":
                case "dr":
                case "deedrot": return DeclinationType.DeedRot;
            }

            if (value.Length > 2 && value.Contains(" "))
                return ParseDeclinationType(value.Split(' ')[0]);

            throw new Exception("Unknown DeclinationType");
        }
    }
}
