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
