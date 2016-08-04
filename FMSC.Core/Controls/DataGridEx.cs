using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FMSC.Core.Controls
{
    public delegate void ListChangedEvent(IList items);

    public class DataGridEx : DataGrid
    {
        public event ListChangedEvent SelectedItemListChanged;
        public event ListChangedEvent VisibleItemListChanged;


        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
                DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(DataGridEx), new PropertyMetadata(null));

        public IList VisibleItemsList
        {
            get { return (IList)GetValue(VisibleItemsListProperty); }
            set { SetValue(VisibleItemsListProperty, value); }
        }

        public static readonly DependencyProperty VisibleItemsListProperty =
                DependencyProperty.Register("VisibleItemsList", typeof(IList), typeof(DataGridEx), new PropertyMetadata(null));


        public DataGridEx()
        {
            this.SelectionChanged += DataGridEx_SelectionChanged;
            this.SourceUpdated += DataGridEx_SourceUpdated;
            this.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;
        }

        private void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            this.VisibleItemsList = this.ItemContainerGenerator.Items;
            VisibleItemListChanged?.Invoke(VisibleItemsList);
        }

        private void DataGridEx_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            this.VisibleItemsList = this.ItemContainerGenerator.Items;
            VisibleItemListChanged?.Invoke(VisibleItemsList);
        }

        void DataGridEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItemsList = this.SelectedItems;
            SelectedItemListChanged?.Invoke(SelectedItemsList);
        }
    }
}
