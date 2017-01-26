using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;

namespace TwoTrails
{
    public class DeviceSettings : NotifyPropertyChangedEx, IDeviceSettings
    {
        private const String DELETE_EXISTING_PLOTS = "DeleteExistingPlots";

        private bool _DeleteExistingPlots;
        public bool DeleteExistingPlots
        {
            get { return _DeleteExistingPlots; }

            set
            {
                SetField(ref _DeleteExistingPlots, value);
                Properties.Settings.Default[DELETE_EXISTING_PLOTS] = value;
                Properties.Settings.Default.Save();
            }
        }

        public DeviceSettings()
        {
            _DeleteExistingPlots = (bool)Properties.Settings.Default[DELETE_EXISTING_PLOTS];
        }
    }
}
