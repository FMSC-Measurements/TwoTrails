﻿using CSUtil.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails
{
    public class TtProject : NotifyPropertyChangedEx
    {
        public String FilePath { get { return DAL.FilePath; } }


        public TtProjectInfo ProjectInfo { get; private set; }
        
        public String ProjectName
        {
            get { return Get<String>(); }
            private set { Set(value); }
        }
        
        public bool RequiresSave
        {
            get { return Get<bool>(); }
            private set { Set(value); }
        }


        public bool RequiresUpgrade { get; private set; }


        public bool DataChanged { get; private set; }


        public ITtDataLayer DAL { get; private set; }

        private TtManager _Manager;
        public TtHistoryManager HistoryManager { get; private set; }


        public TtProject(ITtDataLayer dal, ITtSettings settings)
        {
            DAL = dal;

            ProjectInfo = dal.GetProjectInfo();
            ProjectName = ProjectInfo.Name;

            RequiresSave = false;

            _Manager = new TtManager(dal, settings);
            HistoryManager = new TtHistoryManager(_Manager);
            HistoryManager.HistoryChanged += Manager_HistoryChanged;
        }

        private void Manager_HistoryChanged(object sender, EventArgs e)
        {
            Set(HistoryManager.CanUndo || DataChanged, nameof(RequiresSave));
        }

        public void Save()
        {
            if (RequiresSave)
            {
                try
                {
                    _Manager.Save();
                    RequiresSave = DataChanged = false;
                }
                catch (Exception ex)
                {
                    //Unable to save
                }
            }
        } 
    }
}
