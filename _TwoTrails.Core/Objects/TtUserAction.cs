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
        
        public bool ProjectModified { get { return Action.HasFlag(DataActionType.ModifiedProject); } }

        public bool PointsInserted { get { return Action.HasFlag(DataActionType.InsertedPoints); } }
        public bool PointsModified { get { return Action.HasFlag(DataActionType.ModifiedPoints); } }
        public bool PointsDeleted { get { return Action.HasFlag(DataActionType.DeletedPoints); } }

        public bool PolygonsInserted { get { return Action.HasFlag(DataActionType.InsertedPolygons); } }
        public bool PolygonsModified { get { return Action.HasFlag(DataActionType.ModifiedPolygons); } }
        public bool PolygonsDeleted { get { return Action.HasFlag(DataActionType.DeletedPolygons); } }

        public bool MetadataInserted { get { return Action.HasFlag(DataActionType.InsertedMetadata); } }
        public bool MetadataModified { get { return Action.HasFlag(DataActionType.ModifiedMetadata); } }
        public bool MetadataDeleted { get { return Action.HasFlag(DataActionType.DeletedMetadata); } }

        public bool GroupsInserted { get { return Action.HasFlag(DataActionType.InsertedGroups); } }
        public bool GroupsModified { get { return Action.HasFlag(DataActionType.ModifiedGroups); } }
        public bool GroupsDeleted { get { return Action.HasFlag(DataActionType.DeletedGroups); } }

        public bool MediaInserted { get { return Action.HasFlag(DataActionType.InsertedMedia); } }
        public bool MediaModified { get { return Action.HasFlag(DataActionType.ModifiedMedia); } }
        public bool MediaDeleted { get { return Action.HasFlag(DataActionType.DeletedMedia); } }

        public bool ManualCreatedPoints { get { return Action.HasFlag(DataActionType.ManualPointCreation); } }
        public bool PointsMoved { get { return Action.HasFlag(DataActionType.MovePoints); } }
        public bool PointsRetraced { get { return Action.HasFlag(DataActionType.RetracePoints); } }
        public bool PointsReindexed { get { return Action.HasFlag(DataActionType.ReindexPoints); } }
        public bool PointsConverted { get { return Action.HasFlag(DataActionType.ConvertPoints); } }
        
        public bool DataImported { get { return Action.HasFlag(DataActionType.DataImported); } }
        public bool ModifiedDataDictionary { get { return Action.HasFlag(DataActionType.ModifiedDataDictionary); } }

        public TtUserAction(String userName, String deviceName) :
            this(userName, deviceName, DateTime.Now, DataActionType.None)
        { }

        public TtUserAction(String userName, String deviceName,
            DateTime date, DataActionType action, String notes = null)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            if (String.IsNullOrEmpty(deviceName))
                throw new ArgumentNullException(nameof(deviceName));

            UserName = userName;
            DeviceName = deviceName;
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
                if (Notes == null)
                    Notes = notes;
                else
                    Notes += $"|{notes}"; 
            }
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
