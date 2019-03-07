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
    }
}
