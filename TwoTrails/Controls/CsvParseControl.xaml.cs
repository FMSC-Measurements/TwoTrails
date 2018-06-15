using System;
using System.Windows.Controls;
using TwoTrails.DAL;
using TwoTrails.ViewModels;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for CsvImportControl.xaml
    /// </summary>
    public partial class CsvParseControl : UserControl
    {
        public CsvParseControl(string fileName, int zone, Action<TtCsvDataAccessLayer> onSetup)
        {
            DataContext = new CsvImportModel(fileName, zone, onSetup);

            InitializeComponent();
        }
    }
}
