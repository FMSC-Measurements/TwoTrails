using CSUtil.ComponentModel;
using FMSC.Core.Windows.ComponentModel.Commands;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using TwoTrails.ViewModels;

namespace TwoTrails
{
    public abstract class TtTabModel : NotifyPropertyChangedEx
    {
        public ICommand CloseTabCommand { get; }
        public virtual ICommand OpenInWinndowCommand { get; } = null;


        public TabItem Tab { get; private set; }

        public abstract String TabTitle { get; }

        public abstract bool IsDetachable { get; }

        public virtual String TabInfo => String.Empty;

        public virtual String ToolTip => String.Empty;


        public TtTabModel() : base()
        {
            this.Tab = new TabItem();
            
            CloseTabCommand = new RelayCommand(x => OnTabClose());

            Tab.DataContext = this;
        }


        public void CloseTab() => OnTabClose();

        protected abstract void OnTabClose();

        protected override void Dispose(bool dispoing)
        {
            base.Dispose(dispoing);

            Tab.DataContext = null;
        }
    }
}
