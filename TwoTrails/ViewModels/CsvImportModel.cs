using CSUtil.ComponentModel;
using FMSC.Core;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using TwoTrails.Core.Interfaces;
using TwoTrails.DAL;
using static TwoTrails.DAL.TtCsvDataAccessLayer;

namespace TwoTrails.ViewModels
{
    public class CsvImportModel : NotifyPropertyChangedEx, IFileImportModel
    {
        private Action<TtCsvDataAccessLayer> OnSetup { get; }

        private ParseOptions Options { get; }

        public List<string> Fields { get; }

        #region Field Indexes
        public int CN_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.CN, value)); } }
        public int OPTYPE_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.OPTYPE, value)); } }
        public int INDEX_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.INDEX, value)); } }
        public int PID_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.PID, value)); } }
        public int TIME_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.TIME, value)); } }
        public int POLY_NAME_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.POLY_NAME, value)); } }
        public int GROUP_NAME_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.GROUP_NAME, value)); } }
        public int COMMENT_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.COMMENT, value)); } }
        public int META_CN_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.META_CN, value)); } }
        public int ONBND_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.ONBND, value)); } }
        public int UNADJX_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.UNADJX, value)); } }
        public int UNADJY_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.UNADJY, value)); } }
        public int UNADJZ_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.UNADJZ, value)); } }
        public int ACCURACY_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.ACCURACY, value)); } }
        public int MAN_ACC_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.MAN_ACC, value)); } }
        public int RMSER_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.RMSER, value)); } }
        public int LATITUDE_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.LATITUDE, value)); } }
        public int LONGITUDE_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.LONGITUDE, value)); } }
        public int ELEVATION_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.ELEVATION, value)); } }
        public int FWD_AZ_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.FWD_AZ, value)); } }
        public int BK_AZ_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.BK_AZ, value)); } }
        public int SLOPE_DIST_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.SLOPE_DIST, value)); } }
        public int SLOPE_DIST_TYPE_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.SLOPE_DIST_TYPE, value)); } }
        public int SLOPE_ANG_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.SLOPE_ANG, value)); } }
        public int SLOPE_ANG_TYPE_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.SLOPE_ANG_TYPE, value)); } }
        public int PARENT_CN_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.PARENT_CN, value)); } }
        public int POLY_CN_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.POLY_CN, value)); } }
        public int GROUP_CN_FIELD { get { return Get<int>(); } set { Set(value, () => EditPointMap(PointTextFieldType.GROUP_CN, value)); } }
        #endregion

        public int Zone { get; }

        public ParseMode Mode {
            get { return Get<ParseMode>(); }
            set
            {
                Set(value, () =>
                {
                    OnPropertyChanged(nameof(CanImport));
                    Options.Mode = value;
                });
            }
        }

        public ICommand SetupImportCommand { get; }
        

        public bool TravDistanceOverride
        {
            get { return Options.TravDistanceOverride; }
            set
            {
                if (value != Options.TravDistanceOverride)
                {
                    Options.TravDistanceOverride = value;
                    OnPropertyChanged(nameof(TravDistanceOverride), nameof(SupportsAdvanced), nameof(CanImport));
                }
            }
        }
        public Distance TravDistance
        {
            get { return Options.TravDistance; }
            set
            {
                if (value != Options.TravDistance)
                {
                    Options.TravDistance = value;
                    OnPropertyChanged(nameof(TravDistance));
                }
            }
        }


        public bool TravSlopeOverride
        {
            get { return Options.TravSlopeOverride; }
            set
            {
                if (value != Options.TravSlopeOverride)
                {
                    Options.TravSlopeOverride = value;
                    OnPropertyChanged(nameof(TravSlopeOverride), nameof(SupportsAdvanced), nameof(CanImport));
                }
            }
        }
        public Slope TravSlope
        {
            get { return Options.TravSlope; }
            set
            {
                if (value != Options.TravSlope)
                {
                    Options.TravSlope = value;
                    OnPropertyChanged(nameof(TravSlope));
                }
            }
        }


        public bool SupportsBasic
        {
            get
            {
                return UNADJX_FIELD > 0 && UNADJY_FIELD > 0;
            }
        }

        public bool SupportsLatLon
        {
            get
            {
                return LATITUDE_FIELD > 0 && LONGITUDE_FIELD > 0;
            }
        }

        public bool SupportsAdvanced
        {
            get
            {
                return SupportsBasic && OPTYPE_FIELD > 0 && INDEX_FIELD > 0 &&
                    (FWD_AZ_FIELD > 0 || BK_AZ_FIELD > 0) &&
                    SLOPE_DIST_FIELD > 0 && SLOPE_DIST_TYPE_FIELD > 0 &&
                    SLOPE_ANG_FIELD > 0 && SLOPE_ANG_TYPE_FIELD > 0 &&
                    PARENT_CN_FIELD > 0;
            }
        }

        public bool CanImport
        {
            get
            {
                switch (Mode)
                {
                    case ParseMode.Basic: return SupportsBasic;
                    case ParseMode.Advanced: return SupportsAdvanced;
                    case ParseMode.LatLon: return SupportsLatLon;
                }

                return false;
            }
        }



        public CsvImportModel(string fileName, int zone, Action<TtCsvDataAccessLayer> onSetup, int startPolyNumber = 0)
        {
            Zone = zone;

            OnSetup = onSetup;

            SetupImportCommand = new BindedRelayCommand<CsvImportModel>(
                x => SetupImport(), x => CanImport, this, m => m.CanImport);

            //SetupImportCommand = new BindedRelayCommand<CsvImportModel>(
            //    (x, m) => m.SetupImport(), (x, m) => m.CanImport, this, m => m.CanImport);

            Options = new ParseOptions(fileName, zone, startPolyNumber: startPolyNumber);

            Fields = new List<string>();
            Fields.Add("No Field");
            Fields.AddRange(Options.Fields);

            SetupDefaultFields(Options);

            if (Options.PointMapping.ContainsKey(PointTextFieldType.CN))
            {
                Mode = ParseMode.Advanced;
            }
            else
            {
                if (!Options.PointMapping.ContainsKey(PointTextFieldType.UNADJX) &&
                    Options.PointMapping.ContainsKey(PointTextFieldType.LATITUDE) && Options.PointMapping.ContainsKey(PointTextFieldType.LONGITUDE))
                {
                    Mode = ParseMode.LatLon;
                }
                else
                {
                    Mode = ParseMode.Basic;
                }
            }
        }


        private void SetupDefaultFields(ParseOptions opts)
        {
            foreach (PointTextFieldType ptft in opts.PointMapping.Keys.ToArray())
            {
                int findex = opts.PointMapping[ptft] + 1;
                switch (ptft)
                {
                    case PointTextFieldType.CN:
                        CN_FIELD = findex;
                        break;
                    case PointTextFieldType.OPTYPE:
                        OPTYPE_FIELD = findex;
                        break;
                    case PointTextFieldType.INDEX:
                        INDEX_FIELD = findex;
                        break;
                    case PointTextFieldType.PID:
                        PID_FIELD = findex;
                        break;
                    case PointTextFieldType.TIME:
                        TIME_FIELD = findex;
                        break;
                    case PointTextFieldType.POLY_NAME:
                        POLY_NAME_FIELD = findex;
                        break;
                    case PointTextFieldType.GROUP_NAME:
                        GROUP_NAME_FIELD = findex;
                        break;
                    case PointTextFieldType.COMMENT:
                        COMMENT_FIELD = findex;
                        break;
                    case PointTextFieldType.META_CN:
                        META_CN_FIELD = findex;
                        break;
                    case PointTextFieldType.ONBND:
                        ONBND_FIELD = findex;
                        break;
                    case PointTextFieldType.UNADJX:
                        UNADJX_FIELD = findex;
                        break;
                    case PointTextFieldType.UNADJY:
                        UNADJY_FIELD = findex;
                        break;
                    case PointTextFieldType.UNADJZ:
                        UNADJZ_FIELD = findex;
                        break;
                    case PointTextFieldType.ACCURACY:
                        ACCURACY_FIELD = findex;
                        break;
                    case PointTextFieldType.MAN_ACC:
                        MAN_ACC_FIELD = findex;
                        break;
                    case PointTextFieldType.RMSER:
                        RMSER_FIELD = findex;
                        break;
                    case PointTextFieldType.LATITUDE:
                        LATITUDE_FIELD = findex;
                        break;
                    case PointTextFieldType.LONGITUDE:
                        LONGITUDE_FIELD = findex;
                        break;
                    case PointTextFieldType.ELEVATION:
                        ELEVATION_FIELD = findex;
                        break;
                    case PointTextFieldType.FWD_AZ:
                        FWD_AZ_FIELD = findex;
                        break;
                    case PointTextFieldType.BK_AZ:
                        BK_AZ_FIELD = findex;
                        break;
                    case PointTextFieldType.SLOPE_DIST:
                        SLOPE_DIST_FIELD = findex;
                        break;
                    case PointTextFieldType.SLOPE_DIST_TYPE:
                        SLOPE_DIST_TYPE_FIELD = findex;
                        break;
                    case PointTextFieldType.SLOPE_ANG:
                        SLOPE_ANG_FIELD = findex;
                        break;
                    case PointTextFieldType.SLOPE_ANG_TYPE:
                        SLOPE_ANG_TYPE_FIELD = findex;
                        break;
                    case PointTextFieldType.PARENT_CN:
                        PARENT_CN_FIELD = findex;
                        break;
                    case PointTextFieldType.POLY_CN:
                        POLY_CN_FIELD = findex;
                        break;
                    case PointTextFieldType.GROUP_CN:
                        GROUP_CN_FIELD = findex;
                        break;
                }
            }
        }

        private void SetupImport()
        {
            OnSetup(new TtCsvDataAccessLayer(Options));
        }


        private void EditPointMap(PointTextFieldType field, int value)
        {
            Options.EditPointMap(field, value - 1);

            if (field == PointTextFieldType.LATITUDE || field == PointTextFieldType.LONGITUDE)
                OnPropertyChanged(nameof(SupportsLatLon));

            if (field == PointTextFieldType.UNADJX || field == PointTextFieldType.UNADJY)
                OnPropertyChanged(nameof(SupportsBasic));

            OnPropertyChanged(nameof(SupportsAdvanced), nameof(CanImport));
        }
    }
}
