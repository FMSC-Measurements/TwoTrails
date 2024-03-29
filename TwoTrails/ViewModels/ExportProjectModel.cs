﻿using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using TwoTrails.Utils;

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
            //ExportCommand = new BindedRelayCommand<ExportProjectModel>(
            //    (x, m) => m.CreateFiles(), (x, m) => m.IsCheckAll != false,
            //    this, m => m.IsCheckAll);
            CancelCommand = new RelayCommand(x => Cancel());
            CheckAllCommand = new RelayCommand(x => CheckAll((bool?)x));

            IsCheckAll = true;
            CheckAll(true);

            ExportMediaFiles = false;
        }

        private void BrowseFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FolderLocation = dialog.SelectedPath;
            }
        }

        private void CreateFiles()
        {
            if (IsCheckAll != false)
            {
                if (Directory.Exists(FolderLocation))
                {
                    string path = Path.Combine(FolderLocation, Path.GetFileNameWithoutExtension(_Project.DAL.FilePath)).Trim();

                    if (Directory.Exists(path))
                    {
                        if(System.Windows.MessageBox.Show("An export already exists, would you like to overwrite the existing file(s)?",
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

                            if (ExportPoints)
                                Export.Points(_Project.HistoryManager, Path.Combine(path, "Points.csv"));

                            if (ExportDataDictionary)
                                Export.DataDictionary(_Project.HistoryManager, Path.Combine(path, "DataDictionary.csv"));

                            if (ExportNMEA)
                                Export.TtNmea(_Project.HistoryManager, Path.Combine(path, "TTNmea.csv"));

                            if (ExportPolygons)
                                Export.Polygons(_Project.HistoryManager, Path.Combine(path, "Polygons.csv"));

                            if (ExportMetadata)
                                Export.Metadata(_Project.HistoryManager, Path.Combine(path, "Metadata.csv"));

                            if (ExportGroups)
                                Export.Groups(_Project.HistoryManager, Path.Combine(path, "Groups.csv"));

                            if (ExportMediaInfo && _Project.MAL != null)
                                Export.ImageInfo(_Project.HistoryManager, Path.Combine(path, "ImageInfo.csv"));

                            if (ExportMediaFiles && _Project.MAL != null)
                                Export.MediaFiles(_Project.MAL, path);

                            if (ExportProject)
                                Export.Project(_Project.ProjectInfo, Path.Combine(path, "ProjectInfo.txt"));

                            if (ExportSummary)
                                Export.Summary(_Project.HistoryManager, _Project.ProjectInfo, _Project.FilePath, Path.Combine(path, "Summary.txt"));

                            if (ExportGPX)
                                Export.GPX(_Project.HistoryManager, _Project.ProjectInfo, Path.Combine(path, $"{_Project.ProjectName.Trim()}.gpx"));

                            if (ExportKMZ)
                                Export.KMZ(_Project.HistoryManager, _Project.ProjectInfo, Path.Combine(path, $"{_Project.ProjectName.Trim()}.kmz"));

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
                        System.Windows.MessageBox.Show("An error has occured. Please see log for details.");
                    }
                }
                else
                    System.Windows.MessageBox.Show("Invalid Folder Location"); 
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
