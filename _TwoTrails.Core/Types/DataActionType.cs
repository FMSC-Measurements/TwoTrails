using System;

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
        MovedPoints          = 1 << 17,
        RetracePoints       = 1 << 18,
        ReindexPoints       = 1 << 19,
        ConvertedPoints       = 1 << 20,
        ModifiedDataDictionary  = 1 << 21,
        DataImported        = 1 << 22,
        ProjectUpgraded     = 1 << 23,
        InsertedNmea        = 1 << 24,
        DeletedNmea         = 1 << 25,
        RezonedPoints       = 1 << 26,
        ModifiedNmea        = 1 << 27,
        PointMinimization    = 1 << 28
    }

    public static class DataActionTypeExtensions
    {
        public static bool AffectsPoints(this DataActionType action)
        {
            return
                action.HasFlag(DataActionType.InsertedPoints) ||
                action.HasFlag(DataActionType.ModifiedPoints) ||
                action.HasFlag(DataActionType.DeletedPoints) ||
                action.HasFlag(DataActionType.ManualPointCreation) ||
                action.HasFlag(DataActionType.MovedPoints) ||
                action.HasFlag(DataActionType.RetracePoints) ||
                action.HasFlag(DataActionType.ReindexPoints) ||
                action.HasFlag(DataActionType.ConvertedPoints) ||
                action.HasFlag(DataActionType.RezonedPoints) ||
                action.HasFlag(DataActionType.PointMinimization);
        }

        public static bool AffectsPolygons(this DataActionType action)
        {
            return
                action.HasFlag(DataActionType.InsertedPolygons) ||
                action.HasFlag(DataActionType.ModifiedPolygons) ||
                action.HasFlag(DataActionType.DeletedPolygons);
        }

        public static bool AffectsMetadata(this DataActionType action)
        {
            return
                action.HasFlag(DataActionType.InsertedMetadata) ||
                action.HasFlag(DataActionType.ModifiedMetadata) ||
                action.HasFlag(DataActionType.DeletedMetadata);
        }

        public static bool AffectsGroups(this DataActionType action)
        {
            return
                action.HasFlag(DataActionType.InsertedGroups) ||
                action.HasFlag(DataActionType.ModifiedGroups) ||
                action.HasFlag(DataActionType.DeletedGroups);
        }

        public static bool AffectsProject(this DataActionType action)
        {
            return
                action.HasFlag(DataActionType.ProjectUpgraded) ||
                action.HasFlag(DataActionType.ModifiedProject);
        }

        public static bool AffectsNmea(this DataActionType action)
        {
            return
                action.HasFlag(DataActionType.InsertedNmea) ||
                action.HasFlag(DataActionType.ModifiedNmea) ||
                action.HasFlag(DataActionType.DeletedNmea);
        }

        public static bool AffectsMedia(this DataActionType action)
        {
            return
                action.HasFlag(DataActionType.InsertedMedia) ||
                action.HasFlag(DataActionType.ModifiedMedia) ||
                action.HasFlag(DataActionType.DeletedMedia);
        }

        public static bool AffectsDataDictionary(this DataActionType action)
        {
            return action.HasFlag(DataActionType.ModifiedDataDictionary);
        }
    }
}
