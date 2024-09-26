using FMSC.Core;
using FMSC.Core.ComponentModel;
using System;
using TwoTrails.Core;

namespace TwoTrails.Settings
{
    public class DeviceSettings : BaseModel, IDeviceSettings
    {
        private const string DELETE_EXISTING_PLOTS = "DeleteExistingPlots";
        private const string SPLIT_INTO_INDIVIDUAL_POLYS = "SplitIntoIndividualPolys";
        private const string LOG_DECK_VOLUME = "LogDeckVolume";
        private const string LOG_DECK_DISTANCE = "LogDeckDistance";
        private const string LOG_DECK_LENGTH = "LogDeckLength";
        private const string LOG_DECK_COLLAR_WIDTH = "LogDeckCollarWidth";
        private const string LOG_DECK_DEFECT = "LogDeckDefect";
        private const string LOG_DECK_VOID = "LogDeckVoid";
        private const string DELETE_POINT_WARNING = "DeletePointWarning";

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

        private bool _DeletePointWarning;
        public bool DeletePointWarning
        {
            get { return _DeletePointWarning; }

            set
            {
                SetField(ref _DeletePointWarning, value);
                Properties.Settings.Default[DELETE_POINT_WARNING] = value;
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
                Properties.Settings.Default[LOG_DECK_COLLAR_WIDTH] = value;
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
                Properties.Settings.Default[LOG_DECK_LENGTH] = value;
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
                Properties.Settings.Default[LOG_DECK_DEFECT] = value;
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
                Properties.Settings.Default[LOG_DECK_VOID] = value;
                Properties.Settings.Default.Save();
            }
        }



        public DeviceSettings()
        {
            _DeleteExistingPlots = (bool)Properties.Settings.Default[DELETE_EXISTING_PLOTS];
            _SplitToIndividualPolys = (bool)Properties.Settings.Default[SPLIT_INTO_INDIVIDUAL_POLYS];
            _DeletePointWarning = (bool)Properties.Settings.Default[DELETE_POINT_WARNING];

            _LogDeckCollarWidth = (double)Properties.Settings.Default[LOG_DECK_COLLAR_WIDTH];
            _LogDeckDefect = (double)Properties.Settings.Default[LOG_DECK_DEFECT];
            _LogDeckLength = (double)Properties.Settings.Default[LOG_DECK_LENGTH];
            _LogDeckVoid = (double)Properties.Settings.Default[LOG_DECK_VOID];

            _LogDeckDistance = (Distance)Properties.Settings.Default[LOG_DECK_DISTANCE];
            _LogDeckVolume = (Volume)Properties.Settings.Default[LOG_DECK_VOLUME];
        }
    }
}
