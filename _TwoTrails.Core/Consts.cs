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

        public static readonly String EmptyGuid = "00000000-0000-0000-0000-000000000000";
        public const String FullGuid = "11111111-1111-1111-1111-111111111111";

        public const double MINIMUM_POINT_DIGIT_ACCURACY = 0.000001d;
        public const double DEFAULT_POINT_ACCURACY = 6.01d;

        public const String FILE_EXTENSION = ".ttx";
        public const String FILE_EXTENSION_FILTER = "TwoTrails Files (*.ttx)|.ttx";

        public const String FILE_EXTENSION_MEDIA = ".ttmpx";
        public const String FILE_EXTENSION_FILTER_MEDIA = "TwoTrails Files (*.ttmpx)|.ttmpx";

        public const String FILE_EXTENSION_V2 = ".tt2";
        public const String FILE_EXTENSION_FILTER_V2 = "TwoTrails2 Files (*.tt2)|.tt2";
    }
}
