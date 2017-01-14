using System.Collections;
using TwoTrails.Core.Points;

namespace TwoTrails
{
    public class PointSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TtPoint xp = x as TtPoint;
            TtPoint yp = y as TtPoint;

            if (xp == null && yp == null)
                return 0;

            if (xp == null)
                return -1;

            if (yp == null)
                return 1;
            
            return xp.Compare(xp, yp);
        }
    }
}
