using FMSC.Core;
using System.Collections;
using TwoTrails.Core.Points;

namespace TwoTrails.Utils
{
    public class PointSorter : IComparer
    {
        public bool SortPolysByName { get; private set; }

        public PointSorter(bool sortPolysByName = true)
        {
            SortPolysByName = sortPolysByName;
        }

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

            int val = SortPolysByName ? xp.Polygon.Name.CompareToNatural(yp.Polygon.Name) : xp.Polygon.TimeCreated.CompareTo(yp.Polygon.TimeCreated);

            if (val != 0)
                return val;
            else
            {
                val = xp.PolygonCN.CompareTo(yp.PolygonCN);

                if (val != 0)
                    return val;
                else
                {
                    val = xp.Index.CompareTo(yp.Index);

                    if (val != 0)
                        return val;
                    else
                    {
                        val = xp.PID.CompareTo(yp.PID);

                        if (val != 0)
                            return val;
                        else
                            return xp.CN.CompareTo(yp.CN);
                    }
                }
            }
        }
    }
}
