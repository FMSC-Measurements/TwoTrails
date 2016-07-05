using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwoTrails.Commands;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Dialogs;

namespace TwoTrails
{
    public class MainWindowModel
    {
        private Dictionary<Guid, ProjectTab> _ProjectTabs = new Dictionary<Guid, ProjectTab>();
        private Dictionary<Guid, MapTab> _MapTabs = new Dictionary<Guid, MapTab>();
        private Dictionary<Guid, MapWindow> _MapWindows = new Dictionary<Guid, MapWindow>();

        private Dictionary<Guid, TtProject> _Projects = new Dictionary<Guid, TtProject>();

        
        public TtSettings Settings { get; private set; }

        private MainWindow mainWindow;


        public MainWindowModel(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            Settings = new TtSettings(new DeviceSettings(), new MetadataSettings());
        }


        public void OpenProject(String filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    TtSqliteDataAccessLayer dal = new TtSqliteDataAccessLayer(filePath);

                    TtProject project = new TtProject(dal);

                    AddProject(project);
                }
                catch
                {

                }
            }
            else
            {
                MessageBox.Show("File not found");
            }
        }




        public void CreateProject()
        {
            NewProjectDialog dialog = new NewProjectDialog(Settings.CreateProjectInfo(AppInfo.Version));

            if (dialog.ShowDialog() == true)
            {
                TtSqliteDataAccessLayer dal = TtSqliteDataAccessLayer.Create(dialog.FilePath, dialog.ProjectInfo);
                TtProject proj = new TtProject(dal);

                AddProject(proj);
            }
        }

        private void AddProject(TtProject proj)
        {
            _Projects.Add(proj.ID, proj);
            _ProjectTabs.Add(proj.ID, new ProjectTab(this, proj));
        }

        public void SaveSettings()
        {

        }

        
        public void OpenMapTab(TtProject project)
        {

        }

        public void OpenMapWindow(TtProject project)
        {

        }


        public void CloseProject(TtProject project)
        {
            if (_ProjectTabs.ContainsKey(project.ID))
            {
                mainWindow.tabControl.Items.Remove(_ProjectTabs[project.ID].Tab);
                _ProjectTabs.Remove(project.ID);
            }
            
            CloseMap(project);

            _Projects.Remove(project.ID);
        }

        public void CloseMap(TtProject project)
        {
            if (_MapTabs.ContainsKey(project.ID))
            {

                mainWindow.tabControl.Items.Remove(_MapTabs[project.ID].Tab);
                _MapTabs.Remove(project.ID);
            }

            if (_MapWindows.ContainsKey(project.ID))
            {
                _MapWindows[project.ID].Close();
                _MapWindows.Remove(project.ID);
            }
        }
    }
}
