namespace FMSC.Core.Utilities
{
    public static class ParseEx
    {
        public static float? ParseFloatN(string value)
        {
            if (float.TryParse(value, out float val))
                return val;
            return null;
        }
    }
}
