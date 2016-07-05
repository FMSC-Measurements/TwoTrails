using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoTrails.Commands;

namespace TwoTrails
{
    public class MapTab : TtTabItem
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

        public override ICommand Close { get; }
        public override ICommand Save { get; }

        public MapTab(MainWindowModel mainModel, TtProject project, ProjectControl projectControl) : base(mainModel, project)
        {
            Save = new SaveProjectCommand();
            Close = new CloseProjectCommand(mainModel);
        }

    }
}
