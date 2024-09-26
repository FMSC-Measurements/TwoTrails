using System.Collections.Generic;
using System.Linq;
using TwoTrails.Core.Units;

namespace TwoTrails.Core
{
    public static class TtObjectExtensions
    {
        public static TtPolygon DeepCopy(this TtPolygon polygon)
        {
            return new TtPolygon(polygon);
        }

        public static IEnumerable<TtPolygon> DeepCopy(this IEnumerable<TtPolygon> polygons)
        {
            return polygons.Select(p => p.DeepCopy());
        }


        public static TtGroup DeepCopy(this TtGroup group)
        {
            return new TtGroup(group);
        }

        public static IEnumerable<TtGroup> DeepCopy(this IEnumerable<TtGroup> groups)
        {
            return groups.Select(g => g.DeepCopy());
        }


        public static TtMetadata DeepCopy(this TtMetadata metadata)
        {
            return new TtMetadata(metadata);
        }

        public static IEnumerable<TtMetadata> DeepCopy(this IEnumerable<TtMetadata> metadata)
        {
            return metadata.Select(m => m.DeepCopy());
        }


        public static TtNmeaBurst DeepCopy(this TtNmeaBurst nmea)
        {
            return new TtNmeaBurst(nmea);
        }

        public static IEnumerable<TtNmeaBurst> DeepCopy(this IEnumerable<TtNmeaBurst> bursts)
        {
            return bursts.Select(b => b.DeepCopy());
        }
    }
}
