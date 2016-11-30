using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails.Core
{
    public static class Consts
    {
        public const String DATE_FORMAT = "M/d/yyyy h:mm:ss.SSS";

        public static readonly String EmptyGuid = Guid.Empty.ToString();
        public const String FullGuid = "11111111-1111-1111-1111-111111111111";

        public const double MINIMUM_POINT_DIGIT_ACCURACY = 0.000001d;
        public const double DEFAULT_POINT_ACCURACY = 6.01d;
    }
}
