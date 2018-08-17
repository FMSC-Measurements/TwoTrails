using CSUtil.ComponentModel;
using System;
using TwoTrails.Core;

namespace TwoTrails
{
    public class DeviceSettings : NotifyPropertyChangedEx, IDeviceSettings
    {
        private const String DELETE_EXISTING_PLOTS = "DeleteExistingPlots";
        private const String SPLIT_INTO_INDIVIDUAL_POLYS = "SplitIntoIndividualPolys";

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

        private bool _SplitToIndividualPolys;
        public bool SplitToIndividualPolys
        {
            get { return _DeleteExistingPlots; }

            set
            {
                SetField(ref _SplitToIndividualPolys, value);
                Properties.Settings.Default[DELETE_EXISTING_PLOTS] = value;
                Properties.Settings.Default.Save();
            }
        }

        public DeviceSettings()
        {
            _DeleteExistingPlots = (bool)Properties.Settings.Default[DELETE_EXISTING_PLOTS];
            _SplitToIndividualPolys = (bool)Properties.Settings.Default[SPLIT_INTO_INDIVIDUAL_POLYS];
        }
    }
}
