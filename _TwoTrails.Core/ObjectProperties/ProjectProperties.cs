using System;
using System.Reflection;

namespace TwoTrails.Core
{
    public static class ProjectProperties
    {
        public static readonly Type DataType = typeof(TtProjectInfo);

        public static readonly PropertyInfo NAME;
        public static readonly PropertyInfo DESCRIPTION;


        static ProjectProperties()
        {
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            NAME = DataType.GetProperty(nameof(TtProjectInfo.Name), bf);
            DESCRIPTION = DataType.GetProperty(nameof(TtProjectInfo.Description), bf);
        }

        public static PropertyInfo GetPropertyByName(string name)
        {
            switch (name)
            {
                case nameof(TtProjectInfo.Name): return NAME;
                case nameof(TtProjectInfo.Description): return DESCRIPTION;
            }

            return null;
        }
    }
}
