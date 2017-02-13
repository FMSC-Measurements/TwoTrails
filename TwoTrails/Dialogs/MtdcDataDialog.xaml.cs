using FMSC.Core.ComponentModel.Commands;
using FMSC.GeoSpatial.MTDC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TwoTrails.Core;
using TwoTrails.ViewModels;

namespace TwoTrails.Dialogs
{
    /// <summary>
    /// Interaction logic for MtdcDataDialog.xaml
    /// </summary>
    public partial class MtdcDataDialog : Window
    {
        private MtdcDataModel _Model;

        public double Accuracy {  get { return _Model.Accuracy; } }

        public MtdcDataDialog(GpsAccuracyReport report, bool hasWASS, bool canSelectValue, int make = 0, int model = 0)
        {
            _Model = new MtdcDataModel(report, hasWASS, canSelectValue, make, model, this);
            DataContext = _Model;
            InitializeComponent();
        }

        public static bool? ShowDialog(GpsAccuracyReport report, bool hasWASS, int make = 0, int model = 0, Window owner = null)
        {
            MtdcDataDialog diag = new MtdcDataDialog(report, hasWASS, false, make, model);
            if (owner != null)
                diag.Owner = owner;
            return diag.ShowDialog();
        }
    }
}
