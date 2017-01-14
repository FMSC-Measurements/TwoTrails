using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails.Core
{
    [Flags]
    public enum DataActivityType : int
    {
        Opened              = 0,
        ModifiedProject     = 1,
        InsertedPoints      = 2,
        ModifiedPoints      = 4,
        DeletedPoints       = 8,
        InsertedPolygons    = 16,
        ModifiedPolygons    = 32,
        DeletedPolygons     = 64,
        InsertedMetadata    = 128,
        ModifiedMetadata    = 256,
        DeletedMetadata     = 512,
        InsertedGroups      = 1024,
        ModifiedGroups      = 2048,
        DeletedGroups       = 4096
    }
}
