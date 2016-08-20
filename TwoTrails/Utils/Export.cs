using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public class Export
    {
        public static void Points(ITtManager manager, String fileName)
        {
            Points(manager.GetPoints(), manager.GetMetadata().ToDictionary(m => m.CN, m => m), fileName);
        }

        public static void Points(List<TtPoint> points, Dictionary<String, TtMetadata> metadata, String fileName)
        {
            points.Sort();

            //write to csv
        }
    }
}
