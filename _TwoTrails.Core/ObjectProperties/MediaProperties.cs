using System;
using System.Reflection;
using TwoTrails.Core.Media;

namespace TwoTrails.Core
{
    public static class MediaProperties
    {
        public static readonly Type DataType = typeof(TtMedia);



        static MediaProperties()
        {
            BindingFlags bf = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        }
    }
}
