using System;
using System.Reflection;

namespace TwoTrails.Core
{
    public static class GroupProperties
    {
        public static readonly Type DataType = typeof(TtGroup);

        public static readonly PropertyInfo NAME;
        public static readonly PropertyInfo DESCRIPTION;
        public static readonly PropertyInfo GROUP_TYPE;


        static GroupProperties()
        {
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            NAME = DataType.GetProperty(nameof(TtGroup.Name), bf);
            DESCRIPTION = DataType.GetProperty(nameof(TtGroup.Description), bf);
            GROUP_TYPE = DataType.GetProperty(nameof(TtGroup.GroupType), bf);
        }

        public static PropertyInfo GetPropertyByName(string name)
        {
            switch (name)
            {
                case nameof(TtGroup.Name): return NAME;
                case nameof(TtGroup.Description): return DESCRIPTION;
                case nameof(TtGroup.GroupType): return GROUP_TYPE;
            }

            return null;
        }
    }
}
