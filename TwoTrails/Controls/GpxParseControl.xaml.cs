using System;
using System.Windows.Controls;
using TwoTrails.DAL;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for CsvImportControl.xaml
    /// </summary>
    public partial class GpxParseControl : UserControl
    {
        public GpxParseControl(string fileName, int zone, Action<TtGpxDataAccessLayer> onSetup)
        {
            DataContext = new GpxImportModel(fileName, zone, onSetup);

            InitializeComponent();
        }
    }
}
