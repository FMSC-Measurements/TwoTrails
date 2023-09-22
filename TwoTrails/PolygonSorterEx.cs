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

    public class PolygonPrioritySorterEx<TModel> : PolygonSorter where TModel : class
    {
        private Func<TModel, TtPolygon> _PolyFunc;
        private Func<TModel, bool> _PriorityFunc = null;

        public PolygonPrioritySorterEx(Func<TModel, TtPolygon> func, Func<TModel, bool> pfunc, bool sortPolysByName = true) : base(sortPolysByName)
        {
            this._PolyFunc = func;
            this._PriorityFunc = pfunc;
        }

        public override int Compare(object x, object y)
        {
            if (x is TModel xv && y is TModel yv)
            {
                if (_PriorityFunc(xv))
                    return -1;

                if (_PriorityFunc(yv))
                    return 1;

                return Compare(_PolyFunc(xv), _PolyFunc(yv));
            }

            return 0;
        }
    }
}
