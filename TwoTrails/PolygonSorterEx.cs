using System;
using TwoTrails.Core;

namespace TwoTrails
{
    public class PolygonSorterEx<TModel> : PolygonSorter where TModel : class
    {
        private Func<TModel, TtPolygon> _Func;

        public PolygonSorterEx(Func<TModel, TtPolygon> func, bool sortPolysByName = true) : base(sortPolysByName)
        {
            this._Func = func;
        }

        public override int Compare(object x, object y)
        {
            if (x is TModel xv && y is TModel yv)
            {
                return Compare(_Func(xv), _Func(yv));
            }

            return 0;
        }
    }
}
