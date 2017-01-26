using FMSC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public static class HaidLogic
    {
        public static object locker = new object();

        public static PolygonSummary GenerateSummary(ITtManager manager, TtPolygon polygon, bool showPoints = false)
        {
            lock (locker)
            {
                return new PolygonSummary(manager, polygon, showPoints);
            }
        }
    }
}
