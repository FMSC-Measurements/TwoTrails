using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using FMSC.Core.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
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
            ofd.Multiselect = true;
            ofd.Filter = $@"Importable Files|*{Consts.FILE_EXTENSION}; *{Consts.FILE_EXTENSION_V2}; *.csv; *.txt; *.shp; *.gpx; *.kml; *.kmz|
TwoTrails files (*{Consts.FILE_EXTENSION};*{Consts.FILE_EXTENSION_V2})|*{Consts.FILE_EXTENSION}; *{Consts.FILE_EXTENSION_V2}|
CSV files (*.csv)|*.csv|Text Files (*.txt)|*.txt|Shape Files (*.shp)|*.shp|GPX Files (*.gpx)|*.gpx|Google Earth Files (*.kml *.kmz)|*.kml; *.kmz|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileNames.Length > 1)
                {
                    foreach (string fn in ofd.FileNames)
                    {
                        if (!fn.EndsWith(".shp"))
                        {
                            MessageBox.Show("Only Shape Files can be imported in bulk.", "Import Files", MessageBoxButton.OK, MessageBoxImage.Stop);
                            return;
                        }
                    }

                    SetupShapeFiles(ofd.FileNames);
                }
                else
                {
                    CurrentFile = ofd.FileName;
                    SetupImport(CurrentFile);
                }
            }
        }

        public void SetupImport(string fileName)
        {
            switch (Path.GetExtension(fileName))
            {
                case Consts.FILE_EXTENSION:
                    IsSettingUp = true;
                    ImportControl = new ImportControl(new TtSqliteDataAccessLayer(fileName), true, true, true);
                    break;
                case Consts.FILE_EXTENSION_V2:
                    IsSettingUp = true;
                    ImportControl = new ImportControl(new TtV2SqliteDataAccessLayer(fileName), true, true, true);
                    break;
                case ".csv":
                case ".txt":
                    IsSettingUp = true;
                    MainContent = new CsvParseControl(fileName, _Manager.DefaultMetadata.Zone, (dal) =>
                    {
                        try
                        {
                            MainContent = new TtProgressControl();

                            Task.Run(() =>
                            {
                                dal.Parse();
                                MainContent.Dispatcher.Invoke(() =>
                                {
                                    ImportControl = new ImportControl(dal, false, dal.GetGroups().Any(), false);
                                });
                            });
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message, "ImportModel:SetupImport:CSV");
                            MessageBox.Show("A parsing error has occured. Please check your fields correctly. See log file for details.",
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
                            MainContent = new TtProgressControl();

                            Task.Run(() =>
                            {
                                dal.Parse();
                                MainContent.Dispatcher.Invoke(() =>
                                {
                                    ImportControl = new ImportControl(dal, false, false, false);
                                });
                            });
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message, "ImportModel:SetupImport:GPX");
                            MessageBox.Show("A parsing error has occured. Please check your fields correctly. See log file for details.",
                                "Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                    break;
                case ".kml":
                case ".kmz":
                    {
                        if (fileName.EndsWith(".kmz"))
                        {
                            string tempPath = Path.GetTempPath();

                            ZipFile.ExtractToDirectory(fileName, tempPath);
                            
                            foreach (string file in Directory.GetFiles(tempPath))
                            {
                                if (file.EndsWith(".kml"))
                                {
                                    fileName = file;
                                    break;
                                }
                            }

                            if (!fileName.EndsWith(".kml"))
                            {
                                MessageBox.Show("Unable to extract KML from KMZ", "KMZ Extract Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        
                        ImportControl = new ImportControl(new TtKmlDataAccessLayer(fileName));

                        IsSettingUp = true;
                        break;
                    }
                case ".shp":
                    ImportControl = new ImportControl(
                        new TtShapeFileDataAccessLayer(
                            new TtShapeFileDataAccessLayer.ParseOptions(fileName, _Manager.DefaultMetadata.Zone)
                        )
                    );
                    IsSettingUp = true;
                    break;
                default:
                    MessageBox.Show("File type not supported.");
                    break;
            }
        }

        private void SetupShapeFiles(IEnumerable<string> shapefiles)
        {
            ImportControl = new ImportControl(
                        new TtShapeFileDataAccessLayer(
                            new TtShapeFileDataAccessLayer.ParseOptions(shapefiles, _Manager.DefaultMetadata.Zone)
                        )
                    );
            IsSettingUp = true;
        }

        public void ImportData()
        {
            IsSettingUp = false;
            IsImporting = true;

            try
            {
                Import.DAL(_Manager, ImportControl.DAL, ImportControl.SelectedPolygons,
                        ImportControl.IncludeMetadata, ImportControl.IncludeGroups, ImportControl.IncludeNmea);

                MessageBox.Show($"{ImportControl.SelectedPolygons.Count()} Polygons Imported",
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
