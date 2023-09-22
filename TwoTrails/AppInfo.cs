using System;
using System.Reflection;

namespace TwoTrails
{
    public static class AppInfo
    {
        public static Version Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public static String GetVersionWithBuildType(this Version version, bool excludeRelease = true)
        {
#if DEBUG
            return $"{version}-Debug";
#elif PREVIEW
            return $"{version}-Preview";
#else
            return $"{version}{(excludeRelease ? "" : "-Release")}";
#endif
        }
    }
}
