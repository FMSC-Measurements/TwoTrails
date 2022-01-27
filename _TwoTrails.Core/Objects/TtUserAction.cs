using System;
using System.Collections;

namespace TwoTrails.Core
{
    public class TtUserAction
    {
        public String UserName { get; }
        public String DeviceName { get; }
        public DateTime Date { get; private set; }
        public DataActionType Action { get; private set; }
        public String Notes { get; private set; }

        public String AppVersion { get; private set; }
        
        public bool ProjectModified => Action.HasFlag(DataActionType.ModifiedProject);

        public bool PointsInserted => Action.HasFlag(DataActionType.InsertedPoints);
        public bool PointsModified => Action.HasFlag(DataActionType.ModifiedPoints);
        public bool PointsDeleted => Action.HasFlag(DataActionType.DeletedPoints);

        public bool PolygonsInserted => Action.HasFlag(DataActionType.InsertedPolygons);
        public bool PolygonsModified => Action.HasFlag(DataActionType.ModifiedPolygons);
        public bool PolygonsDeleted => Action.HasFlag(DataActionType.DeletedPolygons);

        public bool MetadataInserted => Action.HasFlag(DataActionType.InsertedMetadata);
        public bool MetadataModified => Action.HasFlag(DataActionType.ModifiedMetadata);
        public bool MetadataDeleted => Action.HasFlag(DataActionType.DeletedMetadata);

        public bool GroupsInserted => Action.HasFlag(DataActionType.InsertedGroups);
        public bool GroupsModified => Action.HasFlag(DataActionType.ModifiedGroups);
        public bool GroupsDeleted => Action.HasFlag(DataActionType.DeletedGroups);

        public bool MediaInserted => Action.HasFlag(DataActionType.InsertedMedia);
        public bool MediaModified => Action.HasFlag(DataActionType.ModifiedMedia);
        public bool MediaDeleted => Action.HasFlag(DataActionType.DeletedMedia);

        public bool NmeaInserted => Action.HasFlag(DataActionType.InsertedNmea);
        public bool NmeaDeleted => Action.HasFlag(DataActionType.DeletedNmea);
        public bool NmeaModified => Action.HasFlag(DataActionType.ModifiedNmea);

        public bool ManualCreatedPoints => Action.HasFlag(DataActionType.ManualPointCreation);
        public bool PointsMoved => Action.HasFlag(DataActionType.MovedPoints);
        public bool PointsRetraced => Action.HasFlag(DataActionType.RetracePoints);
        public bool PointsReindexed => Action.HasFlag(DataActionType.ReindexPoints);
        public bool PointsConverted => Action.HasFlag(DataActionType.ConvertedPoints);
        public bool PointsRezoned => Action.HasFlag(DataActionType.RezonedPoints);
        
        public bool DataImported => Action.HasFlag(DataActionType.DataImported);
        public bool DataDictionaryModified => Action.HasFlag(DataActionType.ModifiedDataDictionary);

        public bool ProjectUpgraded => Action.HasFlag(DataActionType.ProjectUpgraded);


        public TtUserAction(String userName, String deviceName, String appVersion) :
            this(userName, deviceName, appVersion, DateTime.Now, DataActionType.None)
        { }

        public TtUserAction(String userName, String deviceName, String appVersion,
            DateTime date, DataActionType action, String notes = null)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            if (String.IsNullOrEmpty(deviceName))
                throw new ArgumentNullException(nameof(deviceName));

            if (String.IsNullOrEmpty(appVersion))
                throw new ArgumentNullException(nameof(appVersion));

            UserName = userName;
            DeviceName = deviceName;
            AppVersion = appVersion;
            Date = date;
            Action = action;
            Notes = notes;
        }

        public void UpdateAction(DataActionType action, string notes = null)
        {
            Action |= action;
            Date = DateTime.Now;

            UpdateNotes(notes);
        }

        public void UpdateNotes(String notes)
        {
            if (notes != null)
            {
                if (String.IsNullOrWhiteSpace(Notes))
                    Notes = notes;
                else
                    Notes += $" | {notes}"; 
            }

            Date = DateTime.Now;
        }

        public void Reset()
        {
            Action = DataActionType.None;
            Date = DateTime.Now;
            Notes = String.Empty;
        }
    }

    public class TtUserActionSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TtUserAction xa = x as TtUserAction;
            TtUserAction ya = y as TtUserAction;

            if (xa == null && ya == null)
                return 0;
            else if (xa == null)
                return 1;
            else if (ya == null)
                return -1;

            return xa.Date.CompareTo(ya.Date) * -1;
        }
    }
}
