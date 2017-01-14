using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ICommand CancelCommand { get; }

        private Window _Window;
        private ITtManager _Manager;

        public Control MainContent { get { return Get<Control>(); } set { Set(value, () => OnPropertyChanged(nameof(HasMainContent), nameof(CloseText))); } }
        public Visibility HasMainContent { get { return MainContent != null ? Visibility.Visible : Visibility.Collapsed; } }

        public string CloseText { get { return MainContent == null ? "Close" : "Cancel"; } }


        private ImportControl _ImportControl;
        private ImportControl ImportControl
        {
            get { return _ImportControl; }
            set
            {
                SetField(ref _ImportControl, value, () =>
                {
                    if (_ImportControl != null)
                        _ImportControl.PolygonSelectionChanged += (Object sender, EventArgs e) =>
                            OnPropertyChanged(nameof(CanImport));
                    OnPropertyChanged(nameof(CanImport));
                    MainContent = _ImportControl;
                });
            }
        }

        public bool IsImporting { get { return Get<bool>(); } set { Set(value, () => OnPropertyChanged(nameof(CanImport))); } }
        public bool IsSettingUp { get { return Get<bool>(); } set { Set(value); } }

        public string CurrentFile { get { return Get<string>(); } set { Set(value); } }

        public bool CanImport { get { return ImportControl != null && ImportControl.HasSelectedPolygons && !IsImporting; } }


        public ImportModel(Window window, ITtManager manager)
        {
            _Window = window;
            _Manager = manager;
            MainContent = null;
            CurrentFile = null;

            BrowseFileCommand = new BindedRelayCommand<ImportModel>(x => BrowseFile(), x => !IsImporting,
                this, m => m.IsImporting);

            ImportCommand = new BindedRelayCommand<ImportModel>(x => ImportData(), x => CanImport, this, m => m.CanImport);

            CancelCommand = new RelayCommand(x => Cancel());
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
                    IsSettingUp = true;
                    ImportControl = new ImportControl(new TtSqliteDataAccessLayer(fileName), true, true, true);
                    break;
                case ".tt2":
                    IsSettingUp = true;
                    ImportControl = new ImportControl(new TtV2SqliteDataAccessLayer(fileName), true, true, true);
                    break;
                case ".csv":
                case ".text":
                    IsSettingUp = true;
                    MainContent = new CsvParseControl(fileName, _Manager.DefaultMetadata.Zone, (dal) =>
                    {
                        try
                        {
                            //TODO show progress indicator while parsing
                            //MainContent = new WaitCursorControl; ??
                            dal.Parse();
                            //hide progress indicator

                            ImportControl = new ImportControl(dal, false, dal.GetGroups().Any(), false);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message, "ImportModel:SetupImport:CSV");
                            MessageBox.Show(String.Format("A parsing error has occured. Please check your fields correctly. See log file for details."),
                                "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                    break;
                case ".gpx":
                    IsSettingUp = true;
                    MainContent = new GpxParseControl(fileName, _Manager.DefaultMetadata.Zone, (dal) =>
                    {
                        try
                        {
                            //TODO show progress indicator while parsing
                            //MainContent = new WaitCursorControl; ??
                            dal.Parse();
                            //hide progress indicator

                            ImportControl = new ImportControl(dal, false, false, false);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message, "ImportModel:SetupImport:GPX");
                            MessageBox.Show(String.Format("A parsing error has occured. Please check your fields correctly. See log file for details."),
                                "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                    break;
                case ".shp":
                    IsSettingUp = true;
                    break;
                default:
                    MessageBox.Show("File type not supported.");
                    break;
            }
        }

        public void ImportData()
        {
            IsSettingUp = false;
            IsImporting = true;

            try
            {
                Import.DAL(_Manager, ImportControl.DAL, ImportControl.SelectedPolygons,
                        ImportControl.IncludeMetadata, ImportControl.IncludeGroups, ImportControl.IncludeNmea);

                MessageBox.Show(String.Format("{0} Polygons Imported", ImportControl.SelectedPolygons.Count()),
                    String.Empty, MessageBoxButton.OK, MessageBoxImage.None);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message, "ImportModel:ImportData");
                MessageBox.Show("Import Failed. See log file for details.", String.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            IsImporting = false;
            ImportControl = null;
        }

        private void Cancel()

        {
            if (MainContent != null)
            {
                ImportControl = null;
                MainContent = null;
                IsSettingUp = false;
                IsImporting = false;
            }
            else
                _Window.Close();
        }
    }
}
