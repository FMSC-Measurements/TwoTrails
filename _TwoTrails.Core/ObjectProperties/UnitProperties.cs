using System;
using System.Reflection;
using TwoTrails.Core.Units;

namespace TwoTrails.Core
{
    public static class UnitProperties
    {
        public static readonly Type DataType = typeof(TtUnit);

        public static readonly PropertyInfo NAME;
        public static readonly PropertyInfo DESCRIPTION;
        public static readonly PropertyInfo POINT_START_INDEX;
        public static readonly PropertyInfo INCREMENT;
        public static readonly PropertyInfo ACCURACY;

        static UnitProperties()
        {
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            NAME = DataType.GetProperty(nameof(TtUnit.Name), bf);
            DESCRIPTION = DataType.GetProperty(nameof(TtUnit.Description), bf);
            POINT_START_INDEX = DataType.GetProperty(nameof(TtUnit.PointStartIndex), bf);
            INCREMENT = DataType.GetProperty(nameof(TtUnit.Increment), bf);
            ACCURACY = DataType.GetProperty(nameof(TtUnit.Accuracy), bf);
        }

        public static PropertyInfo GetPropertyByName(string name)
        {
            switch (name)
            {
                case nameof(TtUnit.Name): return NAME;
                case nameof(TtUnit.Description): return DESCRIPTION;
                case nameof(TtUnit.PointStartIndex): return POINT_START_INDEX;
                case nameof(TtUnit.Increment): return INCREMENT;
                case nameof(TtUnit.Accuracy): return ACCURACY;
            }

            return null;
        }
    }
}
