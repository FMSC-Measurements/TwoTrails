using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Core;

namespace TwoTrails.ViewModels
{
    public class ImportModel : NotifyPropertyChangedEx
    {
        public ICommand BrowseFileCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand CloseCommand { get; }

        private Window _Window;

        public bool IsImporting { get { return Get<bool>(); } set { Set(value); } }

        public string CurrentFile { get { return Get<string>(); } set { Set(value); } }

        public bool CanImport(string fileName)
        {
            return File.Exists(fileName);
        }


        public ImportModel(Window window, ITtManager manager)
        {
            _Window = window;
            CurrentFile = null;

            BrowseFileCommand = new BindedRelayCommand<ImportModel>(x => BrowseFile(), x => !IsImporting,
                this, m => m.IsImporting);

            ImportCommand = new BindedRelayCommand<ImportModel>(x => ImportData(x as string),
                x => CanImport(CurrentFile) && !IsImporting, this, m => new { m.IsImporting, m.CurrentFile });

            CloseCommand = new RelayCommand(x => Close());


        }

        private void BrowseFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = @"TwoTrails files (*.tt;*.tt2)|*.tt; *.tt2|CSV files (*.csv)|*.csv|
Text Files (*.txt)|*.txt|Shape Files (*.shp)|*.shp|GPX Files (*.gpx)|*.gpx|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                CurrentFile = ofd.FileName;
            }
        }

        private void ImportData(string fileName)
        {
            switch (Path.GetExtension(fileName))
            {
                case ".tt":
                    break;
                case ".tt2":
                    break;
                case ".csv":
                case ".text":
                    break;
                case ".gpx":
                    break;
                default:
                    MessageBox.Show("File type not supported.");
                    break;
            }
        }

        private void Close()
        {
            _Window.Close();
        }
    }
}
