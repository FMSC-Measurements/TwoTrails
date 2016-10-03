﻿using CSUtil.ComponentModel;
using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool ExportPolygons { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportMetadata { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportGroups { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportSummary { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportProject { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportKMZ { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportGPX { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }
        public bool ExportShapes { get { return Get<bool>(); } set { Set(value, () => CheckChanged()); } }

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

                    try
                    {
                        if (IsCheckAll == true)
                        {
                            Export.All(project, path);
                        }
                        else
                        {
                            if (ExportPoints)
                                Export.Points(project.Manager, Path.Combine(path, "Points.csv"));

                            if (ExportPolygons)
                                Export.Polygons(project.Manager, Path.Combine(path, "Polygons.csv"));

                            if (ExportMetadata)
                                Export.Metadata(project.Manager, Path.Combine(path, "Metadata.csv"));

                            if (ExportGroups)
                                Export.Groups(project.Manager, Path.Combine(path, "Groups.csv"));

                            if (ExportProject)
                                Export.Project(project.ProjectInfo, Path.Combine(path, "ProjectInfo.txt"));

                            if (ExportSummary)
                                Export.Summary(project.Manager, Path.Combine(path, "Summary.txt"));

                            if (ExportGPX)
                                Export.GPX(project, Path.Combine(path, String.Format("{0}.gpx", project.ProjectName.Trim())));

                            if (ExportKMZ)
                                Export.KMZ(project, Path.Combine(path, String.Format("{0}.kmz", project.ProjectName.Trim())));

                            if (ExportShapes)
                                Export.Shapes(project, Path.Combine(path));
                        }

                        window.Close();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
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
            ExportPoints = ExportPolygons = ExportMetadata = ExportGroups =
            ExportSummary = ExportProject = ExportKMZ = ExportGPX = ExportShapes =
                (isChecked == true);
        }

        private void CheckChanged()
        {
            if (ExportPoints && ExportPolygons && ExportMetadata && ExportGroups &&
                ExportSummary && ExportProject && ExportKMZ && ExportGPX && ExportShapes)
            {
                IsCheckAll = true; 
            }
            else if (ExportPoints || ExportPolygons || ExportMetadata || ExportGroups ||
                ExportSummary || ExportProject || ExportKMZ || ExportGPX || ExportShapes)
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