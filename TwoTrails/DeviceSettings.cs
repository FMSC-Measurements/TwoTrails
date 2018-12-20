using CSUtil.ComponentModel;
using FMSC.Core;
using System;
using TwoTrails.Core;

namespace TwoTrails
{
    public class DeviceSettings : NotifyPropertyChangedEx, IDeviceSettings
    {
        private const String DELETE_EXISTING_PLOTS = "DeleteExistingPlots";
        private const String SPLIT_INTO_INDIVIDUAL_POLYS = "SplitIntoIndividualPolys";
        private const String LOG_DECK_VOLUME = "LogDeckVolume";
        private const String LOG_DECK_DISTANCE = "LogDeckDistance";
        private const String LOG_DECK_LENGTH = "LogDeckLength";
        private const String LOG_DECK_COLLAR_WIDTH = "LogDeckCollarWidth";
        private const String LOG_DECK_DEFECT = "LogDeckDefect";
        private const String LOG_DECK_VOID = "LogDeckVoid";

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
            get { return _SplitToIndividualPolys; }

            set
            {
                SetField(ref _SplitToIndividualPolys, value);
                Properties.Settings.Default[SPLIT_INTO_INDIVIDUAL_POLYS] = value;
                Properties.Settings.Default.Save();
            }
        }

        private Volume _LogDeckVolume;
        public Volume LogDeckVolume
        {
            get { return _LogDeckVolume; }

            set
            {
                SetField(ref _LogDeckVolume, value);
                Properties.Settings.Default[LOG_DECK_VOLUME] = (int)value;
                Properties.Settings.Default.Save();
            }
        }

        private Distance _LogDeckDistance;
        public Distance LogDeckDistance
        {
            get { return _LogDeckDistance; }

            set
            {
                SetField(ref _LogDeckDistance, value);
                Properties.Settings.Default[LOG_DECK_DISTANCE] = (int)value;
                Properties.Settings.Default.Save();
            }
        }


        private double _LogDeckCollarWidth;
        public double LogDeckCollarWidth
        {
            get { return _LogDeckCollarWidth; }

            set
            {
                SetField(ref _LogDeckCollarWidth, value);
                Properties.Settings.Default[LOG_DECK_COLLAR_WIDTH] = (double)value;
                Properties.Settings.Default.Save();
            }
        }


        private double _LogDeckLength;
        public double LogDeckLength
        {
            get { return _LogDeckLength; }

            set
            {
                SetField(ref _LogDeckLength, value);
                Properties.Settings.Default[LOG_DECK_LENGTH] = (double)value;
                Properties.Settings.Default.Save();
            }
        }

        private double _LogDeckDefect;
        public double LogDeckDefect
        {
            get { return _LogDeckDefect; }

            set
            {
                SetField(ref _LogDeckDefect, value);
                Properties.Settings.Default[LOG_DECK_DEFECT] = (double)value;
                Properties.Settings.Default.Save();
            }
        }


        private double _LogDeckVoid;
        public double LogDeckVoid
        {
            get { return _LogDeckVoid; }

            set
            {
                SetField(ref _LogDeckVoid, value);
                Properties.Settings.Default[LOG_DECK_VOID] = (double)value;
                Properties.Settings.Default.Save();
            }
        }

        public DeviceSettings()
        {
            _DeleteExistingPlots = (bool)Properties.Settings.Default[DELETE_EXISTING_PLOTS];
            _SplitToIndividualPolys = (bool)Properties.Settings.Default[SPLIT_INTO_INDIVIDUAL_POLYS];

            _LogDeckCollarWidth = (double)Properties.Settings.Default[LOG_DECK_COLLAR_WIDTH];
            _LogDeckDefect = (double)Properties.Settings.Default[LOG_DECK_DEFECT];
            _LogDeckLength = (double)Properties.Settings.Default[LOG_DECK_LENGTH];
            _LogDeckVoid = (double)Properties.Settings.Default[LOG_DECK_VOID];

            _LogDeckDistance = (Distance)Properties.Settings.Default[LOG_DECK_DISTANCE];
            _LogDeckVolume = (Volume)Properties.Settings.Default[LOG_DECK_VOLUME];

        }
    }
}
