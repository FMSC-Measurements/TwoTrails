using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FMSC.Core.Controls
{
    public delegate void ListChangedEvent(IList items);

    public class DataGridEx : DataGrid
    {
        public event ListChangedEvent SelectedItemListChanged;
        public event ListChangedEvent VisibleItemListChanged;

        public event EventHandler Sorted;

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

        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionUpdated;


        public ObservableCollection<DataGridColumn> BindableColumns
        {
            get { return (ObservableCollection<DataGridColumn>)GetValue(BindableColumnsProperty); }
            set { SetValue(BindableColumnsProperty, value); }
        }
        
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.Register("BindableColumns",
                typeof(ObservableCollection<DataGridColumn>),
                typeof(DataGridEx),
                new UIPropertyMetadata(null, BindableColumnsPropertyChanged)
            );

        private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = source as DataGrid;
            ObservableCollection<DataGridColumn> columns = e.NewValue as ObservableCollection<DataGridColumn>;

            dataGrid.Columns.Clear();

            if (columns == null)
                return;

            foreach (DataGridColumn column in columns)
                dataGrid.Columns.Add(column);

            columns.CollectionChanged += (sender, e2) =>
            {
                NotifyCollectionChangedEventArgs ne = e2 as NotifyCollectionChangedEventArgs;
                if (ne.Action == NotifyCollectionChangedAction.Reset)
                {
                    dataGrid.Columns.Clear();
                    foreach (DataGridColumn column in ne.NewItems)
                    {
                        dataGrid.Columns.Add(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (DataGridColumn column in ne.NewItems)
                    {
                        dataGrid.Columns.Add(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Move)
                {
                    dataGrid.Columns.Move(ne.OldStartingIndex, ne.NewStartingIndex);
                }
                else if (ne.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (DataGridColumn column in ne.OldItems)
                    {
                        dataGrid.Columns.Remove(column);
                    }
                }
                else if (ne.Action == NotifyCollectionChangedAction.Replace)
                {
                    dataGrid.Columns[ne.NewStartingIndex] = ne.NewItems[0] as DataGridColumn;
                }
            };
        }
        



        public DataGridEx()
        {
            this.SelectionChanged += DataGridEx_SelectionChanged;
            this.SourceUpdated += DataGridEx_SourceUpdated;
            this.ItemContainerGenerator.ItemsChanged += ItemContainerGenerator_ItemsChanged;

            var dpd = DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            if (dpd != null)
            {
                dpd.AddValueChanged(this, OnSourceChanged);
            }
        }

        private void OnSourceChanged(object sender, EventArgs e)
        {
            CollectionView cv = this.ItemsSource as CollectionView;

            if (cv != null)
                ((INotifyCollectionChanged)cv).CollectionChanged += DataGridEx_CollectionChanged;
        }

        private void DataGridEx_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionUpdated?.Invoke(sender, e);
        }

        private void ItemContainerGenerator_ItemsChanged(object sender, System.Windows.Controls.Primitives.ItemsChangedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            this.VisibleItemsList = this.ItemContainerGenerator.Items;
            VisibleItemListChanged?.Invoke(VisibleItemsList);
        }

        private void DataGridEx_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            this.VisibleItemsList = this.ItemContainerGenerator.Items;
            VisibleItemListChanged?.Invoke(VisibleItemsList);
        }

        void DataGridEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItemsList = this.SelectedItems;
            SelectedItemListChanged?.Invoke(SelectedItemsList);
        }


        protected override void OnSorting(DataGridSortingEventArgs e)
        {
            base.OnSorting(e);

            OnSorted();
        }

        protected void OnSorted()
        {
            Sorted?.Invoke(this, new EventArgs());
        }



    }
}
