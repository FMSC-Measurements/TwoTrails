﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoTrails.Core
{
    [Flags]
    public enum DataActionType : int
    {
        None                = 0,
        ModifiedProject     = 1 << 0,
        InsertedPoints      = 1 << 1,
        ModifiedPoints      = 1 << 2,
        DeletedPoints       = 1 << 3,
        InsertedPolygons    = 1 << 4,
        ModifiedPolygons    = 1 << 5,
        DeletedPolygons     = 1 << 6,
        InsertedMetadata    = 1 << 7,
        ModifiedMetadata    = 1 << 8,
        DeletedMetadata     = 1 << 9,
        InsertedGroups      = 1 << 10,
        ModifiedGroups      = 1 << 11,
        DeletedGroups       = 1 << 12,
        InsertedMedia       = 1 << 13,
        ModifiedMedia       = 1 << 14,
        DeletedMedia        = 1 << 15,
        ManualPointCreation = 1 << 16,
        MovePoints          = 1 << 17,
        RetracePoints       = 1 << 18,
        ReindexPoints       = 1 << 19,
        ConvertPoints       = 1 << 20
    }
}
