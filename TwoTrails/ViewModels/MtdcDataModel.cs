using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using FMSC.GeoSpatial.MTDC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.ViewModels
{
    public class MtdcDataModel : NotifyPropertyChangedEx
    {
        public GpsAccuracyReport Report { get; }

        public double Accuracy { get { return Get<double>(); } private set { Set(value); } }
        
        public bool CanSelectValue { get; }

        public ICommand OkCommand { get; }

        

        public MtdcDataModel(GpsAccuracyReport report, bool hasWASS, bool canSelectValue, int make, int model, Window window)
        {
            Report = report;
            CanSelectValue = canSelectValue;
            Accuracy = Consts.DEFAULT_POINT_ACCURACY;

            OkCommand = new BindedRelayCommand<MtdcDataModel>(
                x => { window.DialogResult = true; window.Close(); },
                x => Accuracy > 0, this, x => x.Accuracy);
        }
    }
}
