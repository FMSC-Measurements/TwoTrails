using FMSC.Core.ComponentModel.Commands;
using FMSC.GeoSpatial.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.DAL;
using static TwoTrails.DAL.TtGpxDataAccessLayer;

namespace TwoTrails.ViewModels
{
    public class GpxImportModel
    {
        private Action<TtGpxDataAccessLayer> OnSetup { get; }

        public ParseOptions Options { get; }
        public ICommand SetupImportCommand { get; }


        public GpxImportModel(string fileName, int zone, Action<TtGpxDataAccessLayer> onSetup)
        {
            OnSetup = onSetup;

            SetupImportCommand = new RelayCommand(x => SetupImport());

            Options = new ParseOptions(fileName, zone);
        }


        private void SetupImport()
        {
            OnSetup(new TtGpxDataAccessLayer(Options));
        }
    }
}
