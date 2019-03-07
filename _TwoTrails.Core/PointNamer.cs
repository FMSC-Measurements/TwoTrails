using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class PointNamer
    {
        public static int NamePoint(TtPolygon polygon, TtPoint prevPoint = null)
        {
            return (prevPoint != null) ? prevPoint.PID + polygon.Increment : polygon.PointStartIndex;
        }
    }
}
