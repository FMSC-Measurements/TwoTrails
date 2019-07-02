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
    public partial class CsvParseControl : UserControl, IFileParseControl
    {
        public IFileImportModel Model { get; }

        public CsvParseControl(string fileName, int zone, Action<TtCsvDataAccessLayer> onSetup)
        {
            DataContext = (Model = new CsvImportModel(fileName, zone, onSetup));

            InitializeComponent();
        }
    }
}
