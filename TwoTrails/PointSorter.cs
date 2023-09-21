using System.Collections;
using TwoTrails.Core.Points;

namespace TwoTrails
{
    public class PointSorter : IComparer
    {
        public bool SortPolysByName {get; private set;}

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

            int val = SortPolysByName ? xp.Unit.Name.CompareTo(yp.Unit.Name) : xp.Unit.TimeCreated.CompareTo(yp.Unit.TimeCreated);

            if (val != 0)
                return val;
            else
            {
                val = SortPolysByName ? xp.Unit.TimeCreated.CompareTo(yp.Unit.TimeCreated) : xp.Unit.Name.CompareTo(yp.Unit.Name);

                if (val != 0)
                    return val;
                else
                {
                    val = xp.UnitCN.CompareTo(yp.UnitCN);

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
}
