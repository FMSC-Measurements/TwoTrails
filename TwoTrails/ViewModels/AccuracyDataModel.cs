using CSUtil.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using FMSC.GeoSpatial.MTDC;
using System;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.ViewModels
{
    public class AccuracyDataModel : NotifyPropertyChangedEx
    {
        public GpsAccuracyReport Report { get; }

        public double Accuracy { get { return Get<double>(); } set { Set(value); } }
        
        public String MakeID { get { return Get<string>(); } set { Set(value); } }
        public String ModelID { get { return Get<string>(); } set { Set(value); } }

        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FtToMtCommand { get; }

        public int StartupSelectedTabIndex { get; }
        public bool HasGpsAccuracyReport { get; }

        public AccuracyDataModel(GpsAccuracyReport report, double? accuracy, string make, string model, Window window)
        {
            if (report == null)
            {
                StartupSelectedTabIndex = 1;
                HasGpsAccuracyReport = false;

                MessageBox.Show("Unable to get MTDC Accuracy Reports.");
            }
            else
            {
                StartupSelectedTabIndex = 0;
                HasGpsAccuracyReport = true;
            }

            Report = report;
            Accuracy = accuracy ?? Consts.DEFAULT_POINT_ACCURACY;

            MakeID = make;
            ModelID = model;

            OkCommand = new BindedRelayCommand<AccuracyDataModel>(
                x => { window.DialogResult = true; window.Close(); },
                x => Accuracy > 0, this, x => x.Accuracy);

            CancelCommand = new RelayCommand(x => window.Close());

            FtToMtCommand = new RelayCommand(x =>
            {
                if (x is double val)
                    Accuracy = val;
            });
        }
    }
}
