using TwoTrails.Core.Points;
using TwoTrails.Core.Units;

namespace TwoTrails.Core
{
    public class PointNamer
    {
        public static int NamePoint(TtUnit unit, TtPoint prevPoint = null)
        {
            return (prevPoint != null) ? prevPoint.PID + unit.Increment : unit.PointStartIndex;
        }
    }
}
