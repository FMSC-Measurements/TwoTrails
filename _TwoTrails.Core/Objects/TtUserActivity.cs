using System;
namespace TwoTrails.Core
{
    public class TtUserActivity
    {
        public String UserName { get; private set; }
        public String DeviceName { get; private set; }
        public DateTime Date { get; private set; }
        public DataActivityType Activity { get; private set; }

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
