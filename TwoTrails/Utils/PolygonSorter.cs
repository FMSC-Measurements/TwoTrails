using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Utils
{
    public class PolygonSorter : IComparer
    {
        public bool SortPolysByName { get; private set; }

        public PolygonSorter(bool sortPolysByName = true)
        {
            SortPolysByName = sortPolysByName;
        }

        public virtual int Compare(object x, object y)
        {
            if (x is TtPolygon xp && y is TtPolygon yp)
                return Compare(xp, yp);

            return 0;
        }

        public int Compare(TtPolygon x, TtPolygon y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            return SortPolysByName ? x.Name.ToLower().CompareTo(y.Name.ToLower()) : x.CompareTo(y);
        }
    }

    public class PolygonSorterDirect : PolygonSorter, IComparer<TtPolygon>
    {
        public PolygonSorterDirect(bool sortPolysByName = true) : base(sortPolysByName) { }
    }
}
