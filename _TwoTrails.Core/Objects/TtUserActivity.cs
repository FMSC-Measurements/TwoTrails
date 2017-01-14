using System;
namespace TwoTrails.Core
{
    public class TtUserActivity
    {
        public String UserName { get; private set; }
        public String DeviceName { get; private set; }
        public DateTime Date { get; private set; }
        public DataActivityType Activity { get; private set; }
        
        public bool ProjectModified { get { return Activity.HasFlag(DataActivityType.ModifiedProject); } }
        public bool PointsInserted { get { return Activity.HasFlag(DataActivityType.InsertedPoints); } }
        public bool PointsModified { get { return Activity.HasFlag(DataActivityType.ModifiedPoints); } }
        public bool PointsDeleted { get { return Activity.HasFlag(DataActivityType.DeletedPoints); } }
        public bool PolygonsInserted { get { return Activity.HasFlag(DataActivityType.InsertedPolygons); } }
        public bool PolygonsModified { get { return Activity.HasFlag(DataActivityType.ModifiedPolygons); } }
        public bool PolygonsDeleted { get { return Activity.HasFlag(DataActivityType.DeletedPolygons); } }
        public bool MetadataInserted { get { return Activity.HasFlag(DataActivityType.InsertedMetadata); } }
        public bool MetadataModified { get { return Activity.HasFlag(DataActivityType.ModifiedMetadata); } }
        public bool MetadataDeleted { get { return Activity.HasFlag(DataActivityType.DeletedMetadata); } }
        public bool GroupsInserted { get { return Activity.HasFlag(DataActivityType.InsertedGroups); } }
        public bool GroupsModified { get { return Activity.HasFlag(DataActivityType.ModifiedGroups); } }
        public bool GroupsDeleted { get { return Activity.HasFlag(DataActivityType.DeletedGroups); } }

        public TtUserActivity(String userName, String deviceName) :
            this(userName, deviceName, DateTime.Now, DataActivityType.Opened)
        { }

        public TtUserActivity(String userName, String deviceName,
            DateTime date, DataActivityType activity)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            if (String.IsNullOrEmpty(deviceName))
                throw new ArgumentNullException(nameof(deviceName));

            UserName = userName;
            DeviceName = deviceName;
            Date = date;
            Activity = activity;
        }

        public void UpdateActivity(DataActivityType activity)
        {
            Activity |= activity;
            Date = DateTime.Now;
        }

        public void Reset()
        {
            Activity = DataActivityType.Opened;
            Date = DateTime.Now;
        }
    }
}
