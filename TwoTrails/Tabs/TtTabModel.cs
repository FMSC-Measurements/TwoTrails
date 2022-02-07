using FMSC.Core.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public abstract class TtTabModel : BaseModel
    {
        public ICommand CloseTabCommand { get; }
        public virtual ICommand OpenInWinndowCommand { get; } = null;

        protected MainWindowModel MainModel { get; }

        public TabItem Tab { get; private set; }

        public abstract String TabTitle { get; }

        public abstract bool IsDetachable { get; }

        public virtual String TabInfo => String.Empty;

        public virtual String ToolTip => String.Empty;


        public TtTabModel(MainWindowModel mainWindowModel) : base()
        {
            this.Tab = new TabItem();
            MainModel = mainWindowModel;

            CloseTabCommand = new RelayCommand(x => CloseTab());

            Tab.DataContext = this;
        }


        public void CloseTab()
        {
            OnTabClose();
            MainModel.RemoveTab(this);
        }

        protected abstract void OnTabClose();
    }
}
