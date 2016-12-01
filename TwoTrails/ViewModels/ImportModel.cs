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
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Utils;

namespace TwoTrails.ViewModels
{
    public class ImportModel : NotifyPropertyChangedEx
    {
        public ICommand BrowseFileCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand CloseCommand { get; }

        private Window _Window;
        private ITtManager _Manager;

        public Control MainContent { get { return Get<Control>(); } set { Set(value, () => OnPropertyChanged(nameof(HasMainContent))); } }
        public Visibility HasMainContent { get { return MainContent != null ? Visibility.Visible : Visibility.Collapsed; } }


        private ImportControl _ImportControl;
        private ImportControl ImportControl
        {
            get { return _ImportControl; }
            set
            {
                _ImportControl = value;
                if (_ImportControl != null)
                    _ImportControl.PolygonSelectionChanged += (Object sender, EventArgs e) =>
                        OnPropertyChanged(nameof(CanImport));
            }
        }

        public bool IsImporting { get { return Get<bool>(); } set { Set(value); } }

        public string CurrentFile { get { return Get<string>(); } set { Set(value); } }

        public bool CanImport(string fileName)
        {
            return File.Exists(fileName) && ImportControl != null && ImportControl.HasSelectedPolygons && !IsImporting;
        }


        public ImportModel(Window window, ITtManager manager)
        {
            _Window = window;
            _Manager = manager;
            MainContent = null;
            CurrentFile = null;

            BrowseFileCommand = new BindedRelayCommand<ImportModel>(x => BrowseFile(), x => !IsImporting,
                this, m => m.IsImporting);

            ImportCommand = new BindedRelayCommand<ImportModel>(x => ImportData(),
                x => CanImport(CurrentFile), this, m => new { m.IsImporting, m.MainContent });

            CloseCommand = new RelayCommand(x => Close());
        }

        private void BrowseFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = @"Importable Files|*.tt; *.tt2; *.csv; *.txt; *.shp; *.gpx|TwoTrails files (*.tt;*.tt2)|*.tt; *.tt2|
CSV files (*.csv)|*.csv|Text Files (*.txt)|*.txt|Shape Files (*.shp)|*.shp|GPX Files (*.gpx)|*.gpx|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                CurrentFile = ofd.FileName;
                SetupImport(CurrentFile);
            }
        }

        public void SetupImport(string fileName)
        {
            switch (Path.GetExtension(fileName))
            {
                case ".tt":
                    ImportControl = new ImportControl(new TtSqliteDataAccessLayer(fileName), true, true, true);
                    MainContent = ImportControl;
                    break;
                case ".tt2":
                    ImportControl = new ImportControl(new TtV2SqliteDataAccessLayer(fileName), true, true, true);
                    MainContent = ImportControl;
                    break;
                case ".csv":
                case ".text":
                    MainContent = new CsvParseControl(fileName, _Manager.DefaultMetadata.Zone, (dal) =>
                    {
                        //TODO show progress indicator while parsing
                        //MainContent = new WaitCursorControl; ??
                        bool hasGroups = dal.GetGroups().Any();

                        //hide progress indicator

                        ImportControl = new ImportControl(dal, false, hasGroups, false);
                        MainContent = ImportControl;
                    });
                    break;
                case ".gpx":
                    break;
                case ".shp":
                    break;
                default:
                    MessageBox.Show("File type not supported.");
                    break;
            }
        }

        public void ImportData()
        {
            Import.DAL(_Manager, ImportControl.DAL, ImportControl.SelectedPolygons,
                ImportControl.IncludeMetadata, ImportControl.IncludeGroups, ImportControl.IncludeNmea);
        }

        private void Close()
        {
            _Window.Close();
        }
    }
}
