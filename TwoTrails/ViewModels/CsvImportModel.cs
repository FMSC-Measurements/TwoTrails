using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.DAL;

namespace TwoTrails.ViewModels
{
    public class CsvImportModel : NotifyPropertyChangedEx
    {
        private Action<IReadOnlyTtDataLayer, bool> OnSetup { get; }

        private ParseOptions Options { get; }

        public List<string> Fields { get; }

        #region Field Indexes
        public int CN_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int OPTYPE_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int INDEX_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int PID_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int TIME_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int POLY_NAME_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int GROUP_NAME_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int COMMENT_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int META_CN_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int ONBND_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int UNADJX_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int UNADJY_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int UNADJZ_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int ACCURACY_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int MAN_ACC_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int RMSER_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int LATITUDE_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int LONGITUDE_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int ELEVATION_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int FWD_AZ_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int BK_AZ_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int SLOPE_DIST_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int SLOPE_DIST_TYPE_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int SLOPE_ANG_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int SLOPE_ANG_TYPE_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int PARENT_CN_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int POLY_CN_FIELD { get { return Get<int>(); } set { Set(value); } }
        public int GROUP_CN_FIELD { get { return Get<int>(); } set { Set(value); } }
        #endregion

        public bool IsBasicMode { get { return Get<bool>(); } set { Set(value); } }

        public bool IsAdvancedMode { get { return Get<bool>(); } set { Set(value); } }

        public bool IsLatLonMode { get { return Get<bool>(); } set { Set(value); } }

        public ICommand SetupImportCommand { get; }


        public CsvImportModel(string fileName, Action<IReadOnlyTtDataLayer, bool> onSetup)
        {
            OnSetup = onSetup;

            SetupImportCommand = new RelayCommand(x => SetupImport());

            Options = new ParseOptions(fileName);

            Fields = new List<string>();
            Fields.Add("No Field");
            Fields.AddRange(Options.Fields);

            SetupDefaultFields(Options);

            if (Options.PointMapping.ContainsKey(PointTextFieldType.CN))
            {
                IsAdvancedMode = true;
            }
            else
            {
                if (!Options.PointMapping.ContainsKey(PointTextFieldType.UNADJX) &&
                    Options.PointMapping.ContainsKey(PointTextFieldType.LATITUDE) && Options.PointMapping.ContainsKey(PointTextFieldType.LONGITUDE))
                {
                    IsLatLonMode = true;
                }
                else
                {
                    IsBasicMode = true;
                }
            }
        }


        private void SetupDefaultFields(ParseOptions opts)
        {
            foreach (PointTextFieldType ptft in opts.PointMapping.Keys)
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
            TtCsvDataAccessLayer dal = new TtCsvDataAccessLayer(Options);

            OnSetup(dal, dal.GetGroups().Any());
        }
    }
}
