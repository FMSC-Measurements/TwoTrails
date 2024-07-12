using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwoTrails.Utils;

using WF = System.Windows.Forms;

namespace TwoTrails.ViewModels
{
    public class ExportProjectModel : BaseModel
    {
        private readonly TtProject _Project;
        private readonly Window _Window;
        private readonly MainWindowModel _MainModel;

        public string FolderLocation { get { return Get<string>(); } set { Set(value); } }

        public ICommand BrowseCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand CheckAllCommand { get; }

        public bool? IsCheckAll { get { return Get<bool?>(); } set { Set(value); } }

        public bool ExportPoints { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportDataDictionary { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportNMEA { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportPolygons { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportMetadata { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportGroups { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportSummary { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportProject { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportMediaInfo { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportKMZ { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportGPX { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportShapes { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportMediaFiles { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }


        public bool DataDictionaryEnabled { get { return _Project.HistoryManager.HasDataDictionary; } }


        public ExportProjectModel(Window window, TtProject project, MainWindowModel mainWindowModel)
        {
            this._Window = window;
            this._Project = project;
            _MainModel = mainWindowModel;

            FolderLocation = Path.GetDirectoryName(project.FilePath);

            BrowseCommand = new RelayCommand(x => BrowseFolder());
            ExportCommand = new BindedRelayCommand<ExportProjectModel>(
                x => CreateFiles(), x => IsCheckAll != false,
                this, m => m.IsCheckAll);
            CancelCommand = new RelayCommand(x => Cancel());
            CheckAllCommand = new RelayCommand(x => CheckAll((bool?)x));

            IsCheckAll = true;
            CheckAll(true);

            ExportMediaFiles = false;
        }

        private void BrowseFolder()
        {
            WF.FolderBrowserDialog dialog = new WF.FolderBrowserDialog();

            if (dialog.ShowDialog() == WF.DialogResult.OK)
            {
                FolderLocation = dialog.SelectedPath;
            }
        }

        private void CreateFiles()
        {
            if (IsCheckAll != false)
            {
                if (!DataHelper.IsFileOnStableMedia(FolderLocation))
                {
                    if (MessageBox.Show($@"Export folder is not located locally and may be on a network drive. Files not located locally may have issues loading and saving. It is suggested to copy the file locally before opening. Would you like to open the file from this location anyway?",
                        "Folder Located on Network", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
                        return;
                }

                if (Directory.Exists(FolderLocation))
                {
                    string path = Path.Combine(FolderLocation, Path.GetFileNameWithoutExtension(_Project.DAL.FilePath)).Trim();

                    int pathLen = path.Length;
                    List<String> tlPolyNames = _Project.HistoryManager.Polygons
                        .Where(p => (pathLen + p.Name.Length * 2 + 20) >= byte.MaxValue) //max possible length for generated path
                        .Select(p => p.Name).ToList();

                    if (tlPolyNames.Count > 0)
                    {
                        MessageBox.Show(
                            $"The following Polygon names are too long to export:\n\n{String.Join(", ", tlPolyNames)}\n\nPlease rename the polygons or choose a shallower path to place the export.",
                            "Polygon Names Too Long", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (Directory.Exists(path))
                    {
                        if(MessageBox.Show("An export already exists, would you like to overwrite the existing file(s)?",
                            "Overwrite Export", MessageBoxButton.YesNo, MessageBoxImage.Hand)
                            == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    try
                    {
                        if (IsCheckAll == true)
                        {
                            Export.All(_Project.HistoryManager, _Project.MAL, _Project.ProjectInfo, _Project.FilePath, path);
                        }
                        else
                        {
                            Export.CheckCreateFolder(path);

                            string projectName = _Project.ProjectName.ScrubFileName().Trim();

                            if (ExportPoints)
                                Export.Points(_Project.HistoryManager, Path.Combine(path, "Points"));

                            if (ExportDataDictionary)
                                Export.DataDictionary(_Project.HistoryManager, Path.Combine(path, "DataDictionary"));

                            if (ExportNMEA)
                                Export.TtNmea(_Project.HistoryManager, Path.Combine(path, "TTNmea"));

                            if (ExportPolygons)
                                Export.Polygons(_Project.HistoryManager, Path.Combine(path, "Polygons"));

                            if (ExportMetadata)
                                Export.Metadata(_Project.HistoryManager, Path.Combine(path, "Metadata"));

                            if (ExportGroups)
                                Export.Groups(_Project.HistoryManager, Path.Combine(path, "Groups"));

                            if (ExportMediaInfo && _Project.MAL != null)
                                Export.ImageInfo(_Project.HistoryManager, Path.Combine(path, "ImageInfo"));

                            if (ExportMediaFiles && _Project.MAL != null)
                                Export.MediaFiles(_Project.MAL, path);

                            if (ExportProject)
                                Export.Project(_Project.ProjectInfo, Path.Combine(path, "ProjectInfo"));

                            if (ExportSummary)
                                Export.Summary(_Project.HistoryManager, _Project.ProjectInfo, _Project.FilePath, Path.Combine(path, "Summary"));

                            if (ExportGPX)
                                Export.GPX(_Project.HistoryManager, _Project.ProjectInfo, Path.Combine(path, projectName));

                            if (ExportKMZ)
                                Export.KMZ(_Project.HistoryManager, _Project.ProjectInfo, Path.Combine(path, projectName));

                            if (ExportShapes)
                                Export.Shapes(_Project.HistoryManager, _Project.ProjectInfo, path);
                        }

                        _Window.Close();

                        _MainModel.PostMessage($"Project Exported to: '{path}'");

                        if (_Project.Settings.OpenFolderOnExport)
                        {
                            Process.Start(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"{ ex.Message }\n\t{ ex.StackTrace }", "ExportProjectModel");
                        MessageBox.Show("An error has occured. Please see log for details.");
                    }
                }
                else
                    MessageBox.Show("Invalid Folder Location"); 
            }
        }

        private void Cancel()
        {
            _Window.Close();
        }

        private void CheckAll(bool? isChecked)
        {
            ExportPoints = ExportDataDictionary = ExportNMEA = ExportPolygons = ExportMetadata = ExportGroups =
            ExportSummary = ExportProject = ExportKMZ = ExportGPX = ExportShapes = ExportMediaInfo = ExportMediaFiles =
                (isChecked == true);
        }

        private void CheckChanged()
        {
            if (ExportPoints && ExportPolygons && ExportMetadata && ExportGroups && ExportNMEA &&
                ExportSummary && ExportProject && ExportKMZ && ExportGPX && ExportShapes &&
                ExportMediaInfo && ExportMediaFiles && (!DataDictionaryEnabled || ExportDataDictionary))
            {
                IsCheckAll = true; 
            }
            else if (ExportPoints || ExportPolygons || ExportMetadata || ExportGroups || ExportNMEA ||
                ExportSummary || ExportProject || ExportKMZ || ExportGPX || ExportShapes ||
                ExportMediaInfo || ExportMediaFiles || ExportDataDictionary)
            {
                IsCheckAll = null;
            }
            else
            {
                IsCheckAll = false;
            }
        }
    }
}
