using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Windows.Input;
using TwoTrails.DAL;
using static TwoTrails.DAL.TtGpxDataAccessLayer;

namespace TwoTrails.ViewModels
{
    public class GpxImportModel
    {
        private Action<TtGpxDataAccessLayer> OnSetup { get; }

        public int Zone { get; }

        public ParseOptions Options { get; }
        public ICommand SetupImportCommand { get; }


        public GpxImportModel(string fileName, int zone, Action<TtGpxDataAccessLayer> onSetup)
        {
            Zone = zone;

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
