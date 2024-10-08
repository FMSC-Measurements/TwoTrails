﻿using FMSC.Core.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using TwoTrails.Core;
using TwoTrails.DAL;
using TwoTrails.Settings;
using TwoTrails.Utils;

namespace TwoTrails.ViewModels
{
    public class TtProject : BaseModel
    {
        public event EventHandler<string> MessagePosted;

        #region Properties
        public String FilePath { get { return DAL.FilePath; } }
        
        private TtProjectInfo _ProjectInfo;
        public TtProjectInfo ProjectInfo { get; }
        
        public String ProjectName
        {
            get { return _ProjectInfo.Name; }
        }
        
        public bool RequiresSave
        {
            get { return Get<bool>() || ProjectChanged; }
            private set { Set(value); }
        }


        public bool RequiresUpgrade { get; private set; }

        //changes to project fields
        public bool ProjectChanged
        {
            get { return Get<bool>(); }
            private set { Set(value, () => OnPropertyChanged(nameof(RequiresSave))); }
        }


        public ITtDataLayer DAL { get; private set; }
        public ITtMediaLayer MAL { get; private set; }

        public TtSettings Settings { get; private set; }
        public ProjectSettings ProjectSettings { get; private set; }

        private TtManager Manager { get; }
        public TtHistoryManager HistoryManager { get; }
        #endregion


        public TtProject(ITtDataLayer dal, ITtMediaLayer mal, TtSettings settings)
        {
            DAL = dal;
            MAL = mal;
            Settings = settings;

            ProjectInfo = dal.GetProjectInfo();
            _ProjectInfo = new TtProjectInfo(ProjectInfo);

            ProjectInfo.PropertyChanged += ProjectInfoChanged;

            RequiresSave = false;

            Manager = new TtManager(dal, mal, settings);
            HistoryManager = new TtHistoryManager(Manager);
            HistoryManager.HistoryChanged += Manager_HistoryChanged;

            ProjectSettings = new ProjectSettings(this);
        }

        private void ProjectInfoChanged(object sender, PropertyChangedEventArgs e)
        {
            ProjectChanged = !_ProjectInfo.Equals(ProjectInfo);
        }

        private void Manager_HistoryChanged(object sender, HistoryEventArgs e)
        {
            RequiresSave = HistoryManager.CanUndo;

            String eTypeStr;

            switch (e.HistoryEventType)
            {
                case HistoryEventType.Undone: eTypeStr = "Undone: "; break;
                case HistoryEventType.Redone: eTypeStr = "Redone: "; break;
                case HistoryEventType.Commit:
                default: eTypeStr = String.Empty; break;
            }

            MessagePosted?.Invoke(this, $"{eTypeStr}{e.CommandInfo.Description}");
        }

        public bool Save()
        {
            if (RequiresSave)
            {
                try
                {
                    if (ProjectChanged)
                    {
                        ProjectInfo.Name = ProjectInfo.Name.Trim();
                        DAL.UpdateProjectInfo(ProjectInfo);
                        _ProjectInfo = new TtProjectInfo(ProjectInfo);
                    }

                    Manager.Save();
                    HistoryManager.ClearHistory();
                    RequiresSave = ProjectChanged = false;
                    MessagePosted?.Invoke(this, $"Project '{ProjectName}' Saved");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message, "TtProject:Save");
                    MessagePosted?.Invoke(this, "Error Saving Project");
                    MessageBox.Show("Error Saving Project. See Log for details.", "Project Save Error");
                    return false;
                }
            }

            return true;
        }

        public void ReplaceDAL(ITtDataLayer dal)
        {
            DAL = dal;
            Manager.ReplaceDAL(DAL);
            OnPropertyChanged(nameof(DAL), nameof(FilePath));
        }

        public void ReplaceMAL(ITtMediaLayer mal)
        {
            MAL = mal;
            Manager.ReplaceMAL(MAL);
            OnPropertyChanged(nameof(MAL));
        }

        public List<TtPolygon> GetSortedPolygons() =>
            HistoryManager.Polygons.Sort(Settings.SortPolysByName);
    }
}
