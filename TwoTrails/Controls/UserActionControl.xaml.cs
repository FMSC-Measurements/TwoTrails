﻿using FMSC.Core.ComponentModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TwoTrails.Core;
using TwoTrails.DAL;

namespace TwoTrails.Controls
{
    /// <summary>
    /// Interaction logic for UserActivityControl.xaml
    /// </summary>
    public partial class UserActivityControl : UserControl
    {
        private List<TtUserAction> _Activites;
        public ListCollectionView Activites { get; }

        public ICommand RefreshCommand { get; }

        private ITtDataLayer DAL;

        public UserActivityControl(ITtDataLayer dal)
        {
            DAL = dal;
            _Activites = DAL.GetUserActivity().ToList();
            Activites = CollectionViewSource.GetDefaultView(_Activites) as ListCollectionView;

            this.DataContext = this;

            RefreshCommand = new RelayCommand(x => RefreshActivites());

            InitializeComponent();
        }

        private void RefreshActivites()
        {
            _Activites.Clear();
            _Activites.AddRange(DAL.GetUserActivity());
            Activites.Refresh();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == nameof(TtUserAction.Action))
                e.Cancel = true;
        }
    }
}