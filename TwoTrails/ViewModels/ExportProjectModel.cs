using CSUtil.ComponentModel;
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
    public class ExportProjectModel : NotifyPropertyChangedEx
    {
        private TtProject project;
        private Window window;

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


        public bool DataDictionaryEnabled { get { return project.Manager.HasDataDictionary; } }


        public ExportProjectModel(Window window, TtProject project)
        {
            this.window = window;
            this.project = project;
            FolderLocation = Path.GetDirectoryName(project.FilePath);

            BrowseCommand = new RelayCommand(x => BrowseFolder());
            ExportCommand = new BindedRelayCommand<ExportProjectModel>(x => CreateFiles(), x => IsCheckAll != false, this, x => x.IsCheckAll);
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
                    string path = Path.Combine(FolderLocation, project.ProjectName.Trim()).Trim();

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
                            Export.All(project.Manager, project.MAL, project.ProjectInfo, project.FilePath, path);
                        }
                        else
                        {
                            Export.CheckCreateFolder(path);

                            if (ExportPoints)
                                Export.Points(project.Manager, Path.Combine(path, "Points.csv"));

                            if (ExportDataDictionary)
                                Export.DataDictionary(project.Manager, Path.Combine(path, "DataDictionary.csv"));

                            if (ExportNMEA)
                                Export.TtNmea(project.Manager, Path.Combine(path, "Nmea.csv"));

                            if (ExportPolygons)
                                Export.Polygons(project.Manager, Path.Combine(path, "Polygons.csv"));

                            if (ExportMetadata)
                                Export.Metadata(project.Manager, Path.Combine(path, "Metadata.csv"));

                            if (ExportGroups)
                                Export.Groups(project.Manager, Path.Combine(path, "Groups.csv"));

                            if (ExportMediaInfo && project.MAL != null)
                                Export.ImageInfo(project.Manager, Path.Combine(path, "ImageInfo.csv"));

                            if (ExportMediaFiles && project.MAL != null)
                                Export.MediaFiles(project.MAL, path);

                            if (ExportProject)
                                Export.Project(project.ProjectInfo, Path.Combine(path, "ProjectInfo.txt"));

                            if (ExportSummary)
                                Export.Summary(project.Manager, project.ProjectInfo, project.FilePath, Path.Combine(path, "Summary.txt"));

                            if (ExportGPX)
                                Export.GPX(project.Manager, project.ProjectInfo, Path.Combine(path, $"{project.ProjectName.Trim()}.gpx"));

                            if (ExportKMZ)
                                Export.KMZ(project.Manager, project.ProjectInfo, Path.Combine(path, $"{project.ProjectName.Trim()}.kmz"));

                            if (ExportShapes)
                                Export.Shapes(project.Manager, project.ProjectInfo, Path.Combine(path));
                        }

                        window.Close();


                        //TODO notice of export
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
            window.Close();
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
