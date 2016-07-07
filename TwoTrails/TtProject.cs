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


        private ITtDataLayer DAL;


        public TtProject(ITtDataLayer dal)
        {
            DAL = dal;

            if (dal.RequiresUpgrade)
            {
                //ask to upgrade
            }
            else
            {
                ProjectInfo = dal.GetProjectInfo();

                ProjectName = ProjectInfo.Name;
            }
        }




        public void Save()
        {
            //TODO save info to dal
        }



        public static TtProject Create(String filePath, TtProjectInfo projInfo)
        {
            return new TtProject(TtSqliteDataAccessLayer.Create(filePath, projInfo));
        }
    }
}
