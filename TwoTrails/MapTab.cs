using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Commands;

namespace TwoTrails
{
    public class MapTab : TtTabModel
    {
        public override bool IsDetachable
        {
            get { return !IsDetached; }
        }

        private bool _IsDetached;
        public bool IsDetached
        {
            get { return _IsDetached; }
            set { SetField(ref _IsDetached, value, () => OnPropertyChanged(nameof(IsDetachable))); }
        }

        public override string TabTitle
        {
            get { return String.Format("(Map){0}", base.TabTitle); }
        }
       

        public MapTab(MainWindowModel mainModel, TtProject project, ProjectControl projectControl) : base(mainModel, project)
        {

        }

        protected override void CloseTab()
        {
            throw new NotImplementedException();
        }

        protected override void SaveProject()
        {
            throw new NotImplementedException();
        }
    }
}
