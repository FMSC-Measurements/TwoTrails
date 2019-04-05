using System;
using System.Windows.Controls;
using TwoTrails.Core.Interfaces;
using TwoTrails.DAL;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for CsvImportControl.xaml
    /// </summary>
    public partial class GpxParseControl : UserControl, IFileParseControl
    {
        public IFileImportModel Model { get; }

        public GpxParseControl(string fileName, int zone, Action<TtGpxDataAccessLayer> onSetup)
        {
            DataContext = (Model = new GpxImportModel(fileName, zone, onSetup));

            InitializeComponent();
        }
    }
}
