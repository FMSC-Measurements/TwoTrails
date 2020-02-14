using System;
using System.Reflection;
using TwoTrails.Core.Points;

namespace TwoTrails.Core
{
    public static class PolygonProperties
    {
        public static readonly PropertyInfo NAME;
        public static readonly PropertyInfo DESCRIPTION;
        public static readonly PropertyInfo POINT_START_INDEX;
        public static readonly PropertyInfo INCREMENT;
        public static readonly PropertyInfo ACCURACY;

        static PolygonProperties()
        {
            Type pt = typeof(TtPolygon);
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            NAME = pt.GetProperty(nameof(TtPolygon.Name), bf);
            DESCRIPTION = pt.GetProperty(nameof(TtPolygon.Description), bf);
            POINT_START_INDEX = pt.GetProperty(nameof(TtPolygon.PointStartIndex), bf);
            INCREMENT = pt.GetProperty(nameof(TtPolygon.Increment), bf);
            ACCURACY = pt.GetProperty(nameof(TtPolygon.Accuracy), bf);
        }

        public static PropertyInfo GetPropertyByName(string name)
        {
            switch (name)
            {
                case nameof(TtPolygon.Name): return NAME;
                case nameof(TtPolygon.Description): return DESCRIPTION;
                case nameof(TtPolygon.PointStartIndex): return POINT_START_INDEX;
                case nameof(TtPolygon.Increment): return INCREMENT;
                case nameof(TtPolygon.Accuracy): return ACCURACY;
            }

            return null;
        }
    }
}
