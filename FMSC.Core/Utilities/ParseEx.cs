using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
