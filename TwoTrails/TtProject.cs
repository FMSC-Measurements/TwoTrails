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
        public Guid ID { get; } = Guid.NewGuid();


        public TtProjectInfo ProjectInfo { get; private set; }
        
        private String _ProjectName;
        public String ProjectName
        {
            get { return _ProjectName; }
            set { SetField(ref _ProjectName, value); }
        }

        private bool _RequiresSave;
        public bool RequiresSave
        {
            get { return _RequiresSave; }
            set { SetField(ref _RequiresSave, value); }
        }


        public bool RequiresUpgrade { get; private set; }


        private TtSqliteDataAccessLayer DAL;


        public TtProject(TtSqliteDataAccessLayer dal)
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



        public static TtProject Create(String filePath, TtProjectInfo projInfo)
        {
            return new TtProject(TtSqliteDataAccessLayer.Create(filePath, projInfo));
        }
    }
}
