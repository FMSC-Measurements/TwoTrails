using FMSC.GeoSpatial.MTDC;
using System;
using System.Windows;
using System.Windows.Controls;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for MtdcDataDialog.xaml
    /// </summary>
    public partial class AccuracyDataDialog : Window
    {
        private AccuracyDataModel _Model;

        public double Accuracy {  get { return _Model.Accuracy; } }
        public String MakeID { get { return _Model.MakeID; } }
        public String ModelID { get { return _Model.ModelID; } }

        public AccuracyDataDialog(GpsAccuracyReport report, double? accuracy, string make = null, string model = null)
        {
            _Model = new AccuracyDataModel(report,accuracy, make, model, this);
            DataContext = _Model;
            InitializeComponent();
        }

        public static bool? ShowDialog(GpsAccuracyReport report, double? accuracy = null, string make = null, string model = null, Window owner = null)
        {
            AccuracyDataDialog diag = new AccuracyDataDialog(report, accuracy, make, model);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count > 0)
            {
                DataGridCellInfo cell = e.AddedCells[0];
                if (cell.Column.DisplayIndex > 4 && cell.Column.DisplayIndex < 8)
                {
                    if (cell.Item is Test test)
                    {
                        if (cell.Column.DisplayIndex == 5 && test.OpenAcc != null)
                            _Model.Accuracy = (double)test.OpenAcc;
                        else if (cell.Column.DisplayIndex == 6 && test.MedAcc != null)
                            _Model.Accuracy = (double)test.MedAcc;
                        else if (cell.Column.DisplayIndex == 7 && test.HeavyAcc != null)
                            _Model.Accuracy = (double)test.HeavyAcc;
                    }
                }
            }
        }
    }
}
