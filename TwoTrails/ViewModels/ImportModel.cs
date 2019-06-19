﻿using CSUtil.ComponentModel;
using FMSC.Core.Utilities;
using FMSC.Core.Windows.ComponentModel.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.Controls;
using TwoTrails.Core;
using TwoTrails.Core.Interfaces;
using TwoTrails.Core.Points;
using TwoTrails.DAL;
using TwoTrails.Utils;

namespace TwoTrails.ViewModels
{
    public class ImportModel : NotifyPropertyChangedEx
    {
        public ICommand BrowseFileCommand { get; }

        private ICommand _ImportCommand;
        public ICommand ImportCommand => (MainContent is IFileParseControl fpc) ? fpc.Model.SetupImportCommand : _ImportCommand;

        public String ImportBtnText => (MainContent is IFileParseControl) ? "Continue" : "Import";

        public ICommand CancelCommand { get; }

        private Window _Window;
        private ITtManager _Manager => _Project.Manager;
        private TtProject _Project;

        public Control MainContent
        {
            get => Get<Control>();
            set => Set(value, () => OnPropertyChanged(nameof(HasMainContent), nameof(CloseText), nameof(ImportCommand), nameof(ImportBtnText)));
        }

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

        private bool _AutoCloseOnImport;


        public ImportModel(Window window, TtProject project, string fileName = null, bool autoCloseOnImport = false)
        {
            _Window = window;
            _Project = project;
            MainContent = null;
            CurrentFile = null;

            BrowseFileCommand = new BindedRelayCommand<ImportModel>(x => BrowseFile(), x => !IsImporting,
                this, m => m.IsImporting);

            _ImportCommand = new BindedRelayCommand<ImportModel>(x => ImportData(), x => CanImport, this, m => m.CanImport);

            CancelCommand = new RelayCommand(x => Cancel());

            if (fileName != null)
                SetupImport(fileName);

            _AutoCloseOnImport = true;
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
            if (FileUtils.IsFileLocked(fileName))
            {
                MessageBox.Show($"File '{Path.GetFileName(fileName)}' is currently locked. Make sure it is not in use by any other programs.");
            }
            else
            {
                switch (Path.GetExtension(fileName).ToLower())
                {
                    case Consts.FILE_EXTENSION:
                        IsSettingUp = true;

                        TtSqliteDataAccessLayer idal = new TtSqliteDataAccessLayer(fileName);
                        
                        if (!TtUtils.CheckAndFixErrors(idal))
                        {
                            IsSettingUp = false;
                            return;
                        }

                        if (idal.GetDataVersion() < TwoTrailsSchema.SchemaVersion)
                        {
                            if (MessageBox.Show("The importing file needs to be upgraded before import. Do you want to upgrade it now?",
                                "File Requires Upgrade", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                if (!Upgrade.DAL(idal, _Project.Settings))
                                {
                                    MessageBox.Show("File Failed to Upgrade. See Log File for details.");
                                    IsSettingUp = false;
                                    return;
                                }
                            }
                            else
                            {
                                IsSettingUp = false;
                                return;
                            }
                        }

                        ImportControl = new ImportControl(idal, true, true, true);
                        break;
                    case Consts.FILE_EXTENSION_V2:
                        IsSettingUp = true;
                        ImportControl = new ImportControl(new TtV2SqliteDataAccessLayer(fileName), true, true, true);
                        break;
                    case Consts.CSV_EXT:
                    case Consts.TEXT_EXT:
                        IsSettingUp = true;
                        try
                        {
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
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error try to parse CSV file.");
                            Trace.WriteLine("ImportModel:SetupImport:CSV", ex.Message);
                        }
                        break;
                    case Consts.GPX_EXT:
                        IsSettingUp = true;
                        try
                        {
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
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error try to parse GPX file.");
                            Trace.WriteLine("ImportModel:SetupImport:GPX", ex.Message);
                        }
                        break;
                    case Consts.KML_EXT:
                    case Consts.KMZ_EXT:
                        {
                            try
                            {
                                ImportControl = new ImportControl(
                                    new TtKmlDataAccessLayer(
                                        new TtKmlDataAccessLayer.ParseOptions(fileName, _Manager.DefaultMetadata.Zone, true, startPolyNumber: _Manager.PolygonCount)
                                ), false, false, false);

                                IsSettingUp = true;
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex.Message, "ImportModel:SetupImport:KMZ");
                                MessageBox.Show(ex.Message, "Google Earth File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                        }
                    case Consts.SHAPE_EXT:
                    case Consts.SHAPE_PRJ_EXT:
                    case Consts.SHAPE_SHX_EXT:
                    case Consts.SHAPE_DBF_EXT:
                        switch (TtShapeFileDataAccessLayer.ValidateShapePackage(fileName, _Manager.DefaultMetadata.Zone))
                        {
                            case TtShapeFileDataAccessLayer.ShapeFileValidityResult.Valid:
                                {
                                    ImportControl = new ImportControl(
                                        new TtShapeFileDataAccessLayer(
                                            new TtShapeFileDataAccessLayer.ParseOptions(fileName, _Manager.DefaultMetadata.Zone, _Manager.PolygonCount)
                                        ), false, false, false
                                    );
                                    IsSettingUp = true;
                                    break;
                                }
                            case TtShapeFileDataAccessLayer.ShapeFileValidityResult.MissingFiles:
                                MessageBox.Show("Missing one many of the 'shp', 'prj', 'dbf', or 'shx' files.");
                                break;
                            case TtShapeFileDataAccessLayer.ShapeFileValidityResult.NotNAD83:
                                MessageBox.Show("Shape File is not in the NAD83 format.");
                                break;
                            case TtShapeFileDataAccessLayer.ShapeFileValidityResult.MismatchZone:
                                MessageBox.Show($"Shape File is not in zone {_Manager.DefaultMetadata.Zone}.");
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        MessageBox.Show("File type not supported.");
                        break;
                }
            }
        }

        public void SetupShapeFiles(IEnumerable<string> shapefiles)
        {
            if (shapefiles.All(f => !FileUtils.IsFileLocked(f)))
            {
                List<TtShapeFileDataAccessLayer.ShapeFileValidityResult> invalids =
                        shapefiles.Select(sf => TtShapeFileDataAccessLayer.ValidateShapePackage(sf, _Manager.DefaultMetadata.Zone))
                        .Where(r => r != TtShapeFileDataAccessLayer.ShapeFileValidityResult.Valid).ToList();

                if (invalids.Count > 0)
                {
                    if (invalids.Contains(TtShapeFileDataAccessLayer.ShapeFileValidityResult.MissingFiles))
                        MessageBox.Show("Missing one many of the 'shp', 'prj', 'dbf', or 'shx' files.");
                    else if (invalids.Contains(TtShapeFileDataAccessLayer.ShapeFileValidityResult.NotNAD83))
                        MessageBox.Show("Shape File is not in the NAD83 format.");
                    else if (invalids.Contains(TtShapeFileDataAccessLayer.ShapeFileValidityResult.MismatchZone))
                        MessageBox.Show($"Shape File is not in zone {_Manager.DefaultMetadata.Zone}.");
                    else
                        MessageBox.Show("Issue with shapefile(s).");
                }
                else
                {
                    ImportControl = new ImportControl(
                                new TtShapeFileDataAccessLayer(
                                    new TtShapeFileDataAccessLayer.ParseOptions(shapefiles, _Manager.DefaultMetadata.Zone, _Manager.PolygonCount)
                                )
                            );
                    IsSettingUp = true;
                } 
            }
            else
            {
                MessageBox.Show("One or more of the shapefiles is locked. Make sure they are not in use by any other programs.");
            }
        }

        public void ImportData()
        {
            IEnumerable<string> selectedPolys = ImportControl.SelectedPolygons;
            bool convertForeignQuondams = false;
            
            if (ImportControl.DAL.HandlesAllPointTypes)
            {
                List<string> neededPolys = new List<string>();

                foreach (string polyCN in selectedPolys)
                {
                    IEnumerable<TtPoint> points = ImportControl.DAL.GetPoints(polyCN, true).Where(p => p.OpType == OpType.Quondam);

                    foreach (QuondamPoint qp in ImportControl.DAL.GetPoints(polyCN, true)
                        .Where(p => p.OpType == OpType.Quondam).Cast<QuondamPoint>())
                    {
                        if (qp.ParentPoint.PolygonCN != polyCN && !selectedPolys.Contains(qp.ParentPoint.PolygonCN) && !neededPolys.Contains(qp.ParentPoint.PolygonCN))
                            neededPolys.Add(qp.ParentPoint.PolygonCN);
                    }
                }
                
                if (neededPolys.Count > 0)
                {
                    MessageBoxResult res = MessageBox.Show("Some quondams are linked to points that are not within the list of improted polygons. Would you like to import these polygons (YES) or convert the points to GPS (NO)?", "Foreign Quondams",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        selectedPolys = selectedPolys.Concat(neededPolys);
                    }
                    else if (res == MessageBoxResult.No)
                    {
                        convertForeignQuondams = true;
                    }
                    else
                        return;
                }
            }

            IsSettingUp = false;
            IsImporting = true;

            try
            {
                Import.DAL(_Manager, ImportControl.DAL, selectedPolys, ImportControl.IncludeMetadata,
                    ImportControl.IncludeGroups, ImportControl.IncludeNmea, convertForeignQuondams);

                MessageBox.Show($"{selectedPolys.Count()} Polygons Imported",
                    String.Empty, MessageBoxButton.OK, MessageBoxImage.None);
            }
            catch (Exception ex) when (!_AutoCloseOnImport)
            {
                Trace.WriteLine(ex.Message, "ImportModel:ImportData");
                MessageBox.Show("Import Failed. See log file for details.", String.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            IsImporting = false;
            ImportControl = null;

            if (_AutoCloseOnImport)
                _Window.Close();
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
