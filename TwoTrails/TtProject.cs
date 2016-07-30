using CSUtil.ComponentModel;
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
            set { Set(value); }
        }
        
        public bool RequiresSave
        {
            get { return Get<bool>(); }
            set { Set(value); }
        }


        public bool RequiresUpgrade { get; private set; }


        public ITtDataLayer DAL { get; private set; }

        public TtHistoryManager Manager { get; private set; }


        public TtProject(ITtDataLayer dal, ITtSettings settings)
        {
            DAL = dal;

            ProjectInfo = dal.GetProjectInfo();
            ProjectName = ProjectInfo.Name;

            RequiresSave = false;

            Manager = new TtHistoryManager(new TtManager(dal, settings));
            Manager.HistoryChanged += Manager_HistoryChanged;
        }

        private void Manager_HistoryChanged(object sender, EventArgs e)
        {
            Set(Manager.CanUndo, nameof(RequiresSave));
        }

        public void Save()
        {
            if (RequiresSave)
            {
                //TODO save info to dal
            }
        } 
    }
}
