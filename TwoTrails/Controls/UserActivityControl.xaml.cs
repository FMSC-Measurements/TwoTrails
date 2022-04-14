using FMSC.Core.Windows.ComponentModel.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for UserActivityControl.xaml
    /// </summary>
    public partial class UserActivityControl : UserControl
    {
        private List<TtUserAction> _Actions;
        public ListCollectionView Actions { get; }

        public ICommand RefreshCommand { get; }

        private ITtManager Manager;


        public UserActivityControl(ITtManager manager)
        {
            Manager = manager;
            _Actions = Manager.GetUserActions();
            Actions = CollectionViewSource.GetDefaultView(_Actions) as ListCollectionView;
            Actions.CustomSort = new TtUserActionSorter();

            this.DataContext = this;

            RefreshCommand = new RelayCommand(x => RefreshActions());

            InitializeComponent();
        }

        private void RefreshActions()
        {
            _Actions.Clear();
            _Actions.AddRange(Manager.GetUserActions());
            Actions.Refresh();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == nameof(TtUserAction.Action))
                e.Cancel = true;
        }
    }
}
