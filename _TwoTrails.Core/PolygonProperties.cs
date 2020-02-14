using System;
using System.Reflection;

namespace TwoTrails.Core
{
    public static class PolygonProperties
    {
        public static readonly Type DataType = typeof(TtPolygon);

        public static readonly PropertyInfo NAME;
        public static readonly PropertyInfo DESCRIPTION;
        public static readonly PropertyInfo POINT_START_INDEX;
        public static readonly PropertyInfo INCREMENT;
        public static readonly PropertyInfo ACCURACY;

        static PolygonProperties()
        {
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            NAME = DataType.GetProperty(nameof(TtPolygon.Name), bf);
            DESCRIPTION = DataType.GetProperty(nameof(TtPolygon.Description), bf);
            POINT_START_INDEX = DataType.GetProperty(nameof(TtPolygon.PointStartIndex), bf);
            INCREMENT = DataType.GetProperty(nameof(TtPolygon.Increment), bf);
            ACCURACY = DataType.GetProperty(nameof(TtPolygon.Accuracy), bf);
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
