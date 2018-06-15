using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public class PointNamer
    {
        public static int NamePoint(TtPolygon polygon)
        {
            return polygon.PointStartIndex;
        }

        public static int NamePoint(TtPolygon polygon, TtPoint prevPoint)
        {
            if (prevPoint != null)
                return prevPoint.PID + polygon.Increment;
            return polygon.PointStartIndex;
        }


    }
}
